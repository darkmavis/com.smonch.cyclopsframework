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
using Assert = UnityEngine.Assertions.Assert;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smonch.CyclopsFramework
{
    public class CyclopsGame
    {
        private bool _isActive = false;
        private bool _isUsingAutomaticUpdateMode = false;
        private PlayerLoopSystem _originalPlayerLoopSystem;
        private Vector2Int _screenSize;

        public enum UpdateMode
        {
            Automatic,
            Manual
        }

        protected CyclopsStateMachine StateMachine { get; } = new();
        public bool IsQuitting { get; private set; }

        public Action EarlyUpdateFinished { get; set; }
        public Action FixedUpdateStarting { get; set; }
        public Action FixedUpdateFinished { get; set; }
        public Action LateUpdateStarting { get; set; }
        public Action LateUpdateFinished { get; set; }
        public Action ScreenSizeChanged { get; set; }
        
        public CyclopsGame() { }

        public void Start(CyclopsBaseState initialState, UpdateMode updateMode)
        {
            Assert.IsFalse(_isActive, $"{nameof(CyclopsGame)} was already started.");

            if (_isActive)
                return;

            _isActive = true;

            if (updateMode == UpdateMode.Automatic)
                _isUsingAutomaticUpdateMode = true;

            _screenSize = new Vector2Int(Screen.width, Screen.height);

            StateMachine.PushState(initialState);
            
            _originalPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            
            PlayerLoopSystem cyclopsLoopSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = UpdateStateMachine
            };
            
            PlayerLoopSystem earlyUpdateFinishedSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => EarlyUpdateFinished?.Invoke()
            };
            
            PlayerLoopSystem fixedUpdateStartingSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => FixedUpdateStarting?.Invoke()
            };
            
            PlayerLoopSystem fixedUpdateFinishedSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => FixedUpdateFinished?.Invoke()
            };
            
            PlayerLoopSystem lateUpdateStartingSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => LateUpdateStarting?.Invoke()
            };
            
            PlayerLoopSystem lateUpdateFinishedSystem = new PlayerLoopSystem
            {
                type = typeof(CyclopsGame),
                updateDelegate = () => LateUpdateFinished?.Invoke()
            };
            
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var existingSubsystems = currentPlayerLoop.subSystemList;
            var modifiedSubsystems = new List<PlayerLoopSystem>(existingSubsystems.Length + 1);

            foreach (var subsystem in existingSubsystems)
            {
                // Run after PlayerLoop.Update completes each frame.
                if (_isUsingAutomaticUpdateMode && (subsystem.type == typeof(UnityEngine.PlayerLoop.Update)))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(cyclopsLoopSystem);
                }
                else if (subsystem.type == typeof(UnityEngine.PlayerLoop.EarlyUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(earlyUpdateFinishedSystem);
                }
                else if (subsystem.type == typeof(UnityEngine.PlayerLoop.FixedUpdate))
                {
                    modifiedSubsystems.Add(fixedUpdateStartingSystem);
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(fixedUpdateFinishedSystem);
                }
                else if (subsystem.type == typeof(UnityEngine.PlayerLoop.PreLateUpdate))
                {
                    modifiedSubsystems.Add(subsystem);
                    modifiedSubsystems.Add(lateUpdateStartingSystem);
                }
                else if (subsystem.type == typeof(UnityEngine.PlayerLoop.PostLateUpdate))
                {
                    modifiedSubsystems.Add(lateUpdateFinishedSystem);
                    modifiedSubsystems.Add(subsystem);
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

            UpdateStateMachine();
        }
        
        public void PushState(CyclopsBaseState state)
        {
            StateMachine.PushState(state);
        }
        
        private void UpdateStateMachine()
        {
            // Checking because this could continue to get called in the Editor even after play mode has ended.
            // Not a problem as long as we stop it here.
            if (!Application.isPlaying)
            {
                Stop();
            }
            else
            {
                StateMachine.Update();
                
                // Unity will not notify on screen size changes. This is useful enough to handle here.
                var currentScreenSize = new Vector2Int(Screen.width, Screen.height);

                if (currentScreenSize != _screenSize)
                {
                    _screenSize = currentScreenSize;
                    ScreenSizeChanged?.Invoke();
                }
            }
        }
    }
}
