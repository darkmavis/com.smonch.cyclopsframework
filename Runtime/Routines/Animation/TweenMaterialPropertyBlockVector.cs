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
    public class TweenMaterialPropertyBlockVector : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenMaterialPropertyBlockVector";

        private MaterialPropertyBlock _block;
        private int _propertyId;
        private Vector4? _fromVector;
        private Vector4? _toVector;
        private Vector4 _a;
        private Vector4 _b;

        public TweenMaterialPropertyBlockVector(
            MaterialPropertyBlock block,
            int propertyId,
            Vector4? fromVector = null,
            Vector4? toVector = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = propertyId;
            _fromVector = fromVector;
            _toVector = toVector;
        }

        public TweenMaterialPropertyBlockVector(
            MaterialPropertyBlock block,
            string propertyName,
            Vector4? fromVector = null,
            Vector4? toVector = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = Shader.PropertyToID(propertyName);
            _fromVector = fromVector;
            _toVector = toVector;
        }

        protected override void OnEnter()
        {
            _a = _fromVector ?? _block.GetVector(_propertyId);
            _b = _toVector ?? _block.GetVector(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _block.SetVector(_propertyId, Vector4.Lerp(_a, _b, t));
        }
    }
}
