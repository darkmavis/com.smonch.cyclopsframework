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
    public class TweenMaterialPropertyBlockColor : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenMaterialPropertyBlockColor";

        private MaterialPropertyBlock _block;
        private int _propertyId;
        private Color? _fromColor;
        private Color? _toColor;
        private Color _a;
        private Color _b;

        public TweenMaterialPropertyBlockColor(
            MaterialPropertyBlock block,
            int propertyId,
            Color? fromColor = null,
            Color? toColor = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = propertyId;
            _fromColor = fromColor;
            _toColor = toColor;
        }

        public TweenMaterialPropertyBlockColor(
            MaterialPropertyBlock block,
            string propertyName,
            Color? fromColor = null,
            Color? toColor = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _block = block;
            _propertyId = Shader.PropertyToID(propertyName);
            _fromColor = fromColor;
            _toColor = toColor;
        }

        protected override void OnEnter()
        {
            _a = _fromColor ?? _block.GetColor(_propertyId);
            _b = _toColor ?? _block.GetColor(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _block.SetColor(_propertyId, Color.Lerp(_a, _b, t));
        }
    }
}
