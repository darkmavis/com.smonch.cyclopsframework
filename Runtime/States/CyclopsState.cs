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

namespace Smonch.CyclopsFramework
{
    /// <summary>
    /// <para><see cref="CyclopsState"/> operates as a state within a classic FSM until states are stacked.</para>
    /// <para>If states are stacked, then <see cref="CyclopsState"/> operates as a state within a push-down automata.</para>
    /// <para>For a comprehensive explanation of the state pattern and its relatives, please see:
    /// https://gameprogrammingpatterns.com/state.html</para>
    /// <seealso cref="CyclopsStateMachine"/>
    /// </summary>
    public class CyclopsState : CyclopsBaseState
    {
        /// <summary>
        /// <see cref="Entered"/> is invoked when this state is entered.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Entered { get; set; }
        
        /// <summary>
        /// <see cref="Updating"/> is invoked when this state is the active state in the state machine's stack.
        /// If this is the only state, then <see cref="Updating"/> will always be invoked.
        /// <seealso cref="CyclopsStateMachine"/>
        /// <seealso cref="LayeredUpdating"/>
        /// </summary>
        public Action Updating { get; set; }
        
        /// <summary>
        /// <see cref="BackgroundUpdating"/> is invoked when this state sits below other states in the state machine's stack.
        /// If this is the only state, then <see cref="BackgroundUpdating"/> will not invoked.
        /// <seealso cref="CyclopsStateMachine"/>
        /// <seealso cref="Updating"/>
        /// </summary>
        public Action BackgroundUpdating { get; set; }
        
        /// <summary>
        /// <see cref="Exited"/> is invoked any time this state is exited.
        /// A state can not be exited until after it is entered.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Exited { get; set; }
        
        public Action BackgroundModeEntered { get; set; }
        public Action BackgroundModeExited { get; set; }
        
        protected override void OnEnter() => Entered?.Invoke();
        protected override void OnExit() => Exited?.Invoke();
        protected override void OnUpdate() => Updating?.Invoke();
        protected override void OnEnterBackgroundMode() => BackgroundModeEntered?.Invoke();
        protected override void OnExitBackgroundMode() => BackgroundModeExited?.Invoke();
        protected override void OnBackgroundUpdate() => BackgroundUpdating?.Invoke();
    }
}
