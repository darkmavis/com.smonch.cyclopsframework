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
    public struct Tween4F
    {
        public Vector4? From;
        public Vector4? To;
        public Vector4 A;
        public Vector4 B;

        public Vector4 Fallback
        {
            set
            {
                A = From ?? value;
                B = To ?? value;
            }
        }

        public void SetFromTo(Vector4? fromValue, Vector4? toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public void Reset()
        {
            From = null;
            To = null;
            A = Vector4.zero;
            B = Vector4.zero;
        }

        public Vector4 Evaluate(float t)
        {
            return Vector4.Lerp(A, B, t);
        }
    }
}
