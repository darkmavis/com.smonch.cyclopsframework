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
    public class TweenMaterialTextureScale : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenMaterialTextureScale";

        private Material _material;
        private int _nameID;
        private Vector2? _fromUvScale;
        private Vector2? _toUvScale;
        private Vector2 _a;
        private Vector2 _b;

        public TweenMaterialTextureScale(
            Material material,
            string textureName,
            Vector2? fromUvScale = null,
            Vector2? toUvScale = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _nameID = Shader.PropertyToID(textureName);
            _fromUvScale = fromUvScale;
            _toUvScale = toUvScale;
        }

        public TweenMaterialTextureScale(
            Material material,
            int nameID,
            Vector2? fromUvScale = null,
            Vector2? toUvScale = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _nameID = nameID;
            _fromUvScale = fromUvScale;
            _toUvScale = toUvScale;
        }

        protected override void OnEnter()
        {
            _a = _fromUvScale ?? _material.GetTextureScale(_nameID);
            _b = _toUvScale ?? _material.GetTextureScale(_nameID);
        }

        protected override void OnUpdate(float t)
        {
            _material.SetTextureScale(_nameID, Vector2.Lerp(_a, _b, t));
        }
    }
}
