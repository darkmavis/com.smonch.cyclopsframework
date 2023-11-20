// Cyclops Framework
// 
// Copyright 2010 - 2023 Mark Davis
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsRoutine : CyclopsCommon, ICyclopsDisposable, ICyclopsPausable, ICyclopsTaggable, ICyclopsRoutineScheduler
    {
        // ReSharper disable once InconsistentNaming - ReSharper isn't happy with standard C# naming conventions.
        private static CyclopsPool<CyclopsRoutine> s_pool;
        
        // This is important for in-editor situations where domain reloading is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => s_pool = new CyclopsPool<CyclopsRoutine>();
        
        private bool _canSyncAtStart;
        private bool _isPooled;
        private bool _isReadyToCallFirstFrameAgain;
        private bool _shouldStoppageCallLastFrame;

        private Func<CyclopsRoutine, bool> _stoppagePredicate;

        // Use linear easing by default: t => t;
        // Please note that all built-in easing methods make an attempt to benefit from: MethodImplOptions.AggressiveInlining
        // ReSharper disable once MemberCanBePrivate.Global
        protected Func<float, float> Ease { get; set; }
        
        /// <summary>
        /// This provides a way to gracefully handle failures such as timeouts when waiting for an event or polling for a particular state.
        /// </summary>
        private Action FailureHandler { get; set; }
        
        /// <summary>
        /// This tracks the nesting depth of a CyclopsRoutine that is immediately enqueued.
        /// It is used as a safety measure to prevent misuse and to warn developers of a potentially endless loop.
        /// This situation can only be triggered by immediately adding a CyclopsRoutine.
        /// Use of Next will never trigger this situation. Always use Next unless you know what you are doing.
        /// </summary>
        internal int NestingDepth { get; set; }

        protected bool MustRecycleIfPooled { get; set; } = true;

        /// <summary>
        /// This is used by CyclopsEngine and provides a way to skip adding this routine to the active queue if the specified condition is met.
        /// If needed, use CyclopsCommon.SkipIf to provide a condition.
        /// Any usage is external to CyclopsRoutine.
        /// </summary>
        public Func<bool> SkipPredicate { get; set; }

        public double Age { get; private set; }

        public double Period { get; protected set; }

        public double Cycle { get; private set; }

        public double MaxCycles { get; protected set; }

        public double Position { get => (((Age - Cycle) >= 1.0) ? 1.0 : (Age - Cycle)); }

        public double Speed { get; set; }

        // Pooled
        public IEnumerable<string> Tags { get; private set; }

        // Pooled
        public List<CyclopsRoutine> Children { get; private set; }

        /// <summary>
        /// This provides a reference to the CyclopsEngine host.
        /// </summary>
        internal CyclopsEngine Host { get; set; }

        public virtual bool IsPaused { get; set; }

        public bool IsActive { get; private set; } = true;

        public CyclopsNext Next => CyclopsNext.Rent(this);

        protected CyclopsRoutine()
            => Initialize(0, 1, null);

        public CyclopsRoutine(double period, double cycles, Func<float, float> ease = null)
            => Initialize(period, cycles, ease);
        
        /// <summary>
        /// TODO: Fully document CyclopsRoutine.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="maxCycles"></param>
        /// <param name="ease"></param>
        protected void Initialize(double period, double maxCycles, Func<float, float> ease)
        {
            Assert.IsTrue(ValidateTimingValueWhereZeroIsOk(period, out string reason), reason);
            Assert.IsTrue(ValidateTimingValue(maxCycles, out reason), reason);

            Context = this;
            MaxCycles = maxCycles;
            Period = period;
            Speed = 1d;
            
            Ease = ease ?? Easing.Linear;

            Children = ListPool<CyclopsRoutine>.Get();
            Tags = Array.Empty<string>(); // zero alloc
        }

        private void Reinitialize()
        {
            Initialize(Period, MaxCycles, Ease);
        }

        protected static T InstantiateFromPool<T>(double period = 0d, double cycles = 1.0, Func<float, float> ease = null) where T : CyclopsRoutine, new()
        {
            if (s_pool.Rent(() => new T(), out T result))
                result.Reinitialize();

            result.Period = period;
            result.MaxCycles = cycles;
            result.Ease = ease;

            result._isPooled = true;

            return result;
        }

        protected static bool TryInstantiateFromPool<T>(Func<T> routineFactory, out T result) where T : CyclopsRoutine
        {
            bool wasFound = false;

            if (s_pool.Rent(routineFactory, out result))
            {
                wasFound = true;
                result.Reinitialize();
            }

            result._isPooled = true;

            return wasFound;
        }

        void ICyclopsDisposable.Dispose()
        {
            Ease = null;
            _canSyncAtStart = false;
            _isReadyToCallFirstFrameAgain = false;
            _shouldStoppageCallLastFrame = false;
            _stoppagePredicate = null;

            Age = 0d;
            Context = null;
            Cycle = 0d;
            FailureHandler = null;
            Host = null;
            IsActive = false;
            IsPaused = false;
            MaxCycles = 0d;
            NestingDepth = 0;
            Period = 0d;
            SkipPredicate = null;
            Speed = 1d;

            ListPool<CyclopsRoutine>.Release(Children);

            if (Tags is HashSet<string> tags)
                HashSetPool<string>.Release(tags);

            if (!_isPooled)
                return;
            
            if (MustRecycleIfPooled)
                OnRecycle();

            s_pool.Release(this);
        }

        T ICyclopsRoutineScheduler.Add<T>(T routine)
        {
            Assert.IsNotNull(routine);
            Children.Add(routine);

            return routine;
        }

        public CyclopsRoutine AddTag(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);

            if (Tags is not HashSet<string>)
                Tags = HashSetPool<string>.Get();

            ((HashSet<string>)Tags).Add(tag);

            return this;
        }

        public T ObtainReference<T>(out T self) where T : CyclopsRoutine
        {
            self = (T)this;

            return (T)this;
        }

        public void RemoveAllChildren()
        {
            Children?.Clear();
        }

        public CyclopsRoutine Repeat(double cycles)
        {
            Assert.IsTrue(ValidateTimingValue(cycles, out var reason), reason);
            MaxCycles = cycles;

            return this;
        }

        public void Restart()
        {
            Cycle = 0d;
            Age = 0d;
        }

        public CyclopsRoutine SkipIf(Func<bool> predicate)
        {
            Context.SkipPredicate = predicate;
            return Context;
        }

        public CyclopsRoutine StopWhen(Func<CyclopsRoutine, bool> predicate)
        {
            Assert.IsNotNull(predicate);
            _stoppagePredicate = predicate;
            _shouldStoppageCallLastFrame = false;

            return this;
        }

        public CyclopsRoutine StopWhen(bool shouldCallLastFrame, Func<CyclopsRoutine, bool> predicate)
        {
            Assert.IsNotNull(predicate);
            _stoppagePredicate = predicate;
            _shouldStoppageCallLastFrame = shouldCallLastFrame;

            return this;
        }

        public CyclopsRoutine SyncAtStart()
        {
            _canSyncAtStart = true;

            return this;
        }

        public void StepForward()
        {
            ++Age;
        }

        protected void Fail()
        {
            FailureHandler?.Invoke();
            RemoveAllChildren();
            Stop();
        }

        public CyclopsRoutine OnFailure(Action failureHandler)
        {
            Assert.IsNotNull(failureHandler);
            FailureHandler = failureHandler;

            return this;
        }

        public void Stop(bool callLastFrame = false, bool callExit = true)
        {
            if (IsActive)
            {
                IsActive = false;

                if (callLastFrame)
                    OnLastFrame();

                if (callExit)
                    OnExit();

                IsActive = false;
            }
        }

        internal void Update(float deltaTime)
        {
            // Not asserting deltaTime here.
            // This is already done in CyclopsEngine.

            if (!IsActive)
                return;

            if (Age == 0d)
            {
                OnEnter();
                OnFirstFrame();

                if (_canSyncAtStart)
                    OnUpdate(Ease?.Invoke(0f) ?? 0f);
            }

            if (Age >= MaxCycles)
            {
                Stop(callLastFrame: true, callExit: true);
                return;
            }

            if (_isReadyToCallFirstFrameAgain)
            {
                OnFirstFrame();
                _isReadyToCallFirstFrameAgain = false;
            }

            // If period is 0 then age is naturally incremented.
            // Negative periods aren't valid.
            // Also, please consider floating point drift.
            // It's generally fine, but should be considered.
            if (Period > 0d)
            {
                Age = Math.Min(Age + (deltaTime * Speed) / Period, MaxCycles);
            }
            else
            {
                ++Age;
            }

            var t = (float)(Age - Cycle);

            if (Mathf.Approximately(t, 1f))
                t = 1f;
            
            if (t <= 1f)
            {
                OnUpdate(Ease?.Invoke(t) ?? t);
            }
            else if (Age >= MaxCycles)
            {
                OnUpdate(Ease?.Invoke(1f) ?? 1f);
            }
            else
            {
                OnUpdate(Ease?.Invoke(t - 1f) ?? t - 1f);
            }

            // Cycle lags Age. This provides a way to know if OnLastFrame should be called.
            // Otherwise, Cycle could just be calculated by flooring Age.
            if ((Age - Cycle) >= 1.0)
            {
                if (Cycle < (MaxCycles - 1.0))
                {
                    // OnLastFrame is called at the end of each cycle.
                    // If a cycle's period takes 1 second, then OnFrame will likely be called many times before OnLastFrame is called.
                    OnLastFrame();
                    ++Cycle;

                    if (Cycle >= MaxCycles)
                    {
                        Stop(callLastFrame: true, callExit: true);
                        return;
                    }

                    _isReadyToCallFirstFrameAgain = true;
                }
                else
                {
                    Stop(callLastFrame: true, callExit: true);
                }
            }
            
            if (_stoppagePredicate?.Invoke(this) is not null)
                Stop(callLastFrame: _shouldStoppageCallLastFrame, callExit: false);
        }

        protected virtual void OnEnter() { }
        protected virtual void OnFirstFrame() { }
        protected virtual void OnUpdate(float t) { }
        protected virtual void OnLastFrame() { }
        protected virtual void OnExit() { }

        protected virtual void OnRecycle()
        {
            if (_isPooled)
                throw new NotImplementedException("OnRecycle must be implemented when pooling is utilized and MustRecycleIfPooled == true");
            else
                throw new NotImplementedException("OnRecycle must be implemented when pooling is utilized, but pooling is not being utilized and it was called anyway.");
        }
    }
}
