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
    public class TweenTransformPosition : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenTransformPosition";

        private Transform _transform;
        private Vector3? _fromPosition;
        private Vector3? _toPosition;
        private Vector3 _a;
        private Vector3 _b;

        public TweenTransformPosition(Transform transform, Vector3? fromPosition = null, Vector3? toPosition = null, float period = 0f, float cycles = 1f, Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _transform = transform;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        protected override void OnEnter()
        {
            _a = _fromPosition ?? _transform.position;
            _b = _toPosition ?? _transform.position;
        }

        protected override void OnUpdate(float t)
        {
            _transform.position = Vector3.Lerp(_a, _b, t);
        }
    }
}
