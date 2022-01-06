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
    public class TweenMaterialTextureOffset : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenMaterialTextureOffset";

        private Material _material;
        private int _nameID;
        private Vector2? _fromUV;
        private Vector2? _toUV;
        private Vector2 _a;
        private Vector2 _b;
        
        public TweenMaterialTextureOffset(
            Material material,
            string textureName,
            Vector2? fromUv = null,
            Vector2? toUv = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _nameID = Shader.PropertyToID(textureName);
            _fromUV = fromUv;
            _toUV = toUv;
        }

        public TweenMaterialTextureOffset(
            Material material,
            int nameID,
            Vector2? fromUv = null,
            Vector2? toUv = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _nameID = nameID;
            _fromUV = fromUv;
            _toUV = toUv;
        }

        protected override void OnEnter()
        {
            _a = _fromUV ?? _material.GetTextureOffset(_nameID);
            _b = _toUV ?? _material.GetTextureOffset(_nameID);
        }

        protected override void OnUpdate(float t)
        {
            _material.SetTextureOffset(_nameID, Vector2.Lerp(_a, _b, t));
        }
    }
}
