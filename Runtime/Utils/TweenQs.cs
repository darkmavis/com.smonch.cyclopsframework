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
    public struct TweenQs
    {
        public Quaternion? from;
        public Quaternion? to;
        public Quaternion a;
        public Quaternion b;

        public Quaternion Fallback
        {
            set
            {
                a = from ?? value;
                b = to ?? value;
            }
        }

        public void SetFromTo(Quaternion? fromValue, Quaternion? toValue)
        {
            from = fromValue;
            to = toValue;
        }

        public void Reset()
        {
            from = null;
            to = null;
            a = Quaternion.identity;
            b = Quaternion.identity;
        }

        public Quaternion Evaluate(float t)
        {
            return Quaternion.Slerp(a, b, t);
        }
    }
}
