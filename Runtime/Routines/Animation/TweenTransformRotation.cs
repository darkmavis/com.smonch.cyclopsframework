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
    public class TweenTransformRotation : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(TweenTransformRotation);

        private Transform _transform;
        private TweenQs _tween;

        private TweenTransformRotation(
            Transform transform,
            Quaternion? fromRotation = null,
            Quaternion? toRotation = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _transform = transform;
            _tween.SetFromTo(fromRotation, toRotation);
        }

        public static TweenTransformRotation Instantiate(
            Transform transform,
            Quaternion? fromRotation = null,
            Quaternion? toRotation = null,
            float period = 0f,
            float cycles = 1f,
            Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenTransformRotation(transform, fromRotation, toRotation, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._transform = transform;
                result._tween.SetFromTo(fromRotation, toRotation);
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
            _tween.Fallback = _transform.rotation;
        }

        protected override void OnUpdate(float t)
        {
            _transform.rotation = _tween.Evaluate(t);
        }
    }
}
