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

using System.Collections.Generic;
using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class CyclopsStateMachine
    {
        private readonly LinkedList<CyclopsBaseState> _stateLinkedStack = new();
        private readonly Queue<CyclopsBaseState> _pushQueue = new();
        private CyclopsBaseState _nextState;
        private bool _isForceStopping;
        
        public CyclopsBaseState Context { get; private set; }
        public bool IsIdle => _stateLinkedStack.Count == 0;
        
        private CyclopsBaseState TopState => _stateLinkedStack.Last.Value;

        public void PushState(CyclopsBaseState state)
        {
            state.IsForegroundState = true;
            _pushQueue.Enqueue(state);
        }

        public void ForceStop()
        {
            _pushQueue.Clear();
            
            while (_stateLinkedStack.Count != 0)
            {
                TopState.StopImmediately();
                _stateLinkedStack.RemoveLast();
            }

            _isForceStopping = true;
        }

        public void Update()
        {
            while (_pushQueue.TryDequeue(out CyclopsBaseState state))
            {
                _stateLinkedStack.AddLast(state);
            }

            if (IsIdle)
                return;
            
            CyclopsBaseState topState = TopState;
            
            foreach (CyclopsBaseState backgroundState in _stateLinkedStack)
            {
                if (backgroundState == topState)
                    continue;
                
                // Could set this to null later, but would rather not.
                Context = backgroundState;
                
                // In case of pushing multiple states onto the stack quickly.
                if (!backgroundState.IsActive)
                {
                    // We'll say this is a foreground state, but it won't be for long.
                    backgroundState.IsForegroundState = true;
                    backgroundState.Start(); // <-- calls: OnEnter
                }

                if (backgroundState.IsForegroundState)
                {
                    backgroundState.IsForegroundState = false;
                    backgroundState.JustEnteredBackgroundMode = true;
                }
                
                backgroundState.Update();

                if (_isForceStopping)
                    return;
            }

            Context = topState;

            if (!topState.IsActive)
                topState.Start();
            
            if (topState.QueryTransitions(out _nextState))
            {
                topState.Stop();
            }
            else
            {
                if (!topState.IsForegroundState)
                {
                    topState.IsForegroundState = true;
                    topState.JustExitedBackgroundMode = true;
                }
                
                topState.Update();
                
                if (topState.QueryTransitions(out _nextState))
                    topState.Stop();
            }
            
            if (topState.IsStopping)
            {
                topState.StopImmediately();
                _stateLinkedStack.RemoveLast();
            }

            if (_nextState is null)
                return;
            
            PushState(_nextState);
        }
    }
}
