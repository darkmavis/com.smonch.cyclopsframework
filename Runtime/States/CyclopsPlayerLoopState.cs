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
        public Action LayeredTimeUpdating { get; set; }
        public Action LayeredInitializing { get; set; }
        public Action LayeredEarlyUpdating { get; set; }
        public Action LayeredFixedUpdating { get; set; }
        public Action LayeredPreUpdating { get; set; }
        public Action LayeredUpdating { get; set; }
        public Action LayeredPreLateUpdating { get; set; }
        public Action LayeredPostLateUpdating { get; set; }
        
        /// <summary>
        /// <see cref="Exited"/> is invoked any time this state is exited.
        /// A state can not be exited until after it is entered.
        /// A state must be re-entered to exit again.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public Action Exited { get; set; }
        
        protected override void OnEnter() => Entered?.Invoke();
        
        protected override void OnTimeUpdate() => TimeUpdating?.Invoke();
        protected override void OnPlayerLoopInitialization() => Initializing?.Invoke();
        protected override void OnEarlyUpdate() => EarlyUpdating?.Invoke();
        protected override void OnFixedUpdate() => FixedUpdating?.Invoke();
        protected override void OnPreUpdate() => PreUpdating?.Invoke();
        protected override void OnUpdate() => Updating?.Invoke();
        protected override void OnPreLateUpdate() => PreLateUpdating?.Invoke();
        protected override void OnPostLateUpdate() => PostLateUpdating?.Invoke();
        
        protected override void OnLayeredTimeUpdate() => LayeredTimeUpdating?.Invoke();
        protected override void OnLayeredPlayerLoopInitialization() => LayeredInitializing?.Invoke();
        protected override void OnLayeredEarlyUpdate() => LayeredEarlyUpdating?.Invoke();
        protected override void OnLayeredFixedUpdate() => LayeredFixedUpdating?.Invoke();
        protected override void OnLayeredPreUpdate() => LayeredPreUpdating?.Invoke();
        protected override void OnLayeredUpdate() => LayeredUpdating?.Invoke();
        protected override void OnLayeredPreLateUpdate() => LayeredPreLateUpdating?.Invoke();
        protected override void OnLayeredPostLateUpdate() => LayeredPostLateUpdating?.Invoke();
        
        protected override void OnExit() => Exited?.Invoke();
    }
}
