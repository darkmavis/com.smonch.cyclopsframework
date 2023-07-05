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

namespace Smonch.CyclopsFramework
{
    public class CyclopsStateMachine
    {
        private readonly Stack<CyclopsBaseState> _stateStack = new();

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

        public void Update()
        {
            if (_stateStack.Count == 0)
            {
                IsIdle = true;
            }
            else
            {
                IsIdle = false;

                var activeState = _stateStack.Peek();

                if (_stateStack.Count > 1)
                {
                    foreach (var state in _stateStack)
                    {
                        if (state != activeState)
                            state.Update(isLayeredUpdate: true);
                    }
                }

                if (!activeState.IsActive)
                    activeState.Start();
                
                if (activeState.QueryTransitions(out var nextState))
                {
                    activeState.Stop();
                }
                else
                {
                    activeState.Update();

                    if (activeState.QueryTransitions(out nextState))
                        activeState.Stop();
                }

                if (activeState.IsStopping)
                {
                    activeState.StopImmediately();
                    _stateStack.Pop();

                    // Still required? Check this.
                    if (nextState == null)
                        activeState.QueryTransitions(out nextState);
                }

                if (nextState != null)
                    _stateStack.Push(nextState);
            }
        }
    }
}
