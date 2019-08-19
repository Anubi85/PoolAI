using System;

namespace PoolAI.SDK
{
    static class DoubleExtension
    {
        #region Constants

        private const double c_DefaultTolerance = 5e-10;
        private const double c_DefaultQuantum = 5e-6;

        #endregion

        #region Methods

        public static bool ToleranceEqual(this double value, double other, double tolerance = c_DefaultTolerance)
        {
            return Math.Abs(value - other) < tolerance;
        }

        public static bool Between(this double value, double min, double max)
        {
            return min <= value && value <= max;
        }

        public static bool ToleranceLess(this double value, double other, double tolerance = c_DefaultTolerance)
        {
            return value < (other - tolerance);
        }

        public static bool ToleranceLessEqual(this double value, double other, double tolerance = c_DefaultTolerance)
        {
            return value < (other + tolerance);
        }

        public static bool ToleranceGreater(this double value, double other, double tolerance = c_DefaultTolerance)
        {
            return value > (other + tolerance);
        }

        public static bool ToleranceGreaterEqual(this double value, double other, double tolerance = c_DefaultTolerance)
        {
            return value > (other - tolerance);
        }

        public static double Quantize(this double value, double quantum = c_DefaultQuantum)
        {
            return Math.Round(value / quantum) * quantum;
        }

        #endregion
    }
}
