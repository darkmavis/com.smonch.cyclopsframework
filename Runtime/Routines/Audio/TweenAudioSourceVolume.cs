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
        public static readonly string Tag = TagPrefix_Cyclops + "TweenAudioSourceVolume";

        AudioSource _source;
        float? _fromVolume;
        float? _toVolume;
        float _a;
        float _b;

        public TweenAudioSourceVolume(AudioSource source, float? fromVolume, float? toVolume, float period = 0, float cycles = 1, System.Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _source = source;
            _fromVolume = fromVolume;
            _toVolume = toVolume;
		}

        protected override void OnEnter()
        {
            _a = _fromVolume ?? _source.volume;
            _b = _toVolume ?? _source.volume;
        }

        protected override void OnUpdate(float t)
        {
            _source.volume = Mathf.Lerp(_a, _b, t);
        }
    }
}