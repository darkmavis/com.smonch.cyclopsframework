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
    public class TweenTransformPosition : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(TweenTransformPosition);

        private Transform _transform;
        private Tween3f _tween;

        private TweenTransformPosition(
            Transform transform,
            Vector3? fromPosition = null,
            Vector3? toPosition = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _transform = transform;
            _tween.SetFromTo(fromPosition, toPosition);
        }

        public static TweenTransformPosition Instantiate(
            Transform transform,
            Vector3? fromPosition = null,
            Vector3? toPosition = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenTransformPosition(transform, fromPosition, toPosition, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._transform = transform;
                result._tween.SetFromTo(fromPosition, toPosition);
            }

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
        }

        protected override void OnUpdate(float t)
        {
            _transform.position = _tween.Evaluate(t);
        }
    }
}
