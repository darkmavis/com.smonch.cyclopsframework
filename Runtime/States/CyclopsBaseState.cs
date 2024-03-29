﻿// Cyclops Framework
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
using System.Threading;
using UnityEngine;
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
        private readonly List<CyclopsStateTransition> _transitions = ListPool<CyclopsStateTransition>.Get();
        private CancellationTokenSource _exitCancellationTokenSource = new();

        // ReSharper disable once MemberCanBePrivate.Global
        public CancellationToken ExitCancellationToken { get; private set; }
        public bool IsActive { get; private set; }
        internal bool IsStopping { get; private set; }

        /// <summary>
        /// <see cref="Start"/> is called by the host state machine and should not be otherwise called.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal void Start()
        {
            _exitCancellationTokenSource ??= new CancellationTokenSource();
            ExitCancellationToken = _exitCancellationTokenSource.Token;
            IsActive = true;
            IsStopping = false;
            
            OnEnter();
        }
        
        /// <summary>
        /// <see cref="Update"/> is called by the host state machine and should not be otherwise called.
        /// See <see cref="CyclopsState"/> for example usage. 
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal virtual void Update(CyclopsStateUpdateContext updateContext)
        {
            OnUpdate(updateContext);
        }
        
        internal void StopImmediately()
        {
            bool wasActive = IsActive;
            
            IsActive = false;
            _exitCancellationTokenSource.Cancel();
            _exitCancellationTokenSource.Dispose();
            _exitCancellationTokenSource = null;
            
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
        /// Add an exit transition from this state to a target state that occurs when this state has stopped.
        /// Feel free to add as many transitions as needed.
        /// Transitions can not be removed, nor should they be.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        public void AddExitTransition(CyclopsBaseState target)
        {
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => !IsActive });
        }
        
        /// <summary>
        /// <see cref="QueryTransitions"/> is called by the hosting state machine and should not be otherwise called.
        /// <seealso cref="CyclopsStateMachine"/>
        /// </summary>
        internal bool QueryTransitions(out CyclopsBaseState nextBaseState)
        {
            nextBaseState = null;

            foreach (CyclopsStateTransition transition in _transitions)
            {
                if (!transition.Condition())
                    continue;
                
                nextBaseState = transition.Target;

                return true;
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
        /// </summary>
        protected virtual void OnUpdate(CyclopsStateUpdateContext updateContext) { }
        
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
        
        public async Awaitable WaitForSecondsAsync(float seconds)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(seconds, ExitCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        
        public async Awaitable FixedUpdateAsync()
        {
            try
            {
                await Awaitable.FixedUpdateAsync(ExitCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        
        public async Awaitable FromAsyncOperation(AsyncOperation op)
        {
            try
            {
                await Awaitable.FromAsyncOperation(op, ExitCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        
        public async Awaitable NextFrameAsync()
        {
            try
            {
                await Awaitable.NextFrameAsync(ExitCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        
        public async Awaitable EndOfFrameAsync()
        {
            try
            {
                await Awaitable.EndOfFrameAsync(ExitCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        
        public void AddTransition(CyclopsBaseState target, ref Action multicastDelegate)
        {
            Action localMulticastDelegate = null;
            bool wasTriggered = false;
            
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => wasTriggered} );
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction()
            {
                wasTriggered = true;
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
            }
        }
        
        public void AddTransition<T>(CyclopsBaseState target, ref Action<T> multicastDelegate)
        {
            Action<T> localMulticastDelegate = null;
            bool wasTriggered = false;
            
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => wasTriggered} );
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T x)
            {
                wasTriggered = true;
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
            }
        }
        
        public void AddTransition<T1, T2>(CyclopsBaseState target, ref Action<T1, T2> multicastDelegate)
        {
            Action<T1, T2> localMulticastDelegate = null;
            bool wasTriggered = false;
            
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => wasTriggered} );
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y)
            {
                wasTriggered = true;
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
            }
        }
        
        public void AddTransition<T1, T2, T3>(CyclopsBaseState target, ref Action<T1, T2, T3> multicastDelegate)
        {
            Action<T1, T2, T3> localMulticastDelegate = null;
            bool wasTriggered = false;
            
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => wasTriggered} );
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y, T3 z)
            {
                wasTriggered = true;
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
            }
        }
        
        public void AddTransition<T1, T2, T3, T4>(CyclopsBaseState target, ref Action<T1, T2, T3, T4> multicastDelegate)
        {
            Action<T1, T2, T3, T4> localMulticastDelegate = null;
            bool wasTriggered = false;
            
            AddTransition(new CyclopsStateTransition { Target = target, Condition = () => wasTriggered} );
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y, T3 z, T4 w)
            {
                wasTriggered = true;
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
            }
        }
        
        public void ExitOnAction(ref Action multicastDelegate)
        {
            Action localMulticastDelegate = null;
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction()
            {
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
                Stop();
            }
        }
        
        public void ExitOnAction<T>(ref Action<T> multicastDelegate)
        {
            Action<T> localMulticastDelegate = null;
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T x)
            {
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
                Stop();
            }
        }
        
        public void ExitOnAction<T1, T2>(ref Action<T1, T2> multicastDelegate)
        {
            Action<T1, T2> localMulticastDelegate = null;
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y)
            {
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
                Stop();
            }
        }
        
        public void ExitOnAction<T1, T2, T3>(ref Action<T1, T2, T3> multicastDelegate)
        {
            Action<T1, T2, T3> localMulticastDelegate = null;
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y, T3 z)
            {
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
                Stop();
            }
        }
        
        public void ExitOnAction<T1, T2, T3, T4>(ref Action<T1, T2, T3, T4> multicastDelegate)
        {
            Action<T1, T2, T3, T4> localMulticastDelegate = null;
            
            multicastDelegate += OnAction;
            localMulticastDelegate = multicastDelegate;
            Debug.Assert(localMulticastDelegate is not null, "Multicast delegate must not be null.");

            return;
            
            void OnAction(T1 x, T2 y, T3 z, T4 w)
            {
                // ReSharper disable once AccessToModifiedClosure
                localMulticastDelegate -= OnAction;
                Stop();
            }
        }
    }
}
