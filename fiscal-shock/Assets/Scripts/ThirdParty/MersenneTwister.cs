#pragma warning disable
using System;
// Implementation of the Mersenne Twister pseudorandom number generator (https://en.wikipedia.org/wiki/Mersenne_Twister)

// Taken from https://www.codeproject.com/Articles/164087/Random-Number-Generation and reformatted
// Licensed under the Code Project Open License (CPOL) 1.02
// https://www.codeproject.com/info/cpol10.aspx


/*
Use this like so for a deterministic sequence of random numbers:
    MersenneTwister mt = new MersenneTwister(5);  // Seed the RNG with '5' -- any MersenneTwisters seeded with 5 will generate the same sequence!
    int randomInt = mt.Next();                    // Get the next integer in the sequence
    float randomFloat = mt.NextFloat();           // Get next float in sequence (on the interval [0, 1])
    double randomDouble = mt.NextDouble();        // Get next double in sequence (on the interval [0, 1])
    int boundedRandomInt = mt.Next(1, 5);         // Get next int on the interval [1, 5]

Note: NextFloat is the same as NextDouble, but at a lower precision. You can call any sort of Next on the same RNG, but keep in mind that this advances the sequence.
    MersenneTwister mtA = new MersenneTwister(5);
    MersenneTwister mtB = new MersenneTwister(5);

    mtA.Next();                       // Throw away an int
    float floatA = mtA.NextFloat();   // Get a float from the first RNG

    float floatB = mtB.NextFloat();   // Not the same as floatA! This is the first number generated!
    float floatB2 = mtB.NextFloat();  // Equal to floatA. It's the second number in the sequence and it's also a float.

Using a seed ensures that you get the same sequence every time. This is important if you want to be able to get the same random result (testing, repeating a randomized level, etc.). Just don't share an RNG across different things.
*/

namespace ThirdParty {
    public class MersenneTwister {
        // Class MersenneTwister generates random numbers
        // from a uniform distribution using the Mersenne
        // Twister algorithm.
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0dfU;
        private const uint UPPER_MASK = 0x80000000U;
        private const uint LOWER_MASK = 0x7fffffffU;
        private const int MAX_RAND_INT = 0x7fffffff;
        private uint[] mag01 = {0x0U, MATRIX_A};
        private uint[] mt = new uint[N];
        private int mti = N + 1;

        public uint seed { get; }

        public MersenneTwister() {
            seed = (uint)DateTime.Now.Millisecond;
            init_genrand(seed);
        }

        public MersenneTwister(int inputSeed) {
            seed = (uint)inputSeed;
            init_genrand(seed);
        }

        public MersenneTwister(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i) {
                initArray[i] = (uint)init[i];
            }
            init_by_array(initArray, (uint)initArray.Length);
        }

        public static int MaxRandomInt { get { return 0x7fffffff; } }

        public int Next() {
            return genrand_int31();
        }

        public int Next(int maxValue) {
            return Next(0, maxValue);
        }

        public int Next(int minValue, int maxValue) {
            if (minValue > maxValue) {
                int tmp = maxValue;
                maxValue = minValue;
                minValue = tmp;
            }
            return (int)(Math.Floor((maxValue - minValue + 1) * genrand_real1() + minValue));
        }

        public float NextFloat() {
            return (float) genrand_real2();
        }

        public float NextFloat(bool includeOne) {
            if (includeOne) {
                return (float) genrand_real1();
            }
            return (float) genrand_real2();
        }

        public float NextFloatPositive() {
            return (float) genrand_real3();
        }

        public double NextDouble() {
            return genrand_real2();
        }

        public double NextDouble(bool includeOne)
        {
            if (includeOne) {
                return genrand_real1();
            }
            return genrand_real2();
        }

        public double NextDoublePositive() {
            return genrand_real3();
        }

        public double Next53BitRes() {
            return genrand_res53();
        }

        public void Initialize() {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        public void Initialize(int seed) {
            init_genrand((uint)seed);
        }

        public void Initialize(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i) {
                initArray[i] = (uint)init[i];
            }
            init_by_array(initArray, (uint)initArray.Length);
        }

        private void init_genrand(uint s) {
            mt[0] = s & 0xffffffffU;
            for (mti=1; mti < N; mti++) {
                mt[mti] = (uint)(1812433253U * (mt[mti-1] ^ (mt[mti-1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

        private void init_by_array(uint[] init_key, uint key_length) {
            int i, j, k;
            init_genrand(19650218U);
            i = 1;
            j = 0;
            k = (int)(N > key_length ? N : key_length);

            for (; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i-1] ^ (mt[i-1] >> 30)) * 1664525U)) + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++;
                j++;

                if (i >= N) {
                    mt[0] = mt[N-1];
                    i=1;
                }

                if (j >= key_length) {
                    j=0;
                }
            }

            for (k = N-1; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i-1] ^ (mt[i-1] >> 30)) * 1566083941U)) - i);
                mt[i] &= 0xffffffffU;
                i++;

                if (i >= N) {
                    mt[0] = mt[N-1];
                    i=1;
                }
            }
            mt[0] = 0x80000000U;
        }

        uint genrand_int32() {
            uint y;
            if (mti >= N) {
                int kk;

                if (mti == N+1) {
                    init_genrand(5489U);
                }

                for (kk = 0; kk < N-M; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk+1] & LOWER_MASK);
                    mt[kk] = mt[kk+M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                for (; kk < N-1; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk+1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M-N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                y = (mt[N-1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N-1] = mt[M-1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }

            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        private int genrand_int31() {
            return (int)(genrand_int32() >> 1);
        }

        double genrand_real1() {
            return genrand_int32() * (1.0 / 4294967295.0);
        }

        double genrand_real2() {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

        double genrand_real3() {
            return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
        }

        double genrand_res53() {
            uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }
    }
}