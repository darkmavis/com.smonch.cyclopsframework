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

using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smonch.CyclopsFramework
{
    /// <summary>
    /// CyclopsGame is designed to be started via a bootstrap method of your choice.<br/>
    /// If it suits your needs, implement via Awake in a Monobehaviour.<br/>
    /// CyclopsGame contains and drives a traditional state machine based on CyclopsStateMachine.
    /// </summary>
    public sealed class CyclopsGame
    {
        private bool _isActive;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public CyclopsGameStateMachine StateMachine { get; } = new();
        
        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsQuitting { get; private set; }
        
        public void Start(CyclopsBaseState initialState)
        {
            Assert.IsFalse(_isActive, $"{nameof(CyclopsGame)} was already started.");
            _isActive = true;
            StateMachine.PushState(initialState);
            Application.exitCancellationToken.Register(callback: Quit, useSynchronizationContext: true);
        }
        
        public void Update()
        {
            StateMachine.Update();
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Quit()
        {
            if (IsQuitting)
                return;

            IsQuitting = true;
            StateMachine.ForceStop();
            _isActive = false;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
