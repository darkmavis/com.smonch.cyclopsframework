// Cyclops Framework
// 
// Copyright 2010 - 2024 Mark Davis
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
using UnityEngine.Serialization;

namespace Smonch.CyclopsFramework
{
    public struct CyclopsTagStatus : IComparable<CyclopsTagStatus>
    {
        public string Tag;
        public int Count;

        int IComparable<CyclopsTagStatus>.CompareTo(CyclopsTagStatus other)
        {
            if ((Tag != null) && (other.Tag != null))
            {
                if (Tag.StartsWith("*"))
                    return -1;
                else if (other.Tag.StartsWith("*"))
                    return 1;
            }

            return string.CompareOrdinal(Tag, other.Tag);
        }
    }
}
