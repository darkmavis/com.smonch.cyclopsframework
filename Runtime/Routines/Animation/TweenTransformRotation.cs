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
    public class TweenTransformRotation : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenTransformRotation";

        private Transform _transform;
        private Quaternion? _fromRotation;
        private Quaternion? _toRotation;
        private Quaternion _a;
        private Quaternion _b;

        public TweenTransformRotation(Transform transform, Quaternion? fromRotation = null, Quaternion? toRotation = null, float period = 0f, float cycles = 1f, Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _transform = transform;
            _fromRotation = fromRotation;
            _toRotation = toRotation;
        }

        protected override void OnEnter()
        {
            _a = _fromRotation ?? _transform.rotation;
            _b = _toRotation ?? _transform.rotation;
        }

        protected override void OnUpdate(float t)
        {
            _transform.rotation = Quaternion.Slerp(_a, _b, t);
        }
    }
}
