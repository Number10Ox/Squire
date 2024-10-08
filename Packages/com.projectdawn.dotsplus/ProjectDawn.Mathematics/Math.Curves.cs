using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProjectDawn.Mathematics
{
    /// <summary>
    /// A static class to contain various math functions.
    /// </summary>
    public static partial class math2
    {
        /// <summary>
        /// Returns solved standard/linear line equation ax + by = c for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float line(float a, float b, float c, float x) => (a * x - c) / -b;
        /// <summary>
        /// Returns solved standard/linear line equation ax + by = c for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double line(double a, double b, double c, double x) => (a * x - c) / -b;

        /// <summary>
        /// Returns solved hyperbola equation a/x = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float hyperbola(float a, float x) => a / x;
        /// <summary>
        /// Returns solved hyperbola equation a/x = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double hyperbola(double a, double x) => a / x;

        /// <summary>
        /// Returns solved hyperbola equation ax^2 + bx + c = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float parabola(float a, float b, float c, float x) => a * x * x + b * x + c;
        /// <summary>
        /// Returns solved hyperbola equation ax^2 + bx + c = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double parabola(double a, double b, double c, double x) => a * x * x + b * x + c;

        /// <summary>
        /// Returns solved hyperbola equation ax^2 = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float parabola(float a, float x) => a * x * x;
        /// <summary>
        /// Returns solved hyperbola equation ax^2 = y for y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double parabola(double a, double x) => a * x * x;

        /// <summary>
        /// Returns sampled linear Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 bezier(float2 a, float2 b, float t) => lerp(a, b, t);
        /// <summary>
        /// Returns sampled linear Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double2 bezier(double2 a, double2 b, double t) => lerp(a, b, t);

        /// <summary>
        /// Returns sampled quadratic Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 bezier(float2 a, float2 b, float2 c, float t)
        {
            float ot = 1 - t;
            return b + ot * ot * (a - b) + t * t * (c - b);
        }
        /// <summary>
        /// Returns sampled quadratic Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double2 bezier(double2 a, double2 b, double2 c, double t)
        {
            double ot = 1 - t;
            return b + ot * ot * (a - b) + t * t * (c - b);
        }

        /// <summary>
        /// Returns sampled cubic Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 bezier(float2 a, float2 b, float2 c, float2 d, float t)
        {
            float ot = 1 - t;
            float ot2 = ot * ot;
            float ot3 = ot2 * ot;

            float t2 = t * t;
            float t3 = t2 * t;
            return ot3 * a + 3 * (ot2 * t * b + ot * t2 * c) + t3 * d;
        }
        /// <summary>
        /// Returns sampled cubic Bézier curve at t.
        /// Based on https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double2 bezier(double2 a, double2 b, double2 c, double2 d, double t)
        {
            double ot = 1 - t;
            double ot2 = ot * ot;
            double ot3 = ot2 * ot;

            double t2 = t * t;
            double t3 = t2 * t;
            return ot3 * a + 3 * (ot2 * t * b + ot * t2 * c) + t3 * d;
        }
    }
}
