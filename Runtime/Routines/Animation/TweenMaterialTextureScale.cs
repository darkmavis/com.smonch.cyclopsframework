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
        public const string Tag = TagPrefix_Cyclops + nameof(TweenMaterialTextureScale);

        private Material _material;
        private int _nameId;
        private Tween2f _tween;

        private TweenMaterialTextureScale(
            Material material,
            int nameId,
            Vector2? fromUvScale = null,
            Vector2? toUvScale = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _nameId = nameId;
            _tween.SetFromTo(fromUvScale, toUvScale);
        }

        public static TweenMaterialTextureScale Instantiate(
            Material material,
            int nameId,
            Vector2? fromUvScale = null,
            Vector2? toUvScale = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenMaterialTextureScale(material, nameId, fromUvScale, toUvScale, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._material = material;
                result._nameId = nameId;
                result._tween.SetFromTo(fromUvScale, toUvScale);
            }

            return result;
        }

        protected override void OnRecycle()
        {
            _material = null;
            _tween.Reset();
        }

        protected override void OnEnter()
        {
            _tween.Fallback = _material.GetTextureScale(_nameId);
        }

        protected override void OnUpdate(float t)
        {
            _material.SetTextureScale(_nameId, _tween.Evaluate(t));
        }
    }
}
