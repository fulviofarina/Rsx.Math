using System;

namespace Rsx.Math
{
    public partial class MyMath
    {
        /// <summary>
        /// Saturation factor (lamda,t)
        /// </summary>
        public static Func<double, double, double> S = (l, t) => 1.0 - System.Math.Exp(-1.0 * l * t);

        /// <summary>
        /// Decay factor (lamda,t)
        /// </summary>
        public static Func<double, double, double> D = (l, t) => System.Math.Exp(-1.0 * l * t);

        /// <summary>
        /// Counting factor (lamda,t)
        /// </summary>
        public static Func<double, double, double> C = (l, t) => (1.0 - System.Math.Exp(-1.0 * l * t)) / (t * l);

        /// <summary>
        ///  (alpha)
        /// </summary>
        public static Func<double, double> Calpha = (alpha) =>
        {
            double intalpha = (2.0 * alpha) + 1.0;
            double ecdalpha = System.Math.Pow(0.55, alpha);

            return (0.429) / (ecdalpha * intalpha);
        };

        /// <summary>
        /// (alpha,Qo,Er)
        /// </summary>
        public static Func<double, double, double, double> qoalpha = (alpha, Qo, Er) =>
        {
            return (Qo - 0.429) * System.Math.Pow(Er, -1.0 * alpha);
        };

        public static double NAvg = 6.02214179E+23;
    }
}