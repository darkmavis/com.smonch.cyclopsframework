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

using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public struct Tween1f
    {
        public float? from;
        public float? to;
        public float a;
        public float b;

        public float Fallback
        {
            set
            {
                a = from ?? value;
                b = to ?? value;
            }
        }

        public void SetFromTo(float? fromValue, float? toValue)
        {
            from = fromValue;
            to = toValue;
        }

        public void Reset()
        {
            from = null;
            to = null;
            a = 0f;
            b = 0f;
        }

        public float Evaluate(float t)
        {
            return Mathf.Lerp(a, b, t);
        }
    }
}