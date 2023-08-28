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
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    /// <summary>
    /// <para><see cref="CyclopsBaseState"/> operates as a state within a classic FSM until states are stacked.</para>
    /// <para>If states are stacked, then <see cref="CyclopsBaseState"/> operates as a state within a push-down automata.
    /// This works because these patterns are compatible and the usage defines the pattern.</para>
    /// <para>For a comprehensive explanation of the state pattern and its relatives, please see:
    /// https://gameprogrammingpatterns.com/state.html</para>
    /// <seealso cref="CyclopsStateMachine"/>
    /// </summary>
    public abstract class CyclopsBaseState : IDisposable
    {
        private readonly List<CyclopsStateTransition> _transitions;

        internal bool IsActive { get; private set; }
        internal bool IsStopping { get; private set; }

        protected CyclopsBaseState()
        {
            _transitions = ListPool<CyclopsStateTransition>.Get();
        }
        
        /// <summary>
        /// <see cref="Start"/> is called by the host state machine and should not be otherwise called.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal void Start()
        {
            IsActive = true;
            IsStopping = false;
            
            OnEnter();
        }
        
        /// <summary>
        /// <see cref="Update"/> is called by the host state machine and should not be otherwise called.
        /// See <see cref="CyclopsState"/> for example usage. 
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal virtual void Update(bool isLayeredUpdate = false)
        {
            if (isLayeredUpdate)
                OnLayeredUpdate();
            else
                OnUpdate();
        }
        
        internal void StopImmediately()
        {
            bool wasActive = IsActive;
            
            IsActive = false;
            
            if (wasActive)
                OnExit();
        }
        
        /// <summary>
        /// <see cref="Stop"/> will cause the state to exit if it was entered.
        /// This will also pop a stacked state off the stack.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public void Stop()
        {
            IsStopping = true;
        }
        
        /// <summary>
        /// Add a transition from this state to a target state based on a condition.
        /// Feel free to add as many transitions as needed.
        /// Transitions can not be removed, nor should they be.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public void AddTransition(CyclopsStateTransition transition)
        {
            _transitions.Add(transition);
        }
        
        /// <summary>
        /// Add a transition from this state to a target state based on a condition.
        /// Feel free to add as many transitions as needed.
        /// Transitions can not be removed, nor should they be.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public void AddTransition(CyclopsBaseState target, Func<bool> predicate)
        {
            AddTransition(new CyclopsStateTransition { Target = target, Condition = predicate });
        }
        
        /// <summary>
        /// <see cref="QueryTransitions"/> is called by the hosting state machine and should not be otherwise called.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal bool QueryTransitions(out CyclopsBaseState nextBaseState)
        {
            nextBaseState = null;

            foreach (var transition in _transitions)
            {
                if (transition.Condition())
                {
                    nextBaseState = transition.Target;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <see cref="OnEnter"/> is invoked any time this state is entered.
        /// A state can not be re-entered until it has exited.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        protected virtual void OnEnter() { }
        
        /// <summary>
        /// <see cref="OnUpdate"/> is invoked when this state is the active state in the state machine's stack.
        /// If this is the only state, then <see cref="OnUpdate"/> will always be invoked.
        /// <seealso cref="CyclopsStateMachine"/>
        /// <seealso cref="OnLayeredUpdate"/>
        /// </summary>
        protected virtual void OnUpdate() { }
        
        /// <summary>
        /// <see cref="OnLayeredUpdate"/> is invoked when this state sits below other states in the state machine's stack.
        /// If this is the only state, then <see cref="OnLayeredUpdate"/> will not invoked.
        /// <para><br/>Tip: Keep it simple and only use <see cref="OnLayeredUpdate"/> when background operations are truly essential.
        /// If this isn't the simplest long-term approach, then try something else.</para>
        /// <seealso cref="CyclopsStateMachine"/>
        /// <seealso cref="OnUpdate"/>
        /// </summary>
        protected virtual void OnLayeredUpdate() { }
        
        /// <summary>
        /// <see cref="OnExit"/> is invoked any time this state is exited.
        /// A state can not be exited until after it is entered.
        /// A state must be re-entered to exit again.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        protected virtual void OnExit() { }
        
        /// <summary>
        /// Only use this if you need it. The base class uses <see cref="Dispose"/> to release internally allocated objects to a pool.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        protected virtual void Dispose(bool isDisposing) { }
        
        /// <summary>
        /// <see cref="Dispose"/> releases internally allocated objects to a pool.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public void Dispose()
        {
            ListPool<CyclopsStateTransition>.Release(_transitions);
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
