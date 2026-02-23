using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;

namespace Neo.Infrastructure.Matrix;

/// <summary>
/// Interface for converting <see cref="EquationSystem"/> to MathNet matrix types.
/// </summary>
public interface IMatrixConverter
{
    /// <summary>
    /// Converts an equation system to a MathNet coefficient matrix.
    /// </summary>
    /// <param name="system">The equation system.</param>
    /// <returns>The coefficient matrix A.</returns>
    Result<Matrix<double>> ToMathNetMatrix(EquationSystem system);

    /// <summary>
    /// Converts an equation system to a MathNet constants vector.
    /// </summary>
    /// <param name="system">The equation system.</param>
    /// <returns>The constants vector b.</returns>
    Result<Vector<double>> ToMathNetVector(EquationSystem system);

    /// <summary>
    /// Converts an equation system to raw arrays for matrix operations.
    /// </summary>
    /// <param name="system">The equation system.</param>
    /// <returns>A tuple of coefficient matrix (2D) and constants array.</returns>
    (double[,] Coefficients, double[] Constants) ToArrays(EquationSystem system);
}