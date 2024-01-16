// Cyclops Framework
// 
// Copyright 2010 - 2024 Mark Davis
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
    public class TweenMaterialColor : CyclopsRoutine
    {
        private Material _material;
        private int _propertyId;
        private Tween4C _tween;

        public static TweenMaterialColor Instantiate(
            Material material,
            int propertyId,
            Color? fromColor = null,
            Color? toColor = null,
            double period = 0,
            double cycles = 1,
            Func<float, float> bias = null)
        {
            var result = InstantiateFromPool<TweenMaterialColor>(period, cycles, bias);

            result._material = material;
            result._propertyId = propertyId;
            result._tween.SetFromTo(fromColor, toColor);

            return result;
        }

        protected override void OnRecycle()
        {
            _material = null;
            _tween.Reset();
        }

        protected override void OnEnter()
        {
            _tween.Fallback = _material.GetColor(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _material.color = _tween.Evaluate(t);
        }
    }
}
