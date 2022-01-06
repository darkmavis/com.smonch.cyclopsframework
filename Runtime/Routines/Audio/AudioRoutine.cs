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
    public class AudioRoutine : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "AudioRoutine";

        private AudioSource _source;
        private bool _shouldRestoreAudioSourceSettings;

        private struct AudioSettings
        {
            public AudioClip clip;
            public float volume;
            public float pitch;
            public float panStereo;
            public Vector3 worldPosition;
        }

        private AudioSettings _prevSettings;
        private AudioSettings _currSettings;

        public AudioRoutine(
            AudioSource source,
            AudioClip clip = null,
            float pitch = 1f,
            float volume = 1f,
            float panStereo = 0f,
            Vector3? worldPosition = null,
            double cycles = 1.0,
            bool shouldRestoreAudioSourceSettings = true)
            : base(clip.length / pitch, cycles, null, Tag)
        {
            _source = source;
            _currSettings.clip = clip;
            _currSettings.pitch = pitch;
            _currSettings.volume = volume;
            _currSettings.panStereo = panStereo;
            _currSettings.worldPosition = worldPosition ?? source.transform.position;
            _shouldRestoreAudioSourceSettings = shouldRestoreAudioSourceSettings;
        }

        public override bool IsPaused
        {
            get => base.IsPaused;

            set
            {
                base.IsPaused = value;

                if (value)
                    _source.Pause();
                else
                    _source.UnPause();
            }
        }

        protected override void OnEnter()
        {
            if (_shouldRestoreAudioSourceSettings)
            {
                _prevSettings.clip = _source.clip;
                _prevSettings.pitch = _source.pitch;
                _prevSettings.volume = _source.volume;
                _prevSettings.panStereo = _source.panStereo;
                _prevSettings.worldPosition = _source.transform.position;
            }
        }

        protected override void OnFirstFrame()
        {
            _source.pitch = _currSettings.pitch;
            _source.volume = _currSettings.volume;
            _source.panStereo = _currSettings.panStereo;
            _source.transform.position = _currSettings.worldPosition;
            _source.PlayOneShot(_currSettings.clip);
        }

        protected override void OnLastFrame()
        {
            _source.Stop();
        }

        protected override void OnExit()
        {
            _source.Stop();

            if (_shouldRestoreAudioSourceSettings)
            {
                _source.clip = _prevSettings.clip;
                _source.pitch = _prevSettings.pitch;
                _source.volume = _prevSettings.volume;
                _source.panStereo = _prevSettings.panStereo;
                _source.transform.position = _prevSettings.worldPosition;
            }
        }
    }
}
