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

namespace Smonch.CyclopsFramework
{
    public class CyclopsRoutine : CyclopsCommon, ICyclopsDisposable, ICyclopsPausable, ICyclopsTaggable
    {
        // These collections provide a way to ease up on allocations.
        private static Queue<List<string>> _tagCollectionPool;
        private static Queue<List<CyclopsRoutine>> _childRoutineCollectionPool;

        // Use a linear bias by default: t => t;
        // Please note that all built-in Bias methods make an attempt to benefit from: MethodImplOptions.AggressiveInlining
        private Func<float, float> _bias = null;

        private bool _canSyncAtStart;
        private bool _shouldStoppageCallLastFrame;
        private Func<CyclopsRoutine, bool> _stoppagePredicate;
        private bool _tagsAreDirty;

        /// <summary>
        /// This provides a way to gracefully handle failures such as timeouts when waiting for an event or polling for a particular state.
        /// </summary>
        private Action FailureHandler { get; set; }
        
        internal int NestingDepth { get; set; }

        /// <summary>
        /// This is used by CyclopsEngine and provides a way to skip adding this routine to the active queue if the specified condition is met.
        /// If needed, use CyclopsCommon.SkipIf to provide a condition.
        /// Any usage is external to CyclopsRoutine.
        /// </summary>
        public Func<bool> SkipPredicate { get; set; }

        public double Age { get; private set; }
        
        public double Period { get; private set; }
        
        public double Cycle { get; private set; }
        
        public double MaxCycles { get; private set; }
        
        public double Position { get => (((Age - Cycle) >= 1.0) ? 1.0 : (Age - Cycle)); }
        
        public double Speed { get; set; } = 0d;

        // Pooled
        public List<string> Tags { get; private set; }

        private string _cachedCsvTagsText;

        // Note: This smells like an extension method.
        public string TagsAsCachedCsvText
        {
            get
            {
                if (_tagsAreDirty)
                {
                    _cachedCsvTagsText = string.Join(",", Tags);
                    _tagsAreDirty = false;
                }

                return _cachedCsvTagsText;
            }
        }

        // Pooled
        public List<CyclopsRoutine> Children { get; private set; }
        
        public virtual bool IsPaused { get; set; } = false;

        public bool IsActive { get; private set; } = true;

        // These collections provide a way to ease up on allocations.
        static CyclopsRoutine()
        {
            _childRoutineCollectionPool = new Queue<List<CyclopsRoutine>>(capacity: 256);
            _tagCollectionPool = new Queue<List<string>>(capacity: 256);

            for (int i = 0; i < 256; ++i)
            {
                _childRoutineCollectionPool.Enqueue(new List<CyclopsRoutine>(capacity: 4));
                _tagCollectionPool.Enqueue(new List<string>(capacity: 12));
            }
        }

        /// <summary>
        /// TODO: Fully document CyclopsRoutine.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="cycles"></param>
        /// <param name="bias"></param>
        /// <param name="tag"></param>
        public CyclopsRoutine(double period, double cycles, Func<float, float> bias, string tag)
            : base()
        {
            Assert.IsTrue(ValidateTimingValueWhereZeroIsOk(period, out var reason), reason);
            Assert.IsTrue(ValidateTimingValue(cycles, out reason), reason);
            // No assertion for a null tag here. Tags can be added later.

            Context = this;
            MaxCycles = cycles;
            Period = period;
            Speed = 1d;

            _bias = bias ?? Bias.Linear;

            EnsureCollectionCapacity();

            AddTag(tag ?? Tag_Undefined);
        }

        private void EnsureCollectionCapacity()
        {
            if (_childRoutineCollectionPool.Count > 0)
            {
                Children = _childRoutineCollectionPool.Dequeue();
                Children.Clear();
            }
            else
            {
                Children = new List<CyclopsRoutine>(capacity: 4);
            }

            if (Tags == null)
            {
                if (_tagCollectionPool.Count > 0)
                {
                    Tags = _tagCollectionPool.Dequeue();
                    Tags.Clear();
                }
                else
                {
                    Tags = new List<string>(capacity: 16);
                }
            }
        }

        void ICyclopsDisposable.Dispose()
        {
            Children.Clear();
            _childRoutineCollectionPool.Enqueue(Children);
            Children = null; // Prevent any possibility of accidentally accessing this in the future.

            Tags.Clear();
            _tagCollectionPool.Enqueue(Tags);
            Tags = null; // Prevent any possibility of accidentally accessing this in the future.
        }

        public override T Add<T>(T routine)
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
            _tagsAreDirty = true;

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
                    OnUpdate((_bias == null) ? 0f : _bias(0f));
            }

            if (Age >= MaxCycles)
            {
                Stop(callLastFrame: true, callExit: true);
                return;
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
                OnUpdate((_bias == null) ? t : _bias(t));
            }
            else if (Age >= MaxCycles)
            {
                OnUpdate((_bias == null) ? 1f : _bias(1f));
            }
            else
            {
                OnUpdate((_bias == null) ? t - 1f : _bias(t - 1f));
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

                    OnFirstFrame();
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
    }
}
