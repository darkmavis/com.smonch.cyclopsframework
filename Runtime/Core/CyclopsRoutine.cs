// Cyclops Framework
// 
// Copyright 2010 - 2024 Mark Davis
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
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsRoutine : CyclopsCommon, ICyclopsDisposable, ICyclopsPausable, ICyclopsTaggable, ICyclopsRoutineScheduler
    {
        // ReSharper disable once InconsistentNaming - ReSharper isn't happy with idiomatic C#.
        private static CyclopsPool<CyclopsRoutine> s_pool;
        
        // This is important for in-editor situations where domain reloading is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => s_pool = new CyclopsPool<CyclopsRoutine>();
        
        private bool _canSyncAtStart;
        private bool _isPooled;
        private bool _isReadyToCallFirstFrameAgain;

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
        
        /// <summary>
        /// Not every routine needs to be recycled when pooled.
        /// Don't use this unless you're sure about it.
        /// </summary>
        protected bool MustRecycleIfPooled { get; set; } = true;

        /// <summary>
        /// This is used by CyclopsEngine and provides a way to skip adding this routine to the active queue if the specified condition is met.
        /// If needed, use CyclopsCommon.SkipIf to provide a condition.
        /// Any usage is external to CyclopsRoutine.
        /// </summary>
        public Func<bool> SkipPredicate { get; set; }
        
        /// <summary>
        /// Age is measured as Cycle + Position.
        /// </summary>
        public double Age { get; private set; }
        
        /// <summary>
        /// The period is the time in seconds between cycles.
        /// </summary>
        public double Period { get; protected set; }
        
        /// <summary>
        /// This is the current cycle. Cycles are whole numbers.
        /// </summary>
        public double Cycle { get; private set; }

        /// <summary>
        /// This is the maximum number of cycles and has the option to be fractional. Half is a valid value.
        /// Example: If t => abs(sin(t * Tau)) was used to animate blinking then 3 blinks would use 1.5 cycles.
        /// </summary>
        public double MaxCycles { get; protected set; }

        /// <summary>
        /// Position is a normalized value between 0 and 1 and can be thought of as progress in between cycles.
        /// It can be calculated as Age - Cycle.
        /// </summary>
        public double Position => ((Age - Cycle) >= 1.0) ? 1.0 : (Age - Cycle);
        
        /// <summary>
        /// Speed is a multiplier that can be used to speed up or slow down a routine.
        /// This can be especially useful when multiple cycles are involved.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Tags can be used when manipulating routines by tags in various ways from CyclopsEngine.
        /// Routines can contain multiple tags that cascade down through all child routines that follow.
        /// Cascading is optional. Prefix a tag with a ! character to prevent cascading.
        /// Tags will be registered with CyclopsEngine when a routine is added to the active queue.
        /// Note: This collection is pooled.
        /// </summary>
        public IEnumerable<string> Tags { get; private set; }

        /// <summary>
        /// Multiple child routines can be sequenced to immediately follow this one.
        /// They will be added to the active queue in the order that they are sequenced.
        /// Although typically appearing as a simple sequence, the routine graph is actually a tree structure.
        /// Note: This collection is pooled.
        /// </summary>
        public List<CyclopsRoutine> Children { get; private set; }
        
        /// <summary>
        /// This provides a reference to the CyclopsEngine host.
        /// </summary>
        internal CyclopsEngine Host { get; set; }
        
        /// <summary>
        /// Routines can be paused and resumed, typically by tag, using CyclopsEngine.
        /// </summary>
        public virtual bool IsPaused { get; set; }
        
        internal bool IsActive { get; private set; } = true;
        
        /// <summary>
        /// Was the first frame in the first cycle just entered?
        /// Note: This is called before OnEnter.
        /// </summary>
        public bool WasEntered { get; private set; }

        public CyclopsNext Next => CyclopsNext.Rent(this);

        /// <summary>
        /// Instantiate a CyclopsRoutine with the default settings.
        /// </summary>
        protected CyclopsRoutine()
            => Initialize(0d, 1d, null);

        /// <summary>
        /// This will create a new Routine vs instantiating from a pool.
        /// Depending on your needs, either way is fine.
        /// </summary>
        /// <param name="period">is the time in seconds between cycles.</param>
        /// <param name="maxCycles">is the maximum cycles and is optionally fractional (half is valid.)</param>
        /// <param name="ease">is an f(t) easing function. Default: Easing.Linear</param>
        public CyclopsRoutine(double period, double maxCycles, Func<float, float> ease = null)
            => Initialize(period, maxCycles, ease);
        
        /// <summary>
        /// This provides a common way to initialize a CyclopsRoutine and is used by the reinitialization process
        /// when instantiating from a pool.
        /// </summary>
        /// <param name="period">is the time in seconds between cycles.</param>
        /// <param name="maxCycles">is the maximum cycles and is optionally fractional (half is valid.)</param>
        /// <param name="ease">is an f(t) easing function. Default: Easing.Linear</param>
        protected void Initialize(double period, double maxCycles, Func<float, float> ease)
        {
            Debug.Assert(ValidateTimingValueWhereZeroIsOk(period, out string reason), reason);
            Debug.Assert(ValidateTimingValue(maxCycles, out reason), reason);

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
        
        /// <summary>
        /// Instantiate a routine from the pool to avoid additional allocations and garbage collection.
        /// </summary>
        /// <param name="period">is the time in seconds between cycles.</param>
        /// <param name="maxCycles">is the maximum cycles and is optionally fractional (half is valid.)</param>
        /// <param name="ease">is an f(t) easing function. Default: Easing.Linear</param>
        /// <typeparam name="T">T : CyclopsRoutine</typeparam>
        /// <returns>T : CyclopsRoutine</returns>
        protected static T InstantiateFromPool<T>(double period = 0d, double maxCycles = 1.0, Func<float, float> ease = null) where T : CyclopsRoutine, new()
        {
            // If thread safety was needed, we would handle the return value of Rent.
            // CyclopsEngine isn't intended to be thread-safe even if this is the only thing holding it back.
            // CyclopsPool is thread-safe, but that functionality just isn't terribly useful for the intended use case.
            _ = s_pool.Rent(() => new T(), out T result);
            result.Reinitialize();
            
            result.Period = period;
            result.MaxCycles = maxCycles;
            result.Ease = ease;

            result._isPooled = true;

            return result;
        }

        void ICyclopsDisposable.Dispose()
        {
            Ease = null;
            _canSyncAtStart = false;
            _isReadyToCallFirstFrameAgain = false;

            Age = 0d;
            Context = null;
            Cycle = 0d;
            FailureHandler = null;
            Host = null;
            IsActive = false;
            IsPaused = false;
            WasEntered = false;
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
        
        /// <summary>
        /// Sequence a child routine to run after this one.
        /// Multiple child routines can be sequenced to immediately follow this one.
        /// They will be added to the active queue in the order that they are sequenced.
        /// Call this method multiple times to sequence multiple concurrent child routines.
        /// Although typically appearing as a simple sequence, the routine graph is actually a tree structure.
        /// If a tree structure is required, the use ObtainReference to provide easier access to this routine.
        /// </summary>
        /// <param name="routine">is the routine to be added as a child.</param>
        /// <typeparam name="T">T : CyclopsRoutine</typeparam>
        /// <returns>T : CyclopsRoutine</returns>
        T ICyclopsRoutineScheduler.Add<T>(T routine)
        {
            Debug.Assert(routine is not null);
            Children.Add(routine);

            return routine;
        }

        /// <summary>
        /// Tag this routine with as many tags as required to manipulate it by tag later.
        /// Call this method multiple times to add multiple tags.
        /// </summary>
        /// <param name="tag">
        /// is like a hashtag that cascades from this routine, trickling down through all child routines that follow.
        /// Cascading is optional. Prefix a tag with a ! character to prevent cascading.
        /// </param>
        /// <returns>CyclopsRoutine</returns>
        public CyclopsRoutine AddTag(string tag)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);

            if (Tags is not HashSet<string>)
                Tags = HashSetPool<string>.Get();

            ((HashSet<string>)Tags).Add(tag);

            return this;
        }

        /// <summary>
        /// Obtain a reference to this routine for later use.
        /// This is most useful when inserted into a sequence of routines.
        /// This will allow modification of this routine outside the sequence.
        /// </summary>
        /// <param name="self">outputs a reference for future use.</param>
        /// <typeparam name="T">is a generic subclass of CyclopsRoutine.</typeparam>
        /// <returns>T : CyclopsRoutine</returns>
        public T ObtainReference<T>(out T self) where T : CyclopsRoutine
        {
            self = (T)this;

            return (T)this;
        }
        
        /// <summary>
        /// Ensure that any routine scheduled to follow this one, will not run.
        /// </summary>
        public void RemoveAllChildren()
        {
            Children?.Clear();
        }
        
        /// <summary>
        /// This will alter the maximum number of cycles that a routine will run for.
        /// CAUTION: Some routines should not be altered.
        /// </summary>
        /// <param name="cycles"></param>
        /// <returns>CyclopsRoutine</returns>
        public CyclopsRoutine Repeat(double cycles)
        {
            Debug.Assert(ValidateTimingValue(cycles, out string reason), reason);
            MaxCycles = cycles;

            return this;
        }
        
        /// <summary>
        /// This will NOT RESET the routine to its initial state.
        /// This will only reset the age and cycle to zero.
        /// </summary>
        public void Restart()
        {
            Cycle = 0d;
            Age = 0d;
        }
        
        /// <summary>
        /// If the predicate is satisfied then this routine and all of its children will be skipped.
        /// </summary>
        /// <param name="predicate"> checks to see if this routine should be skipped.</param>
        /// <returns>CyclopsRoutine</returns>
        public CyclopsRoutine SkipIf(Func<bool> predicate)
        {
            Context.SkipPredicate = predicate;
            return Context;
        }
        
        /// <summary>
        /// This will force an OnUpdate(t=0) call immediately after FirstFrame is called for the first time.
        /// This implies that two calls to OnUpdate(t) will occur on that frame.
        /// </summary>
        /// <returns>CyclopsRoutine</returns>
        public CyclopsRoutine SyncAtStart()
        {
            _canSyncAtStart = true;

            return this;
        }

        /// <summary>
        /// Jump to the beginning of the next cycle.
        /// </summary>
        public void StepForward()
        {
            Age = Math.Ceiling(Age);
            Cycle = Age;
        }
        
        /// <summary>
        /// Jumps to age which is Cycle + Position.
        /// Position is a normalized value between 0 and 1.
        /// </summary>
        /// <param name="age"></param>
        public void JumpTo(double age)
        {
            Age = age;
            Cycle = Math.Floor(age);
        }
        
        /// <summary>
        /// Call the optional failure handler, remove all children, and stop.
        /// </summary>
        protected void Fail()
        {
            FailureHandler?.Invoke();
            RemoveAllChildren();
            Stop();
        }
        
        /// <summary>
        /// Add an optional failure handler for routines that require one.
        /// </summary>
        /// <param name="failureHandler"></param>
        /// <returns>CyclopsRoutine</returns>
        public CyclopsRoutine OnFailure(Action failureHandler)
        {
            Debug.Assert(failureHandler is not null);
            FailureHandler = failureHandler;

            return this;
        }
        
        /// <summary>
        /// Stop this routine and optionally force calls to OnLastFrame and OnExit.
        /// OnExit is called by default.
        /// </summary>
        /// <param name="callLastFrame"></param>
        /// <param name="callExit"></param>
        public void Stop(bool callLastFrame = false, bool callExit = true)
        {
            if (!IsActive)
                return;
            
            IsActive = false;

            if (callLastFrame)
                OnLastFrame();

            if (callExit)
                OnExit();

            IsActive = false;
        }
        
        /// <summary>
        /// CyclopsEngine uses this update method to drive the routine.
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void Update(float deltaTime)
        {
            // Not asserting deltaTime here.
            // This is already done in CyclopsEngine.

            if (!IsActive)
                return;

            if (Age == 0d)
            {
                WasEntered = true;
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
        }
        
        /// <summary>
        /// Override to react to the first frame of the first cycle.
        /// </summary>
        protected virtual void OnEnter() { }
        
        /// <summary>
        /// Override to react to the first frame of each cycle.
        /// </summary>
        protected virtual void OnFirstFrame() { }
        
        /// <summary>
        /// Override to react to each frame of each cycle.
        /// This is useful for interpolation and progress tracking.
        /// </summary>
        /// <param name="t">is a normalized value between 0 and 1.</param>
        protected virtual void OnUpdate(float t) { }
        
        /// <summary>
        /// Override to react to the last frame of each cycle or optionally when Stop is called.
        /// </summary>
        protected virtual void OnLastFrame() { }
        
        /// <summary>
        /// Override to react to the last frame of the last cycle or by optional default when Stop is called.
        /// </summary>
        protected virtual void OnExit() { }
        
        /// <summary>
        /// If and only if pooling is utilized, be sure to override this method to reset state before releasing to the pool.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual void OnRecycle()
        {
            if (_isPooled)
                throw new NotImplementedException("OnRecycle must be implemented when pooling is utilized and MustRecycleIfPooled == true");
            
            throw new NotImplementedException("OnRecycle must be implemented when pooling is utilized, but pooling is not being utilized and it was called anyway.");
        }
    }
}
