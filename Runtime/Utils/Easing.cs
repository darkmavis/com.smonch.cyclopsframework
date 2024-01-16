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

// Note: These functions are commonly used and can be found from various sources online.
// Credit to Robert Penner for popularizing some of these functions many years ago.
// I've chosen to include functions that remain normalized because that plays well with CyclopsRoutines.
// All functions are f(t) for ease of use, pun intended.

using System.Runtime.CompilerServices;
using static Unity.Mathematics.math;
// ReSharper disable MemberCanBePrivate.Global

namespace Smonch.CyclopsFramework
{
    public static class Easing
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Linear(float t) => t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Reverse(float t) => 1f - t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float BounceIn(float t) => 1f - BounceOut(1f - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float BounceOut(float t) => t < 1f/2.75f ? 7.5625f * t * t : t < 2f/2.75f ? 7.5625f * (t -= 1.5f/2.75f) * t + .75f : t < 2.5f/2.75f ? 7.5625f * (t -= 2.25f/2.75f) * t + .9375f : 7.5625f * (t -= 2.625f/2.75f) * t + .984375f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseIn3(float t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseIn5(float t) => t * t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseOut3(float t) => (t - 1f) * (t - 1f) * (t - 1f) + 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseOut5(float t) => (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) + 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseInOut3(float t) => ((t /= .5f) < 1f) ? ((t * t * t) * .5f) : (((t - 2f) * (t - 2f) * (t - 2f) + 2f) * .5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EaseInOut5(float t) => ((t /= .5f) < 1f) ? ((t * t * t * t * t) * .5f) : (((t - 2f) * (t - 2f) * (t - 2f) * (t - 2f) * (t - 2f) + 2f) * .5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float SawWave(float t) => (t <= 0.5f) ? (t * 2f) : (2f - t * 2f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float TriangleWave(float t, float period = 1f) => 2f * abs(2f * ((t / period) - floor((t / period) + 0.5f))) - 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float SquareWave(float t) => (t < 0.5f) ? 0f : 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Cos01(float t) => 0.5f + cos((t * 2f - 0.5f) * PI) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sin01(float t) => 0.5f + sin((t * 2f - 0.5f) * PI) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Zero(float t) => 0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Half(float t) => .5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float One(float t) => 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Noise(float t) => UnityEngine.Random.value;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Noise(double t) => UnityEngine.Random.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Linear(double t) => t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Reverse(double t) => 1.0 - t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double BounceIn(double t) => 1.0 - BounceOut(1.0 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double BounceOut(double t) => t < 1/2.75 ? 7.5625 * t * t : t < 2/2.75 ? 7.5625 * (t -= 1.5/2.75) * t + .75 : t < 2.5/2.75 ? 7.5625 * (t -= 2.25/2.75) * t + .9375 : 7.5625 * (t -= 2.625/2.75) * t + .984375;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseIn3(double t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseIn5(double t) => t * t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseOut3(double t) => (t - 1.0) * (t - 1.0) * (t - 1.0) + 1.0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseOut5(double t) => (t - 1.0) * (t - 1.0) * (t - 1.0) * (t - 1.0) * (t - 1.0) + 1.0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseInOut3(double t) => ((t /= 0.5) < 1.0) ? ((t * t * t) * 0.5) : (((t - 2.0) * (t - 2.0) * (t - 2.0) + 2.0) * 0.5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EaseInOut5(double t) => ((t /= 0.5) < 1.0) ? ((t * t * t * t * t) * 0.5) : (((t - 2.0) * (t - 2.0) * (t - 2.0) * (t - 2.0) * (t - 2.0) + 2.0) * 0.5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double SawWave(double t) => (t <= 0.5) ? (t * 2.0) : (2.0 - t * 2.0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double TriangleWave(double t, double period = 1.0) => 2.0 * abs(2.0 * ((t / period) - floor((t / period) + 0.5))) - 1.0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double SquareWave(double t) => (t < 0.5) ? 0.0 : 1.0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Cos01(double t) => 0.5 + cos((t * 2.0 - 0.5) * PI) * 0.5;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Sin01(double t) => 0.5 + sin((t * 2.0 - 0.5) * PI) * 0.5;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Zero(double t) => 0.0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Half(double t) => 0.5;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double One(double t) => 1.0;
    }
}
