﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TootTallyTTCounter
{
    public static class TTUtils
    {
        //https://github.com/emmett-shark/HighscoreAccuracy/blob/6a8d8f25f77d906f2d5dff3a306def21146ba343/Utils.cs#L32
        public static int GetRealMax(float length, int noteCount)
        {
            double champbonus = noteCount > 23 ? 1.5 : 0;
            double realCoefficient = (Math.Min(noteCount, 10) + champbonus) * 0.100000001490116 + 1.0;
            length = GetLength(length);
            return (int)(Mathf.Floor((float)((double)length * 100 * realCoefficient)) * 10f);
        }
        public static float GetLength(float length) => Mathf.Clamp(length, 0.2f, 5f) * 8f + 10f;

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
        //y = (0.7x^2 + 12x + 0.05)/1.5
        public static float CalculateBaseTT(float starRating) => (0.7f * FastPow(starRating, 2) + (12f * starRating) + 0.05f) / 1.5f;

        public static float CalculateScoreTT(float baseTT, float percent)
        {
            if (percent < 0.98f)
                return ((c * (float)Math.Pow(Math.E, b * percent)) - c) * baseTT;
            else
                return FastPow(9.2f * percent - 7.43037117f, 5) * baseTT;
        }

        public const float c = 0.028091281f;
        public const float b = 6f;
    }
}
