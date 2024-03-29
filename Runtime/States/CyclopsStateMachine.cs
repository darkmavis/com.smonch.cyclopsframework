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

using System.Collections.Generic;

namespace Smonch.CyclopsFramework
{
    public class CyclopsStateMachine
    {
        private readonly LinkedList<CyclopsBaseState> _stateLinkedStack = new();
        private CyclopsBaseState _nextState;
        private bool _isForceStopping;
        
        public CyclopsBaseState Context { get; private set; }
        
        public bool IsIdle { get; private set; }

        public void PushState(CyclopsBaseState baseState)
        {
            _stateLinkedStack.AddLast(baseState);
        }

        public void ForceStop()
        {
            while (_stateLinkedStack.Count > 0)
            {
                _stateLinkedStack.Last.Value.StopImmediately();
                _stateLinkedStack.RemoveLast();
            }

            _isForceStopping = true;
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public void Update()
        {
            Update(CyclopsGame.UpdateSystem.Update);
            Update(CyclopsGame.UpdateSystem.CyclopsStateMachinePostUpdate);
        }

        internal void Update(CyclopsGame.UpdateSystem updateSystem)
        {
            if (_stateLinkedStack.Count == 0)
            {
                IsIdle = true;
            }
            else
            {
                IsIdle = false;

                CyclopsBaseState topState = _stateLinkedStack.Last.Value;
                
                Context = topState;
                
                if (updateSystem == CyclopsGame.UpdateSystem.CyclopsStateMachinePostUpdate)
                {
                    if (topState.IsStopping)
                    {
                        topState.StopImmediately();
                        _stateLinkedStack.RemoveLast();
                        
                        if (_nextState is null)
                            topState.QueryTransitions(out _nextState);
                    }

                    if (_nextState is not null)
                        _stateLinkedStack.AddLast(_nextState);

                    return;
                }
                
                foreach (CyclopsBaseState state in _stateLinkedStack)
                {
                    if (state == topState)
                        continue;
                    
                    // Could set this to null later, but would rather not.
                    Context = state;
                    
                    // In case of pushing multiple states onto the stack quickly.
                    if (!state.IsActive)
                        state.Start();
                    
                    state.Update(new CyclopsStateUpdateContext { UpdateSystem = updateSystem, IsLayered = true });

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
                    topState.Update(new CyclopsStateUpdateContext { UpdateSystem = updateSystem, IsLayered = false });
                    
                    if (topState.QueryTransitions(out _nextState))
                        topState.Stop();
                }
            }
        }
    }
}
