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

using UnityEngine;
using UnityEngine.Serialization;

namespace Smonch.CyclopsFramework
{
    public struct Tween3F
    {
        public Vector3? From;
        public Vector3? To;
        public Vector3 A;
        public Vector3 B;

        public Vector3 Fallback
        {
            set
            {
                A = From ?? value;
                B = To ?? value;
            }
        }

        public void SetFromTo(Vector3? fromValue, Vector3? toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public void Reset()
        {
            From = null;
            To = null;
            A = Vector3.zero;
            B = Vector3.zero;
        }

        public Vector3 Evaluate(float t)
        {
            return Vector3.Lerp(A, B, t);
        }
    }
}
