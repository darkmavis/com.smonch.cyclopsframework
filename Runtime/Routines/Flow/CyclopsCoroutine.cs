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
using System.Collections;

namespace Smonch.CyclopsFramework
{
    public sealed class CyclopsCoroutine : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsCoroutine);

        private Func<IEnumerator> _f;

        private CyclopsCoroutine(Func<IEnumerator> f)
            : base(double.MaxValue, 1, Tag)
        {
            _f = f;
        }
        
        public static CyclopsCoroutine Instantiate(Func<IEnumerator> f)
        {
            if (TryInstantiateFromPool(() => new CyclopsCoroutine(f), out var result))
            {
                result._f = f;
            }

            return result;
        }

        protected override void OnRecycle()
        {
            _f = null;
        }

        protected override void OnUpdate(float t)
        {
            if (!_f().MoveNext())
                Stop();
        }
    }
}