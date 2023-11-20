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
        /// <see cref="Entered"/> is invoked any time this state is entered.
        /// A state can not be re-entered until it has exited.
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
        /// <see cref="LayeredUpdating"/> is invoked when this state sits below other states in the state machine's stack.
        /// If this is the only state, then <see cref="LayeredUpdating"/> will not invoked.
        /// <para><br/>Tip: Keep it simple and only use <see cref="LayeredUpdating"/> when background operations are truly essential.
        /// If this isn't the simplest long-term approach, then try something else.</para>
        /// <seealso cref="CyclopsStateMachine"/>
        /// <seealso cref="Updating"/>
        /// </summary>
        public Action LayeredUpdating { get; set;  }
        
        /// <summary>
        /// <see cref="Exited"/> is invoked any time this state is exited.
        /// A state can not be exited until after it is entered.
        /// A state must be re-entered to exit again.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Exited { get; set; }
        
        protected override void OnEnter() => Entered?.Invoke();

        internal override void Update(bool isLayered)
        {
            if (isLayered)
                Updating?.Invoke();
            else
                LayeredUpdating?.Invoke();
        }
        
        protected override void OnExit() => Exited?.Invoke();
    }
}
