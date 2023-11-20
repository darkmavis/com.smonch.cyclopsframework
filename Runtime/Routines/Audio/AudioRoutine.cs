// Cyclops Framework
// 
// Copyright 2010 - 2023 Mark Davis
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
        private AudioSource _source;
        private bool _shouldRestoreAudioSourceSettings;

        private struct AudioSettings
        {
            public AudioClip Clip;
            public float Volume;
            public float Pitch;
            public float PanStereo;
            public Vector3 WorldPosition;
        }

        private AudioSettings _prevSettings;
        private AudioSettings _currSettings;

        public static AudioRoutine Instantiate(
            AudioSource source,
            AudioClip clip = null,
            float pitch = 1f,
            float volume = 1f,
            float panStereo = 0f,
            Vector3? worldPosition = null,
            double cycles = 1f,
            bool shouldRestoreAudioSourceSettings = true)
        {
            float length = clip == null ? source.clip.length : clip.length;
            var result = InstantiateFromPool<AudioRoutine>(length / pitch, cycles, ease:null);

            result._source = source;
            result._currSettings.Clip = clip;
            result._currSettings.Pitch = pitch;
            result._currSettings.Volume = volume;
            result._currSettings.PanStereo = panStereo;
            result._currSettings.WorldPosition = worldPosition ?? source.transform.position;
            result._shouldRestoreAudioSourceSettings = shouldRestoreAudioSourceSettings;

            return result;
        }

        protected override void OnRecycle()
        {
            _source = null;
            _prevSettings.Clip = null;
            _currSettings.Clip = null;
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
                _prevSettings.Clip = _source.clip;
                _prevSettings.Pitch = _source.pitch;
                _prevSettings.Volume = _source.volume;
                _prevSettings.PanStereo = _source.panStereo;
                _prevSettings.WorldPosition = _source.transform.position;
            }
        }

        protected override void OnFirstFrame()
        {
            _source.pitch = _currSettings.Pitch;
            _source.volume = _currSettings.Volume;
            _source.panStereo = _currSettings.PanStereo;
            _source.transform.position = _currSettings.WorldPosition;
            _source.PlayOneShot(_currSettings.Clip);
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
                _source.clip = _prevSettings.Clip;
                _source.pitch = _prevSettings.Pitch;
                _source.volume = _prevSettings.Volume;
                _source.panStereo = _prevSettings.PanStereo;
                _source.transform.position = _prevSettings.WorldPosition;
            }
        }
    }
}
