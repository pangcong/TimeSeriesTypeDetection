using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesAnalysis
{
    /// <summary>
    /// Fast Fourier Transform
    /// </summary>
    class FastFourierTransform
    {
        private FastFourierTransform()
        {
        }

        /// <summary>
        /// Bit-reversal Permutation
        /// </summary>
        /// <param name="xreal"></param>
        /// <param name="ximag"></param>
        /// <param name="n"></param>
        private static void bitrp(double [] xreal, double [] ximag, int n)
        {
            // Bit-reversal Permutation
            int i, j, a, b, p;

            for (i = 1, p = 0; i < n; i *= 2)
            {
                p++;
            }

            for (i = 0; i < n; i++)
            {
                a = i;
                b = 0;
                for (j = 0; j < p; j++)
                {
                    b = b * 2 + a % 2;
                    a = a / 2;
                }
                if (b > i)
                {
                    double t = xreal[i];
                    xreal[i] = xreal[b];
                    xreal[b] = t;
                    t = ximag[i];
                    ximag[i] = ximag[b];
                    ximag[b] = t;
                }
            }
        }

        /// <summary>
        /// Fast Fourier Transfom Function
        /// </summary>
        /// <param name="xreal"></param>
        /// <param name="ximag"></param>
        /// <returns></returns>
        public static int FFT(ref double [] xreal, ref double [] ximag)
        {
            //2^n < length
            int n = 2;
            while (n <= xreal.Length)
            {
                n *= 2;
            }
            n /= 2;

            // save it as real part and imagine part
            double [] wreal = new double [n / 2];
            double [] wimag = new double [n / 2];

            double treal, timag, ureal, uimag, arg;

            int m, k, j, t, index1, index2;
            bitrp(xreal, ximag, n);

            // complex W'j = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
            arg = (float)(-2 * Math.PI / n);
            treal = (float)Math.Cos(arg);
            timag = (float)Math.Sin(arg);
            wreal[0] = 1.0f;
            wimag[0] = 0.0f;
            for (j = 1; j < n / 2; j++)
            {
                wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
                wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
            }
            for (m = 2; m <= n; m *= 2)
            {
                for (k = 0; k < n; k += m)
                {
                    for (j = 0; j < m / 2; j++)
                    {
                        index1 = k + j;
                        index2 = index1 + m / 2;
                        t = n * j / m;    
                        treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                        timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                        ureal = xreal[index1];
                        uimag = ximag[index1];
                        xreal[index1] = ureal + treal;
                        ximag[index1] = uimag + timag;
                        xreal[index2] = ureal - treal;
                        ximag[index2] = uimag - timag;
                    }
                }
            }
            return n;
        }

        /// <summary>
        /// Inverse Fourier Transform
        /// </summary>
        /// <param name="xreal"></param>
        /// <param name="ximag"></param>
        /// <returns></returns>
        public static int IFFT(ref double [] xreal, ref double [] ximag)
        {
            //2^n < length
            int n = 2;
            while (n <= xreal.Length)
            {
                n *= 2;
            }
            n /= 2;

            // Fast Inverse Fourer Transferm
            double [] wreal = new double [n / 2];
            double [] wimag = new double [n / 2];
            double treal, timag, ureal, uimag, arg;
            int m, k, j, t, index1, index2;
            bitrp(xreal, ximag, n);

            // Wj = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
            arg = (float)(2 * Math.PI / n);
            treal = (float)(Math.Cos(arg));
            timag = (float)(Math.Sin(arg));
            wreal[0] = 1.0f;
            wimag[0] = 0.0f;
            for (j = 1; j < n / 2; j++)
            {
                wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
                wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
            }
            for (m = 2; m <= n; m *= 2)
            {
                for (k = 0; k < n; k += m)
                {
                    for (j = 0; j < m / 2; j++)
                    {
                        index1 = k + j;
                        index2 = index1 + m / 2;
                        t = n * j / m;
                        treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                        timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                        ureal = xreal[index1];
                        uimag = ximag[index1];
                        xreal[index1] = ureal + treal;
                        ximag[index1] = uimag + timag;
                        xreal[index2] = ureal - treal;
                        ximag[index2] = uimag - timag;
                    }
                }
            }
            for (j = 0; j < n; j++)
            {
                xreal[j] /= n;
                ximag[j] /= n;
            }
            return n;
        }

        public static int FFT(List<double> x, out List<double> fft)
        {
            int i;
            
            double [] xreal = new double[x.Count];
            double [] ximag = new double[x.Count];

            // Initial the list
            for (i = 0; i < x.Count; i++)
            {
                xreal[i] = x[i];
                ximag[i] = 0.0;
            }

            int n = FFT(ref xreal, ref ximag);
            fft = new List<double>();

            for (i = 0; i < n/2; i++)
            {
                fft.Add(Math.Sqrt(xreal[i] * xreal[i] + ximag[i] * ximag[i]));
            }

            return n;
        }
    }
}
