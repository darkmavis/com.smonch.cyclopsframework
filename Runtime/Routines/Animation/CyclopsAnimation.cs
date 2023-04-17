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

using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class CyclopsAnimation : CyclopsRoutine
    {
        private Animation _animation;
        private AnimationState _state;

        public override bool IsPaused
        {
            get => base.IsPaused;
            set => _state.speed = (base.IsPaused = /* !cmp */ value) ? 0f : (float)Speed;
        }

        public static CyclopsAnimation Instantiate(GameObject target, Animation animation, string clipName = null, float cycles = 1f)
        {
            clipName ??= animation.clip.name;

            var result = InstantiateFromPool<CyclopsAnimation>(animation[clipName].length, cycles, bias:null);

            result._animation = animation;
            result._state = animation[clipName];

            return result;
        }

        protected override void OnRecycle()
        {
            _animation = null;
            _state = null;
        }

        protected override void OnFirstFrame()
        {
            _state.normalizedTime = 0f;
            _animation.Stop(_state.name);
            _animation.Play(_state.name);
        }

        protected override void OnUpdate(float t)
        {
            _state.speed = (float)Speed;
        }

        protected override void OnLastFrame()
        {
            if (!Mathf.Approximately(1f, _state.normalizedTime))
            {
                _state.normalizedTime = 1f;
                _animation.Sample();
            }
        }

        protected override void OnExit()
        {
            _animation.Stop(_state.name);
        }
    }
}
