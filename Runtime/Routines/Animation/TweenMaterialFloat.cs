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
    public class TweenMaterialFloat : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(TweenMaterialFloat);

        private Material _material;
        private int _propertyId;
        private Tween1f _tween;
        
        private TweenMaterialFloat(
            Material material,
            int propertyId,
            float? fromValue = null,
            float? toValue = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _material = material;
            _propertyId = propertyId;
            _tween.SetFromTo(fromValue, toValue);
        }

        public static TweenMaterialFloat Instantiate(
            Material material,
            int propertyId,
            float? fromValue = null,
            float? toValue = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenMaterialFloat(material, propertyId, fromValue, toValue, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._material = material;
                result._propertyId = propertyId;
                result._tween.SetFromTo(fromValue, toValue);
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
            _tween.Fallback = _material.GetFloat(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _material.SetFloat(_propertyId, _tween.Evaluate(t));
        }
    }
}
