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
    public class TweenMaterialPropertyBlockFloat : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenMaterialPropertyBlockFloat";

        private MaterialPropertyBlock _block;
        private int _propertyId;
        private float? _fromValue;
        private float? _toValue;
        private float _a;
        private float _b;

        public TweenMaterialPropertyBlockFloat(
            MaterialPropertyBlock block,
            int propertyId,
            float? fromValue = null,
            float? toValue = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = propertyId;
            _fromValue = fromValue;
            _toValue = toValue;
        }

        public TweenMaterialPropertyBlockFloat(
            MaterialPropertyBlock block,
            string propertyName,
            float? fromValue = null,
            float? toValue = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = Shader.PropertyToID(propertyName);
            _fromValue = fromValue;
            _toValue = toValue;
        }

        protected override void OnEnter()
        {
            _a = _fromValue ?? _block.GetFloat(_propertyId);
            _b = _toValue ?? _block.GetFloat(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _block.SetFloat(_propertyId, Mathf.Lerp(_a, _b, t));
        }
    }
}
