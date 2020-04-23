using UnityEngine;

namespace FiscalShock.Procedural {
    /// <summary>
    /// Gaussian (normal) pseudorandom number generator. Useful when you want
    /// outliers to be rare.
    ///
    /// <para>Taken from https://stackoverflow.com/a/218600</para>
    /// </summary>
    public static class Gaussian {
        public static float next(float mean, float stdDev) {
            float u1 = 1.0f - Random.value;
            float u2 = 1.0f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                        Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
            float randNormal =
                        mean + (stdDev * randStdNormal); //random normal(mean,stdDev^2)

            return randNormal;
        }
    }
}
