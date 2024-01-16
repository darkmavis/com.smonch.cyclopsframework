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

using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public class TweenAudioSourceVolume : CyclopsRoutine
    {
        private AudioSource _source;
        private Tween1F _tween;

        public static TweenAudioSourceVolume Instantiate(
            AudioSource source,
            float? fromVolume,
            float? toVolume,
            double period = 0,
            double cycles = 1,
            System.Func<float, float> bias = null)
        {
            var result = InstantiateFromPool<TweenAudioSourceVolume>(period, cycles, bias);
            
            result._source = source;
            result._tween.SetFromTo(fromVolume, toVolume);

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