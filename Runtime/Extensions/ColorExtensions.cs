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

#if UNITY_2017_4_OR_NEWER
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Smonch.CyclopsFramework.Extensions
{
    public static class ColorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color LerpPerComponent(this Color c, Color toColor, Vector4 t)
        {
            int i = 0;

            c[i] = Mathf.Lerp(c[i], toColor[i], t[i++]);
            c[i] = Mathf.Lerp(c[i], toColor[i], t[i++]);
            c[i] = Mathf.Lerp(c[i], toColor[i], t[i++]);
            c[i] = Mathf.Lerp(c[i], toColor[i], t[i]);

            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color LerpUnclampedPerComponent(this Color c, Color toColor, Vector4 t)
        {
            int i = 0;

            c[i] = Mathf.LerpUnclamped(c[i], toColor[i], t[i++]);
            c[i] = Mathf.LerpUnclamped(c[i], toColor[i], t[i++]);
            c[i] = Mathf.LerpUnclamped(c[i], toColor[i], t[i++]);
            c[i] = Mathf.LerpUnclamped(c[i], toColor[i], t[i]);

            return c;
        }
    }
}
#endif
