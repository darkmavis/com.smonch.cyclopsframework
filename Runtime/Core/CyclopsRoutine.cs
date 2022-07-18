// Cyclops Framework
// 
// Copyright 2010 - 2022 Mark Davis
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
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsRoutine : CyclopsCommon, ICyclopsDisposable, ICyclopsPausable, ICyclopsTaggable, ICyclopsRoutineScheduler
    {
        private static CyclopsPool<CyclopsRoutine> Pool { get; set; } = new CyclopsPool<CyclopsRoutine>();
        
        private bool _canSyncAtStart;
        private bool _isPooled;
        private bool _isReadyToCallFirstFrameAgain;
        private bool _shouldStoppageCallLastFrame;

        private string _initialTag = Tag_Undefined;

        private Func<CyclopsRoutine, bool> _stoppagePredicate;

        // Use a linear bias by default: t => t;
        // Please note that all built-in Bias methods make an attempt to benefit from: MethodImplOptions.AggressiveInlining
        protected Func<float, float> Bias { get; set; } = null;

        /// <summary>
        /// This provides a way to gracefully handle failures such as timeouts when waiting for an event or polling for a particular state.
        /// </summary>
        private Action FailureHandler { get; set; }

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

        public double Speed { get; set; } = 0d;

        // Pooled
        public HashSet<string> Tags { get; private set; }

        // Pooled
        public List<CyclopsRoutine> Children { get; private set; }

        /// <summary>
        /// This provides a reference to the CyclopsEngine host.
        /// </summary>
        internal CyclopsEngine Host { get; set; }

        public virtual bool IsPaused { get; set; } = false;

        public bool IsActive { get; private set; } = true;

        public CyclopsNext Next => CyclopsNext.Rent(this);

        public CyclopsRoutine(double period, double cycles, string tag)
        {
            Initialize(period, cycles, bias: null, tag);
        }

        public CyclopsRoutine(double period, double cycles, Func<float, float> bias, string tag)
        {
            Initialize(period, cycles, bias, tag);
        }

        /// <summary>
        /// TODO: Fully document CyclopsRoutine.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="maxCycles"></param>
        /// <param name="bias"></param>
        /// <param name="tag"></param>
        protected void Initialize(double period, double maxCycles, Func<float, float> bias, string tag)
        {
            Assert.IsTrue(ValidateTimingValueWhereZeroIsOk(period, out var reason), reason);
            Assert.IsTrue(ValidateTimingValue(maxCycles, out reason), reason);
            Assert.IsTrue(ValidateTag(tag, out reason), reason);

            Context = this;
            MaxCycles = maxCycles;
            Period = period;
            Speed = 1d;

            Bias = bias ?? CyclopsFramework.Bias.Linear;

            Children = ListPool<CyclopsRoutine>.Get();
            Tags = HashSetPool<string>.Get();

            AddTag(tag ?? Tag_Undefined);

            _initialTag = tag;
        }

        private void Reinitialize()
        {
            Initialize(Period, MaxCycles, Bias, _initialTag);
        }

        protected static bool TryInstantiateFromPool<T>(Func<T> routineFactory, out T result) where T : CyclopsRoutine
        {
            bool wasFound = false;

            if (Pool.Rent(routineFactory, out result))
            {
                wasFound = true;
                result.Reinitialize();
            }

            result._isPooled = true;

            return wasFound;
        }

        void ICyclopsDisposable.Dispose()
        {
            Bias = null;
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
            HashSetPool<string>.Release(Tags);

            if (_isPooled)
            {
                if (MustRecycleIfPooled)
                    OnRecycle();

                Pool.Release(this);
            }
        }

        T ICyclopsRoutineScheduler.Add<T>(T routine)
        {
            Assert.IsNotNull(routine);
            Children.Add(routine);

            return routine;
        }

        public CyclopsRoutine AddTag(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);

            if (string.IsNullOrWhiteSpace(tag))
                tag = Tag_Undefined;

            Tags.Add(tag);

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
                {
                    OnLastFrame();
                }

                if (callExit)
                {
                    OnExit();
                }

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
                    OnUpdate((Bias == null) ? 0f : Bias(0f));
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

            if (t <= 1f)
            {
                OnUpdate((Bias == null) ? t : Bias(t));
            }
            else if (Age >= MaxCycles)
            {
                OnUpdate((Bias == null) ? 1f : Bias(1f));
            }
            else
            {
                OnUpdate((Bias == null) ? t - 1f : Bias(t - 1f));
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

            if (_stoppagePredicate != null)
            {
                if (_stoppagePredicate(this))
                {
                    Stop(callLastFrame: _shouldStoppageCallLastFrame, callExit: false);
                }
            }

            return;
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
