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

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Smonch.CyclopsFramework
{
    public static class Bias
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Linear(float t) => t;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Reverse(float t) => 1f - t;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseIn3(float t) => t * t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseIn5(float t) => t * t * t * t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOut3(float t) => (t - 1f) * (t - 1f) * (t - 1f) + 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOut5(float t) => (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) + 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOut3(float t) => ((t /= .5f) < 1f) ? ((t * t * t) * .5f) : (((t - 2f) * (t - 2f) * (t - 2f) + 2f) * .5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOut5(float t) => ((t /= .5f) < 1f)
            ? ((t * t * t * t * t) * .5f)    
            : (((t - 2f) * (t - 2f) * (t - 2f) * (t - 2f) * (t - 2f) + 2f) * .5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float One(float t) => 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Half(float t) => .5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Zero(float t) => 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SawWave(float t) => (t <= 0.5f) ? (t * 2f) : (2f - t * 2f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SquareWave(float t) => (t < 0.5f) ? 0f : 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Noise(float t) => Random.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos01(float t) => 0.5f + Mathf.Sin((t * 2f - 0.5f) * Mathf.PI) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin01(float t) => 0.5f + Mathf.Sin((t * 2f - 0.5f) * Mathf.PI) * 0.5f;
        
        /// <summary>
        /// Ensures: sign(f(t)) == sign(t)
        /// </summary>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CorrectSign(System.Func<float, float> f, float t)
        {
            if (t < 0f)
            {
                t = -t;
                return (t < 1f) ? -f(t) : f(1f);
            }
            else
            {
                return (t < 1f) ? f(t) : f(1f);
            }
        }
    }
}
