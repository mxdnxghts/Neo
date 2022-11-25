﻿using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Neo.Services
{
    internal class Parser
    {
        public const char SplitSymbol = ';';

        /// <summary>
        /// every what iteration 'll doing something
        /// </summary>
        public static int Every { get; private set; } = 1;

        /// <summary>
        /// output of Tesseract OCR
        /// </summary>
        public static string Input { get; internal set; } = string.Empty;

        /// <summary>
        /// Parse data from <see cref="Input"/> to <see cref="Matrix{T}"/>
        /// </summary>
        /// <returns></returns>
        public static Matrix<double> ParseToMatrix()
        {
            var targetArray = new double[
                // read count of ";" and therefore count will one less than actually
                Input.Count(x => x == SplitSymbol) + 1,
                // read count of spaces and divide it on count of symbol ";" and subtract 1
                Input.Count(x => x == ' ') / Input.Count(x => x == SplitSymbol) - 1];

            Every = targetArray.GetLength(1);

            // removing white space and commas
            var filterResult = Input.Split(' ', SplitSymbol).Where(x => x != " " || x != string.Empty).ToList()
                .RemoveEvery(Every, targetArray.GetLength(0));

            return Matrix<double>.Build.DenseOfArray(AddToMatrix(targetArray, filterResult));
        }

        /// <summary>
        /// Parse data from <see cref="Input"/> to <see cref="Vector{T}"/>
        /// </summary>
        /// <returns></returns>
        public static Vector<double> ParseToVector()
        {
            // remove white space and commas
            var filterResult = Input.Split(' ', SplitSymbol).Where(x => x != " " || x != string.Empty).ToList();

            // remove empty space
            filterResult = filterResult.Where(s => !string.IsNullOrWhiteSpace(s)).AsEnumerable().ToList()
                .AddEvery(Every, Input.Count(x => x == SplitSymbol) + 1);

            var targetArray = new double[filterResult.Count];

            return Vector<double>.Build.DenseOfArray(AddToVector(targetArray, filterResult));
        }

        /// <summary>
        /// Adding data to <see cref="targetArray"/> from <see cref="filterResult"/>
        /// </summary>
        /// <param name="targetArray">array which will contain parsed data from <see cref="filterResult"/></param>
        /// <param name="filterResult">parsed data from Tesseract OCR</param>
        private static double[,] AddToMatrix(double[,] targetArray, List<string> filterResult)
        {
            // start point for input text
            // like an iterator
            var point = 0;

            for (int i = 0; i < targetArray.GetLength(0);)
            {
                // on every iteration we increasing point instead of j
                // because we have to iterate filterResult but not columns of sourceArray
                for (int j = 0; j < targetArray.GetLength(1); point++)
                {
                    if (!Validate(ref point, filterResult))
                        break;
                    targetArray[i, j] = double.Parse(filterResult[point]);
                    j++;
                }
                i++;
            }
            return targetArray;
        }

        /// <summary>
        /// Adding data to <see cref="targetArray"/> from <see cref="filterResult"/>
        /// </summary>
        /// <param name="targetArray">array which will contain parsed data from <see cref="filterResult"/></param>
        /// <param name="filterResult">parsed data from Tesseract OCR</param>
        private static double[] AddToVector(double[] targetArray, List<string> filterResult)
        {
            // start point for input text
            // like an iterator
            var point = 0;
            var x = 0;
            for (int i = 0; i < filterResult.Count; i++)
            {
                // on every iteration we increasing point instead of j
                // because we have to iterate filterResult but not columns of sourceArray
                if (!Validate(ref point, filterResult))
                    break;

                targetArray[x] = double.Parse(filterResult[i]);
            }
            return targetArray;
        }

        /// <summary>
        /// Validate index of filterResult is corresponding to requirements or not
        /// </summary>
        /// <param name="point">index of <see cref="filterResult"/></param>
        /// <param name="filterResult">parsed data from Tesseract OCR</param>
        /// <returns></returns>
        private static bool Validate(ref int point, List<string> filterResult)
        {
            if (point == filterResult.Count)
                return false;
            if (filterResult[point] == string.Empty)
            {
                point++;
                return false;
            }

            return true;
        }
    }

    public static class ListExtension
    {
        /// <summary>
        /// remove elements every time when i in cycle will divide witout a trace by every' value
        /// after removes white spaces and empty strings from list
        /// </summary>
        /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
        /// <param name="every">position to delete</param>
        /// <param name="rows">count of rows from string of parsed matrix</param>
        /// <returns></returns>
        public static List<string> RemoveEvery(this List<string> input, int every, int rows)
        {
            for (int i = 1; i <= rows; i++)
            {
                input.RemoveAt((--every * i) - 1);
                every++;
            }
            return input.Where(s => !string.IsNullOrWhiteSpace(s)).AsEnumerable().ToList();
        }

        /// <summary>
        /// return new <see cref="List{T}"/> with elements which were at point equals every' value
        /// </summary>
        /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
        /// <param name="every">position to get</param>
        /// <param name="rows">count of rows from string of parsed matrix</param>
        /// <returns></returns>
        public static List<string> AddEvery(this List<string> input, int every, int rows)
        {
            var output = new List<string>();
            for (int i = 1; i <= rows; i++)
            {
                if (i == 1)
                {
                    output.Add(input[--every * i]);
                    every++;
                    continue;
                }
                output.Add(input[(every * i) - 1]);
            }
            return output.Where(s => !string.IsNullOrWhiteSpace(s)).AsEnumerable().ToList();
        }
    }
}