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
    public struct Tween4C
    {
        public Color? From;
        public Color? To;
        public Color A;
        public Color B;

        public Color Fallback
        {
            set
            {
                A = From ?? value;
                B = To ?? value;
            }
        }

        public void SetFromTo(Color? fromValue, Color? toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public void Reset()
        {
            From = null;
            To = null;
            A = new Color();
            B = new Color();
        }

        public Color Evaluate(float t)
        {
            return Color.Lerp(A, B, t);
        }
    }
}
