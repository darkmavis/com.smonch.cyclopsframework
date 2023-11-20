using System;

namespace Smonch.CyclopsFramework
{
    public class CyclopsPlayerLoopState : CyclopsBaseState
    {
        /// <summary>
        /// <see cref="Entered"/> is invoked any time this state is entered.
        /// A state can not be re-entered until it has exited.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Entered { get; set; }
        
        public Action TimeUpdating { get; set; }
        public Action Initializing { get; set; }
        public Action EarlyUpdating { get; set; }
        public Action FixedUpdating { get; set; }
        public Action PreUpdating { get; set; }
        public Action Updating { get; set; }
        public Action PreLateUpdating { get; set; }
        public Action PostLateUpdating { get; set; }
        
        /// <summary>
        /// <see cref="Exited"/> is invoked any time this state is exited.
        /// A state can not be exited until after it is entered.
        /// A state must be re-entered to exit again.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Exited { get; set; }
        
        protected override void OnEnter() => Entered?.Invoke();
        
        protected internal override void OnTimeUpdate(bool isLayered) => TimeUpdating?.Invoke();
        protected internal override void OnPlayerLoopInitialization(bool isLayered) => Initializing?.Invoke();
        protected internal override void OnEarlyUpdate(bool isLayered) => EarlyUpdating?.Invoke();
        protected override void OnFixedUpdate(bool isLayered) => FixedUpdating?.Invoke();
        protected internal override void OnPreUpdate(bool isLayered) => PreUpdating?.Invoke();
        protected override void OnUpdate(bool isLayered) => Updating?.Invoke();
        protected internal override void OnPreLateUpdate(bool isLayered) => PreLateUpdating?.Invoke();
        protected internal override void OnPostLateUpdate(bool isLayered) => PostLateUpdating?.Invoke();
        
        protected override void OnExit() => Exited?.Invoke();
    }
}
