using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Problem
{
    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO
    // *****************************************
    public static class IntegerMultiplication
    {
        #region YOUR CODE IS HERE
        //Your Code is Here:
        //==================
        /// <summary>
        /// Multiply 2 large integers of N digits in an efficient way [Karatsuba's Method]
        /// </summary>
        /// <param name="X">First large integer of N digits [0: least significant digit, N-1: most signif. dig.]</param>
        /// <param name="Y">Second large integer of N digits [0: least significant digit, N-1: most signif. dig.]</param>
        /// <param name="N">Number of digits (power of 2)</param>
        /// <returns>Resulting large integer of 2xN digits (left padded with 0's if necessarily) [0: least signif., 2xN-1: most signif.]</returns>
        /// 
        public static byte[] AddOp(byte[] firsthalf, byte[] secondhalf)
        {
            int len1 = firsthalf.Length, len2 = secondhalf.Length;
            if (firsthalf.Length > secondhalf.Length)
            {
                Array.Resize(ref secondhalf, len1);
            }
            else if (firsthalf.Length < secondhalf.Length)
            {
                Array.Resize(ref firsthalf, len2);
            }
            byte[] result = new byte[firsthalf.Length + 1];
            int carry = 0;
            for (int i = 0; i < firsthalf.Length; i++)
            {
                int sum = firsthalf[i] + secondhalf[i] + carry;
                result[i] = (byte)(sum % 10);
                carry = sum / 10;
            }
            if (carry != 0)
            {
                result[result.Length - 1] = (byte)carry;
            }
            if (carry == 0)
            {
                Array.Resize(ref result, result.Length - 1);
            }
            return result;
        }
        static public byte[] subtract(byte[] firsthalf, byte[] secondhalf)
        {
            int len1 = firsthalf.Length;
            int len2 = secondhalf.Length;
            byte[] result = new byte[Math.Max(len1, len2)];
            for (int i = 0; i < Math.Min(len1, len2); i++)
            {
                if (firsthalf[i] >= secondhalf[i])
                {
                    int diff = firsthalf[i] - secondhalf[i];
                    result[i] = (byte)(diff);
                }
                else if (firsthalf[i] < secondhalf[i])
                {
                    int idx = i + 1;
                    while (idx < Math.Max(len1, len2))
                    {
                        if (firsthalf[idx] != 0)
                        {
                            firsthalf[idx]--;
                            int diff = (10 + firsthalf[i]) - secondhalf[i];
                            result[i] = (byte)(diff);
                            break;
                        }
                        else if (firsthalf[idx] == 0)
                        {
                            firsthalf[idx] = 9;
                        }
                        ++idx;
                    }
                }
            }
            for (int i = Math.Min(len1, len2); i < Math.Max(len1, len2); i++)
            {
                int diff = firsthalf[i];
                if (diff < 0)
                {
                    diff += 10;
                }
                result[i] = (byte)diff;
            }
            return result;
        }
        public static byte[] Multiply(byte[] X, byte[] Y)
        {
            // Initialize the result to 0
            //Array.Reverse(X);
            //Array.Reverse(Y);
            int n = Math.Max(X.Length, Y.Length);
            byte[] result = new byte[2 * n];
            int carry = 0;
            int i = 0, j = 0;
            for (i = 0; i < X.Length; i++)
            {
                for (j = 0; j < Y.Length; j++)
                {
                    int prod = X[i] * Y[j] + result[i + j] + carry;
                    carry = (byte)(prod / 10);
                    result[i + j] = (byte)(prod % 10);
                }
                if (carry != 0)
                {
                    result[i + j] += (byte)carry;
                    carry = 0;
                }
            }
            //Array.Reverse(result);
            return result;
        }
        public static byte[] shiftright(byte[] X, int N)
        {
            byte[] result = new byte[X.Length + N];
            for (int i = 0; i < N; i++)
            {
                result[i] = 0;
            }
            for (int i = N; i < X.Length + N; i++)
            {
                result[i] = X[i - N];
            }
            return result;
        }
        static public byte[] karatsuba(byte[] X, byte[] Y, int N)
        {
            if (N <= 128)
            {
                byte[] ree = Multiply(X, Y);
                return ree;
            }
            int half1 = N / 2;
            int half2 = N - half1;
            byte[] A = X.Take(half1).ToArray();
            byte[] B = X.Skip(half1).Take(half2).ToArray();
            byte[] C = Y.Take(half1).ToArray();
            byte[] D = Y.Skip(half1).Take(half2).ToArray();

            byte[] aplusb = AddOp(A, B);
            byte[] cplusd = AddOp(C, D);

            if (aplusb.Length != cplusd.Length)
            {
                if (aplusb.Length > cplusd.Length)
                    Array.Resize(ref cplusd, aplusb.Length);
                if (aplusb.Length < cplusd.Length)
                    Array.Resize(ref aplusb, cplusd.Length);
            }
            byte[] Z = null;
            byte[] amultc = null;
            byte[] bmultd = null;

            Task t1 = Task.Run(() =>
            {
                Z = karatsuba(aplusb, cplusd, aplusb.Length);
            });
            Task t2 = Task.Run(() =>
            {
                amultc = karatsuba(A, C, A.Length);
            });
            Task t3 = Task.Run(() =>
            {
                bmultd = karatsuba(B, D, B.Length);
            });
            Task.WaitAll(t1, t2, t3);

            byte[] arr2 = (subtract(Z, AddOp(amultc, bmultd)));
            byte[] arr1 = (bmultd);
            byte[] arr3 = amultc;

            byte[] res2 = new byte[(half1 * 2) + arr1.Length];
            int index = half1 * 2;
            foreach (byte it in arr1)
            {
                res2[index] = it;
                index++;
            }

            byte[] res3 = new byte[half1 + arr2.Length];
            index = half1;
            foreach (byte it in arr2)
            {
                res3[index] = it;
                index++;
            }
            byte[] result = new byte[N * 2];
            result = AddOp(AddOp(res2, res3), arr3);
            return result;
        }
        static public byte[] IntegerMultiply(byte[] X, byte[] Y, int N)
        {
            //REMOVE THIS LINE BEFORE START CODING
            //throw new NotImplementedException();
            // Ensure that N is a power of 2
            byte[] res = karatsuba(X, Y, N);
            return res;
        }
    }
    #endregion
}
