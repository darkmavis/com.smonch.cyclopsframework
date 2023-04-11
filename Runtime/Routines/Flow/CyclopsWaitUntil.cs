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

using System;

namespace Smonch.CyclopsFramework
{
    public class CyclopsWaitUntil : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsWaitUntil);
		
        private Func<bool> _f;
        private bool _wasSuccessful;

        public static CyclopsWaitUntil Instantiate(Func<bool> f)
        {
            var result = InstantiateFromPool<CyclopsWaitUntil>(double.MaxValue, tag: Tag);
            
            result._f = f;
            
            return result;
        }

        public static CyclopsWaitUntil Instantiate(Func<bool> f, double timeout)
        {
            var result = InstantiateFromPool<CyclopsWaitUntil>(timeout, tag: Tag);

            result._f = f;
            
            return result;
        }

        protected override void OnRecycle()
        {
            _f = null;
            _wasSuccessful = false;
        }

        protected override void OnUpdate(float t)
        {
            if (_f())
            {
                _wasSuccessful = true;
                Stop();
            }
        }

        protected override void OnExit()
        {
            // Note: float.Epsilon is not a typo.
            bool isCloseEnough = (Position >= (Period - float.Epsilon))
                && (Position <= (Period + float.Epsilon));
            
            if (isCloseEnough && !_wasSuccessful)
                Fail();
        }
    }
}