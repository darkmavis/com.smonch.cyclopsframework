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

using System.Collections.Generic;
using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class CyclopsStateMachine
    {
        private readonly Stack<CyclopsBaseState> _stateStack = new();
        private CyclopsBaseState _nextState;

        public bool IsIdle { get; private set; }

        public void PushState(CyclopsBaseState baseState)
        {
            _stateStack.Push(baseState);
        }

        public void ForceStop()
        {
            if (_stateStack.TryPeek(out var state))
                state.StopImmediately();
        }

        public void Update(CyclopsGame.UpdateSystem updateSystem)
        {
            if (_stateStack.Count == 0)
            {
                IsIdle = true;
            }
            else
            {
                IsIdle = false;

                CyclopsBaseState topState = _stateStack.Peek();
                
                if (updateSystem == CyclopsGame.UpdateSystem.CyclopsStateMachinePostUpdate)
                {
                    if (topState.IsStopping)
                    {
                        topState.StopImmediately();
                        _stateStack.Pop();
                        
                        if (_nextState == null)
                            topState.QueryTransitions(out _nextState);
                    }

                    if (_nextState != null)
                        _stateStack.Push(_nextState);

                    return;
                }

                foreach (CyclopsBaseState state in _stateStack)
                {
                    if (state == topState)
                        continue;
                    
                    // In case of pushing multiple states onto the stack quickly.
                    if (!state.IsActive)
                        state.Start();
                    
                    switch (updateSystem)
                    {
                        case CyclopsGame.UpdateSystem.TimeUpdate:
                            state.TimeUpdate(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.InitializationUpdate:
                            state.PlayerLoopInitialization(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.EarlyUpdate:
                            state.EarlyUpdate(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.FixedUpdate:
                            state.FixedUpdate(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.PreUpdate:
                            state.PreUpdate(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.Update:
                            state.Update(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.PreLateUpdate:
                            state.PreLateUpdate(isLayeredUpdate: true);
                            break;
                        case CyclopsGame.UpdateSystem.PostLateUpdate:
                            state.PostLateUpdate(isLayeredUpdate: true);
                            break;
                        default:
                            Debug.LogWarning("Unexpected UpdateSystem encountered.");
                            break;
                    }
                }

                if (!topState.IsActive)
                    topState.Start();
                
                if (topState.QueryTransitions(out _nextState))
                {
                    topState.Stop();
                }
                else
                {
                    switch (updateSystem)
                    {
                        case CyclopsGame.UpdateSystem.InitializationUpdate:
                            topState.PlayerLoopInitialization(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.EarlyUpdate:
                            topState.EarlyUpdate(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.FixedUpdate:
                            topState.FixedUpdate(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.PreUpdate:
                            topState.PreUpdate(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.Update:
                            topState.Update(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.PreLateUpdate:
                            topState.PreLateUpdate(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.PostLateUpdate:
                            topState.PostLateUpdate(isLayeredUpdate: false);
                            break;
                        case CyclopsGame.UpdateSystem.TimeUpdate:
                            topState.TimeUpdate(isLayeredUpdate: false);
                            break;
                        default:
                            Debug.LogWarning("Unexpected UpdateSystem encountered.");
                            break;
                    }

                    if (topState.QueryTransitions(out _nextState))
                        topState.Stop();
                }
            }
        }
    }
}
