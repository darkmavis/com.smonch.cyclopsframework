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

        AudioSource _source;
        float? _fromPan;
        float? _toPan;
        float _a;
        float _b;

        public TweenAudioSourcePan(AudioSource source, float? fromPan, float? toPan, float period=0f, float cycles=1f, System.Func<float, float> bias = null)
            : base(period, cycles, bias, Tag)
        {
            _source = source;
            _fromPan = fromPan;
            _toPan = toPan;
		}

        protected override void OnEnter()
        {
            _a = _fromPan ?? _source.panStereo;
            _b = _toPan ?? _source.panStereo;
        }

        protected override void OnUpdate(float t)
        {
            _source.panStereo = Mathf.Lerp(_a, _b, t);
        }
    }
}