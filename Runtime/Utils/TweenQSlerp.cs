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

namespace Smonch.CyclopsFramework
{
    public struct TweenQSlerp
    {
        public Quaternion? From;
        public Quaternion? To;
        public Quaternion A;
        public Quaternion B;

        public Quaternion Fallback
        {
            set
            {
                A = From ?? value;
                B = To ?? value;
            }
        }

        public void SetFromTo(Quaternion? fromValue, Quaternion? toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public void Reset()
        {
            From = null;
            To = null;
            A = Quaternion.identity;
            B = Quaternion.identity;
        }

        public Quaternion Evaluate(float t)
        {
            return Quaternion.Slerp(A, B, t);
        }
    }
}
