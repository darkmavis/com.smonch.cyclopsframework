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
    public class TweenAudioSourceVolume : CyclopsRoutine
    {
        public static readonly string Tag = TagPrefix_Cyclops + nameof(TweenAudioSourceVolume);

        private AudioSource _source;
        private Tween1f _tween;

        public TweenAudioSourceVolume(
            AudioSource source,
            float? fromVolume,
            float? toVolume,
            float period = 0,
            float cycles = 1,
            System.Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _source = source;
            _tween.SetFromTo(fromVolume, toVolume);
		}

        public static TweenAudioSourceVolume Instantiate(
            AudioSource source,
            float? fromVolume,
            float? toVolume,
            float period = 0f,
            float cycles = 1f,
            System.Func<float, float> bias = null)
        {
            if (TryInstantiateFromPool(() => new TweenAudioSourceVolume(source, fromVolume, toVolume, period, cycles, bias), out var result))
            {
                result.Period = period;
                result.MaxCycles = cycles;
                result.Bias = bias;

                result._source = source;
                result._tween.SetFromTo(fromVolume, toVolume);
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
            _tween.Fallback = _source.volume;
        }

        protected override void OnUpdate(float t)
        {
            _source.volume = _tween.Evaluate(t);
        }
    }
}