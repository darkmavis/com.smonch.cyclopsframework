﻿// Cyclops Framework
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

namespace Smonch.CyclopsFramework
{
    public class CyclopsNop : CyclopsRoutine
    {
        public const string Tag = Tag_Nop;

        public static CyclopsNop Instantiate(double cycles)
        {
            var result = InstantiateFromPool<CyclopsNop>(period: 0, cycles, tag: Tag);
            
            result.MustRecycleIfPooled = false;

            return result;
        }
    }
}
