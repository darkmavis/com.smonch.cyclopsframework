// Cyclops Framework
// 
// Copyright 2010 - 2023 Mark Davis
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
    public class TweenMaterialVector : CyclopsRoutine
    {
        private Material _material;
        private int _propertyId;
        private Tween4F _tween;

        public static TweenMaterialVector Instantiate(
            Material material,
            int propertyId,
            Vector4? fromVector = null,
            Vector4? toVector = null,
            double period = 0,
            double cycles = 1,
            Func<float, float> bias = null)
        {
            var result = InstantiateFromPool<TweenMaterialVector>(period, cycles, bias);

            result._material = material;
            result._propertyId = propertyId;
            result._tween.SetFromTo(fromVector, toVector);

            return result;
        }

        protected override void OnRecycle()
        {
            _material = null;
            _tween.Reset();
        }

        protected override void OnEnter()
        {
            _tween.Fallback = _material.GetVector(_propertyId);
        }

        protected override void OnUpdate(float t)
        {
            _material.SetVector(_propertyId, _tween.Evaluate(t));
        }
    }
}
