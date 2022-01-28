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

using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class TweenAudioSourcePan : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenAudioSourcePan";

        private AudioSource _source;
        private Tween1f _tween;

        public TweenAudioSourcePan(
            AudioSource source,
            float? fromPan,
            float? toPan,
            float period = 0f,
            float cycles = 1f,
            System.Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _source = source;
            _tween.SetFromTo(fromPan, toPan);
        }

        public static TweenAudioSourcePan Instantiate(
            AudioSource source,
            float? fromPan,
            float? toPan,
            float period = 0f,
            float cycles = 1f,
            System.Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenAudioSourcePan(source, fromPan, toPan, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._source = source;
                result._tween.SetFromTo(fromPan, toPan);
            }

            return result;
        }

        protected override void OnRecycle()
        {
            _source = null;
            _tween.Reset();
        }
        protected override void OnEnter()
        {
            _tween.Fallback = _source.panStereo;
        }

        protected override void OnUpdate(float t)
        {
            _source.panStereo = _tween.Evaluate(t);
        }
    }
}