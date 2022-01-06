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

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsState
    {
        private Queue<CyclopsState> _substates = new Queue<CyclopsState>();
        private List<CyclopsStateTransition> _transitions = new List<CyclopsStateTransition>();
        
        public bool IsActive { get; private set; }
        public bool IsFinished => !IsActive;
        public bool IsStopping { get; set; }

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

        public void Stop(Type callerType = null)
        {
            if (callerType == typeof(CyclopsStateMachine))
            {
                OnExit();
                IsActive = false;
            }
            else
            {
                IsStopping = true;
            }
        }

        public void AddSubstate(CyclopsState substate)
        {
            _substates.Enqueue(substate);
        }

        public void AddTransition(CyclopsStateTransition transition)
        {
            _transitions.Add(transition);
        }

        public void AddTransition(CyclopsState target, Func<bool> condition)
        {
            AddTransition(new CyclopsStateTransition { Target = target, Condition = condition });
        }

        public bool TryGetSubstate(out CyclopsState substate)
        {
            bool result = false;

            substate = null;

            if (_substates.Count > 0)
            {
                substate = _substates.Dequeue();
                result = true;
            }

            return result;
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
    }
}
