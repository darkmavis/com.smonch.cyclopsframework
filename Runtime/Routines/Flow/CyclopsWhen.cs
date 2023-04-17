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
    public class CyclopsWhen : CyclopsRoutine
    {
        private Func<bool> _f;
        private Action _g;
        private bool _wasSuccessful = false;
		
        public static CyclopsWhen Instantiate(Func<bool> f, Action g = null, double timeout = double.MaxValue)
        {
            var result = InstantiateFromPool<CyclopsWhen>(timeout);

            result._f = f;
            result._g = g;

            return result;
        }

        protected override void OnRecycle()
        {
            _f = null;
            _g = null;
            _wasSuccessful = false;
        }

        protected override void OnUpdate(float t)
        {
            if (_f())
			{
                _g?.Invoke();
                _wasSuccessful = true;
                Stop();
			}
        }

        protected override void OnExit()
        {
            // Close enough?
            if ((Math.Abs(Position - Period) < .00001d) && !_wasSuccessful)
                Fail();
        }
    }
}