using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgorithmPackages.ChangeDetection
{
    public static class Tools
    {
        public static double Hypotenuse(double a, double b)
        {
            double r = 0.0;
            double absA = System.Math.Abs(a);
            double absB = System.Math.Abs(b);

            if (absA > absB)
            {
                r = b / a;
                r = absA * System.Math.Sqrt(1 + r * r);
            }
            else if (b != 0)
            {
                r = a / b;
                r = absB * System.Math.Sqrt(1 + r * r);
            }

            return r;
        }
    }
}
