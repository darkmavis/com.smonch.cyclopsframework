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
    public class TweenAudioSourcePitch : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "TweenAudioSourcePitch";

        AudioSource _source;
        float? _fromPitch;
        float? _toPitch;
        float _a;
        float _b;

        public TweenAudioSourcePitch(AudioSource source, float? fromPitch, float? toPitch, float period = 0f, float cycles = 1f, System.Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _source = source;
            _fromPitch = fromPitch;
            _toPitch = toPitch;
        }

        protected override void OnEnter()
        {
            _a = _fromPitch ?? _source.pitch;
            _b = _toPitch ?? _source.pitch;
        }

        protected override void OnUpdate(float t)
        {
            _source.pitch = Mathf.Lerp(_a, _b, t);
        }
    }
}