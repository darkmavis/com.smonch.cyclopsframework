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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Assert = UnityEngine.Assertions.Assert;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smonch.CyclopsFramework
{
    public class CyclopsGame
    {
        private bool _isActive;
        private bool _isUsingAutomaticUpdateMode;
        private PlayerLoopSystem _originalPlayerLoopSystem;
        private Vector2Int _screenSize;

        public enum UpdateMode
        {
            Automatic,
            ManualLimited
        }
        
        public enum UpdateSystem
        {
            CyclopsStateMachinePostUpdate,
            InitializationUpdate,
            EarlyUpdate,
            FixedUpdate,
            PreUpdate,
            Update,
            PreLateUpdate,
            PostLateUpdate,
            TimeUpdate
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        protected CyclopsStateMachine StateMachine { get; } = new();
        // ReSharper disable once MemberCanBePrivate.Global
        
        public bool IsQuitting { get; private set; }

        public event Action ScreenSizeChanged;
        
        public void Start(CyclopsBaseState initialState, UpdateMode updateMode = UpdateMode.Automatic)
        {
            Assert.IsFalse(_isActive, $"{nameof(CyclopsGame)} was already started.");

            if (_isActive)
                return;

            _isActive = true;

            if (updateMode == UpdateMode.Automatic)
                _isUsingAutomaticUpdateMode = true;
            
            _screenSize = new Vector2Int(Screen.width, Screen.height);

            StateMachine.PushState(initialState);

            if (!_isUsingAutomaticUpdateMode)
                return;
            
            _originalPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            
            var cylopsInitialization = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.InitializationUpdate)
            };
            
            var cylopsEarlyUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.EarlyUpdate)
            };
            
            var cyclopsPreUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.PreUpdate)
            };
            
            var cyclopsUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.Update)
            };
            
            var cyclopsFixedUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.FixedUpdate)
            };
            
            var cyclopsPreLateUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.PreLateUpdate)
            };
            
            var cyclopsPostLateUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.PostLateUpdate)
            };
            
            var cyclopsTimeUpdate = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => UpdateStateMachine(UpdateSystem.TimeUpdate)
            };
            
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var existingSubsystems = currentPlayerLoop.subSystemList;
            var modifiedSubsystems = new List<PlayerLoopSystem>(existingSubsystems.Length + 1);
            
            foreach (PlayerLoopSystem subsystem in existingSubsystems)
            {
                if (subsystem.type == typeof(Initialization))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cylopsInitialization);
                }
                else if (subsystem.type == typeof(EarlyUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cylopsEarlyUpdate);
                }
                else if (subsystem.type == typeof(PreUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsPreUpdate);
                }
                else if (subsystem.type == typeof(FixedUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsFixedUpdate);
                }
                else if (subsystem.type == typeof(Update))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsUpdate);
                }
                else if (subsystem.type == typeof(PreLateUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsPreLateUpdate);
                }
                else if (subsystem.type == typeof(PostLateUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsPostLateUpdate);
                }
                else if (subsystem.type == typeof(TimeUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsTimeUpdate);
                }
                else
                {
                    modifiedSubsystems.Add(subsystem);
                }
            }
            
            currentPlayerLoop.subSystemList = modifiedSubsystems.ToArray();
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            
            Application.exitCancellationToken.Register(callback: Quit, useSynchronizationContext: true);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Quit()
        {
            if (IsQuitting)
                return;

            IsQuitting = true;
            Stop();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Stop()
        {
            Assert.IsTrue((_isActive && Application.isPlaying)
                || (_isUsingAutomaticUpdateMode && _isActive && !Application.isPlaying),
                "Stop was already called and did not need to be called again.");
            
            StateMachine.ForceStop();
            _isActive = false;
            
            PlayerLoop.SetPlayerLoop(_originalPlayerLoopSystem);
        }

        public void Update()
        {
            Assert.IsFalse(_isUsingAutomaticUpdateMode, "Update should not be called manually while using UnityEngine.PlayerLoop.Update to drive updates.");
            Assert.IsTrue(_isActive, "Update should not be called unless CyclopsGame is active.");

            if (!_isActive)
                return;

            UpdateStateMachine(UpdateSystem.Update);
        }
        
        public void PushState(CyclopsBaseState state)
        {
            StateMachine.PushState(state);
        }
        
        private void UpdateStateMachine(UpdateSystem updateSystem)
        {
            // Checking because this could continue to get called in the Editor even after play mode has ended.
            // Not a problem as long as we stop it here.
            if (!Application.isPlaying)
            {
                Stop();
                return;
            }
            
            switch (updateSystem)
            {
                case UpdateSystem.TimeUpdate:
                    StateMachine.Update(UpdateSystem.TimeUpdate);
                    break;
                case UpdateSystem.InitializationUpdate:
                    StateMachine.Update(UpdateSystem.InitializationUpdate);
                    break;
                case UpdateSystem.EarlyUpdate:
                    StateMachine.Update(UpdateSystem.EarlyUpdate);
                    break;
                case UpdateSystem.FixedUpdate:
                    StateMachine.Update(UpdateSystem.FixedUpdate);
                    break;
                case UpdateSystem.PreUpdate:
                    StateMachine.Update(UpdateSystem.PreUpdate);
                    break;
                case UpdateSystem.Update:
                    // Unity will not notify on screen size changes. This is useful enough to handle here.
                    // This actually looks pretty ugly and we're not likely to do anything similar. Hmm.
                    var currentScreenSize = new Vector2Int(Screen.width, Screen.height);

                    if (currentScreenSize == _screenSize)
                    {
                        _screenSize = currentScreenSize;
                        ScreenSizeChanged?.Invoke();
                    }
                    
                    StateMachine.Update(UpdateSystem.Update);
                    break;
                case UpdateSystem.PreLateUpdate:
                    StateMachine.Update(UpdateSystem.PreLateUpdate);
                    break;
                case UpdateSystem.PostLateUpdate:
                    StateMachine.Update(UpdateSystem.PostLateUpdate);
                    StateMachine.Update(UpdateSystem.CyclopsStateMachinePostUpdate);
                    break;
            }
        }
    }
}
