using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Solver.Equation;
using Neo.Domain.Result;

namespace Neo.Application.Solver.Matrix;

/// <summary>
/// Interface for solving linear systems using various decomposition algorithms.
/// </summary>
public interface IMatrixSolver
{
    /// <summary>
    /// Solves the system Ax = b using the specified algorithm.
    /// </summary>
    /// <param name="a">The coefficient matrix.</param>
    /// <param name="b">The constants vector.</param>
    /// <param name="algorithm">The solving algorithm to use.</param>
    /// <returns>The solution vector x, or an error if solving fails.</returns>
    Result<Vector<double>> Solve(Matrix<double> a, Vector<double> b, SolvingAlgorithm algorithm);

    /// <summary>
    /// Solves using LU decomposition with partial pivoting.
    /// Requires a square matrix.
    /// </summary>
    /// <param name="a">The coefficient matrix (must be square).</param>
    /// <param name="b">The constants vector.</param>
    /// <returns>The solution vector x.</returns>
    Result<Vector<double>> SolveLU(Matrix<double> a, Vector<double> b);

    /// <summary>
    /// Solves using QR decomposition.
    /// Works for non-square matrices (least squares solution).
    /// </summary>
    /// <param name="a">The coefficient matrix.</param>
    /// <param name="b">The constants vector.</param>
    /// <returns>The solution vector x.</returns>
    Result<Vector<double>> SolveQR(Matrix<double> a, Vector<double> b);

    /// <summary>
    /// Solves using Cholesky decomposition.
    /// Requires a symmetric positive-definite matrix.
    /// </summary>
    /// <param name="a">The coefficient matrix (must be symmetric positive-definite).</param>
    /// <param name="b">The constants vector.</param>
    /// <returns>The solution vector x.</returns>
    Result<Vector<double>> SolveCholesky(Matrix<double> a, Vector<double> b);

    /// <summary>
    /// Solves using Singular Value Decomposition (SVD).
    /// Most stable method, handles ill-conditioned matrices.
    /// </summary>
    /// <param name="a">The coefficient matrix.</param>
    /// <param name="b">The constants vector.</param>
    /// <returns>The solution vector x.</returns>
    Result<Vector<double>> SolveSVD(Matrix<double> a, Vector<double> b);

    /// <summary>
    /// Computes the condition number of a matrix using SVD.
    /// High condition numbers indicate ill-conditioned matrices.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The condition number.</returns>
    double ConditionNumber(Matrix<double> matrix);
}