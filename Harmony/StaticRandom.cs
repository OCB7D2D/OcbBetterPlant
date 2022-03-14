using System;

namespace OCB
{
    public static class StaticRandom
    {

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Explicit)]
        public class ReinterpretCaster
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public ulong t_ulong;
            [System.Runtime.InteropServices.FieldOffset(0)] public double t_double;
            [System.Runtime.InteropServices.FieldOffset(0)] public int t_int1;
            [System.Runtime.InteropServices.FieldOffset(5)] public int t_int2;
        }

        public static readonly Random rng = new Random();

        private static readonly ReinterpretCaster caster = new ReinterpretCaster();

        public static ulong DOUBLE2ULONG(double value)
        {
            caster.t_double = value;
            return caster.t_ulong;
        }

        public static double ULONG2DOUBLE(ulong value)
        {
            caster.t_ulong = value;
            return caster.t_double;
        }

        public static ulong RandomSeed()
        {
            caster.t_ulong = 0;
            caster.t_int1 = rng.Next();
            caster.t_int2 = rng.Next();
            return caster.t_ulong;
        }

        public static void HashSeed(ref ulong seed, ulong value)
        {
            seed = (((3074457345618258791ul + seed) * 2774457345618258799ul) + value) * 3374457345618258799ul;
        }

        public static void HashSeed(ref ulong seed, float value)
        {
            HashSeed(ref seed, DOUBLE2ULONG(value));
        }

        public static void HashSeed(ref ulong seed, double value)
        {
            HashSeed(ref seed, DOUBLE2ULONG(value));
        }

        public static float Range(float min, float max, ulong seed)
        {
            float range = Math.Abs(max - min);
            return Math.Min(min, max) +
                range * seed / (ulong.MaxValue);
        }

        public static float RangeSquare(float min, float max, ulong seed)
        {
            float range = Math.Abs(max - min);
            double rnd = (double)seed / (ulong.MaxValue) - 0.5;
            rnd = rnd * rnd * 2 * Math.Sign(rnd) + 0.5;
            return Math.Min(min, max) + range * (float)rnd;
        }

    }
}
