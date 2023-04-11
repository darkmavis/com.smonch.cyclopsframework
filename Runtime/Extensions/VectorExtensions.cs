using UnityEngine;
using System.Runtime.CompilerServices;

namespace Smonch.CyclopsFramework.Extensions
{
    public static class VectorExtensions
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XXX(this Vector3 v) { return new Vector3(v.x, v.x, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XXY(this Vector3 v) { return new Vector3(v.x, v.x, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XXZ(this Vector3 v) { return new Vector3(v.x, v.x, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XYX(this Vector3 v) { return new Vector3(v.x, v.y, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XYY(this Vector3 v) { return new Vector3(v.x, v.y, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XYZ(this Vector3 v) { return new Vector3(v.x, v.y, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XZX(this Vector3 v) { return new Vector3(v.x, v.z, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XZY(this Vector3 v) { return new Vector3(v.x, v.z, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 XZZ(this Vector3 v) { return new Vector3(v.x, v.z, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YXX(this Vector3 v) { return new Vector3(v.y, v.x, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YXY(this Vector3 v) { return new Vector3(v.y, v.x, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YXZ(this Vector3 v) { return new Vector3(v.y, v.x, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YYX(this Vector3 v) { return new Vector3(v.y, v.y, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YYY(this Vector3 v) { return new Vector3(v.y, v.y, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YYZ(this Vector3 v) { return new Vector3(v.y, v.y, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YZX(this Vector3 v) { return new Vector3(v.y, v.z, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YZY(this Vector3 v) { return new Vector3(v.y, v.z, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 YZZ(this Vector3 v) { return new Vector3(v.y, v.z, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZXX(this Vector3 v) { return new Vector3(v.z, v.x, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZXY(this Vector3 v) { return new Vector3(v.z, v.x, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZXZ(this Vector3 v) { return new Vector3(v.z, v.x, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZYX(this Vector3 v) { return new Vector3(v.z, v.y, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZYY(this Vector3 v) { return new Vector3(v.z, v.y, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZYZ(this Vector3 v) { return new Vector3(v.z, v.y, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZZX(this Vector3 v) { return new Vector3(v.z, v.z, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZZY(this Vector3 v) { return new Vector3(v.z, v.z, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ZZZ(this Vector3 v) { return new Vector3(v.z, v.z, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X0Y(this Vector3 v) { return new Vector3(v.x, 0f, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X0Z(this Vector3 v) { return new Vector3(v.x, 0f, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X_Y(this Vector3 v, float z) { return new Vector3(v.x, z, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X_Z(this Vector3 v, float y) { return new Vector3(v.x, y, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 XY(this Vector3 v) { return new Vector2(v.x, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 XZ(this Vector3 v) { return new Vector2(v.x, v.z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 YZ(this Vector3 v) { return new Vector2(v.y, v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 XX(this Vector2 v) { return new Vector2(v.x, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 XY(this Vector2 v) { return new Vector2(v.x, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 YX(this Vector2 v) { return new Vector2(v.y, v.x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 YY(this Vector2 v) { return new Vector2(v.y, v.y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X0Y(this Vector2 v) { return new Vector3(v.x, 0f, v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 Y0X(this Vector2 v) { return new Vector3(v.y, 0f, v.x); }
    }
}