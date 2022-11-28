// Cyclops Framework
// 
// Copyright 2010 - 2022 Mark Davis
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
using UnityEngine.Assertions;
using UnityEngine.LowLevel;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsGame
    {
        private bool _isActive = false;
        private bool _isUsingAutomaticUpdateMode = false;

        public enum UpdateMode
        {
            Automatic,
            Manual
        }

        protected CyclopsStateMachine StateMachine { get; private set; } = new CyclopsStateMachine();
        public bool IsQuitting { get; private set; }

        public CyclopsGame() { }

        public void Start(CyclopsGameState initialState, UpdateMode updateMode)
        {
            Assert.IsFalse(_isActive, "CyclopsGame was already started.");

            if (_isActive)
                return;

            _isActive = true;

            if (updateMode == UpdateMode.Automatic)
                _isUsingAutomaticUpdateMode = true;

            StateMachine.PushState(initialState);

            if (_isUsingAutomaticUpdateMode)
            {
                var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

                for (int i = 0; i < currentPlayerLoop.subSystemList.Length; ++i)
                    if (currentPlayerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
                        currentPlayerLoop.subSystemList[i].updateDelegate = UpdateStateMachine;

                PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            }

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

        public void Stop()
        {
            Assert.IsTrue((_isActive && Application.isPlaying)
                || (_isUsingAutomaticUpdateMode && _isActive && !Application.isPlaying),
                "Stop was already called and did not need to be called again.");

            StateMachine.ForceStop();

            _isActive = false;

            if (_isUsingAutomaticUpdateMode)
            {
                var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

                for (int i = 0; i < currentPlayerLoop.subSystemList.Length; ++i)
                    if (currentPlayerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
                        currentPlayerLoop.subSystemList[i].updateDelegate = null;

                PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                _isUsingAutomaticUpdateMode = false;
            }
        }

        public void Update()
        {
            Assert.IsFalse(_isUsingAutomaticUpdateMode, "Update should not be called manually while using UnityEngine.PlayerLoop.Update to drive updates.");
            Assert.IsTrue(_isActive, "Update should not be called unless CyclopsGame is active.");

            if (!_isActive)
                return;

            UpdateStateMachine();
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
            }
        }
    }
}
