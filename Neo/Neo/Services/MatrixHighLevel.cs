﻿using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronOcr;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Neo.Services
{
    public class MatrixHighLevel
    {
        /// <summary>
        /// returns determinant of matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double GetDeterminant(Matrix<double> matrix) 
            => matrix.Determinant();

        /// <summary>
        /// returns rank of matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double GetRank(Matrix<double> matrix)
            => matrix.Rank();

        /// <summary>
        /// reversing matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Matrix<double> GetReverseMatrix(Matrix<double> matrix)
            => matrix.Inverse();
        
        /// <summary>
        /// Raises the matrix to the specified power
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="n">Exponentiation value</param>
        /// <returns></returns>
        public static Matrix<double> Exponentiation(Matrix<double> matrix, int n)
        {
            var tempMatrix = matrix;
            for (int i = 1; i < n; i++)
                matrix = matrix.Multiply(tempMatrix);
            return matrix;
        }

        /// <summary>
        /// transposing matrix
        /// column go to row and row go to column
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Matrix<double> Transpose(Matrix<double> matrix)
            => matrix.Transpose();
    }
}