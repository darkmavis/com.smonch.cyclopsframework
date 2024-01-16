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
    public class TweenTransformPosition : CyclopsRoutine
    {
        private Transform _transform;
        private Tween3F _tween;

        public static TweenTransformPosition Instantiate(
            Transform transform,
            Vector3? fromPosition = null,
            Vector3? toPosition = null,
            double period = 0,
            double cycles = 1,
            Func<float, float> bias = null)
        {
            var result = InstantiateFromPool<TweenTransformPosition>(period, cycles, bias);
            
            result._transform = transform;
            result._tween.SetFromTo(fromPosition, toPosition);
            
            return result;
        }

        protected override void OnRecycle()
        {
            _transform = null;
            _tween.Reset();
        }

        protected override void OnEnter()
        {
            _tween.Fallback = _transform.position;
            SyncAtStart();
        }

        protected override void OnUpdate(float t)
        {
            _transform.position = _tween.Evaluate(t);
        }
    }
}
