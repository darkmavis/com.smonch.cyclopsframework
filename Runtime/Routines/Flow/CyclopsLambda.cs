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
    public class CyclopsLambda : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsLambda);
		
		private Action _f;

        public static CyclopsLambda Instantiate(Action f)
        {
            var result = InstantiateFromPool<CyclopsLambda>(tag: Tag);

            result._f = f;

            return result;
        }

        public static CyclopsLambda Instantiate(double period, double maxCycles, Action f)
        {
            var result = InstantiateFromPool<CyclopsLambda>(period, maxCycles, bias: null, tag: Tag);
            
            result._f = f;

            return result;
        }

        protected override void OnRecycle()
        {
            _f = null;
        }

        protected override void OnFirstFrame()
        {
            _f();
        }
    }
}
