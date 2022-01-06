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

using System;
using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class TweenTransformScale : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenTransformScale";

        private Transform _transform;
        private Vector3? _fromScale;
        private Vector3? _toScale;
        private Vector3 _a;
        private Vector3 _b;

        public TweenTransformScale(Transform transform, Vector3? fromScale = null, Vector3? toScale = null, float period = 0f, float cycles = 1f, Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _transform = transform;
            _fromScale = fromScale;
            _toScale = toScale;
        }

        protected override void OnEnter()
        {
            _a = _fromScale ?? _transform.localScale;
            _b = _toScale ?? _transform.localScale;
        }

        protected override void OnUpdate(float t)
        {
            _transform.localScale = Vector3.Lerp(_a, _b, t);
        }
    }
}
