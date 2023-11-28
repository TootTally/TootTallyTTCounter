using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TootTallyTTCounter
{
    public static class TTUtils
    {
        public static float FastPow(double num, int exp)
        {
            double result = 1.0;
            while (exp > 0)
            {
                if (exp % 2 == 1)
                    result *= num;
                exp >>= 1;
                num *= num;
            }
            return (float)result;
        }

        //TT for S rank (60% score)
        //https://www.desmos.com/calculator/rhwqyp21nr
        public static float CalculateBaseTT(float starRating)
        {

            return (0.7f * FastPow(starRating, 2) + (12f * starRating) + 0.05f) / 1.5f;
            //y = (0.7x^2 + 12x + 0.05)/1.5
        }

        public static float CalculateScoreTT(float baseTT, float percent)
        {
            float scoreTT;
            if (percent < 0.6f)
                scoreTT = 21.433f * FastPow(percent, 6) * baseTT;
            else if (percent < 0.98f)
                scoreTT = ((0.028091281f * (float)Math.Pow(Math.E, 6f * percent)) - 0.028091281f) * baseTT;
            else
                scoreTT = FastPow(9.2f * percent - 7.43037117f, 5) * baseTT;
            //y = (0.28091281 * e^6x - 0.028091281) * b

            return scoreTT;
        }
    }
}
