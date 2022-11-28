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
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsState
    {
        private List<CyclopsStateTransition> _transitions;

        public bool IsActive { get; private set; }
        public bool IsFinished => !IsActive;
        public bool IsStopping { get; set; }

        public CyclopsState()
        {
            _transitions = ListPool<CyclopsStateTransition>.Get();
        }

        public void Start()
        {
            IsActive = true;
            IsStopping = false;

            OnEnter();
        }

        public virtual void Update(bool isLayeredUpdate = false)
        {
            if (isLayeredUpdate)
                OnLayeredUpdate();
            else
                OnUpdate();
        }

        internal void StopImmediately()
        {
            OnExit();
            IsActive = false;
        }

        public void Stop()
        {
            IsStopping = true;
        }

        public void AddTransition(CyclopsStateTransition transition)
        {
            _transitions.Add(transition);
        }

        public void AddTransition(CyclopsState target, Func<bool> predicate)
        {
            AddTransition(new CyclopsStateTransition { Target = target, Condition = predicate });
        }

        public bool QueryTransitions(out CyclopsState nextState)
        {
            nextState = null;

            foreach (var transition in _transitions)
            {
                if (transition.Condition())
                {
                    nextState = transition.Target;

                    return true;
                }
            }

            return false;
        }

        protected virtual void OnEnter() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLayeredUpdate() { }
        protected virtual void OnExit() { }

        internal void Dispose()
        {
            ListPool<CyclopsStateTransition>.Release(_transitions);
            _transitions = null;

            OnDisposed();
        }

        protected virtual void OnDisposed() { }
    }
}
