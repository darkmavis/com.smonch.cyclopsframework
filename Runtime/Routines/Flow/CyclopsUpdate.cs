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
    public class CyclopsUpdate : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsUpdate);

        private Action<float> _ft = null;

        public static CyclopsUpdate Instantiate(
            double period,
            double cycles,
            Func<float, float> bias,
            Action<float> ft)
        {
            var result = InstantiateFromPool<CyclopsUpdate>(period, cycles, bias, Tag);
            
            result._ft = ft;
            
            return result;
        }

        protected override void OnRecycle() => _ft = null;
        protected override void OnUpdate(float t) => _ft(t);
    }
}
