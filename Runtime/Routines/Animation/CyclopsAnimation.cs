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
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsAnimation);

        private GameObject _target;
        private AnimationClip _clip;

        public static CyclopsAnimation Instantiate(GameObject target, AnimationClip clip, float cycles = 1f)
        {
            var result = InstantiateFromPool<CyclopsAnimation>(clip.length, cycles, bias:null, Tag);

            result._target = target;
            result._clip = clip;

            return result;
        }

        protected override void OnRecycle()
        {
            _target = null;
            _clip = null;
        }

        protected override void OnUpdate(float t)
        {
            _clip.SampleAnimation(_target, _clip.length * t);
        }
    }
}
