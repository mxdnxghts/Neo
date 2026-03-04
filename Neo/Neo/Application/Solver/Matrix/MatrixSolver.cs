using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Solver.Equation;
using Neo.Domain.Result;
using System;
using System.Linq;

namespace Neo.Application.Solver.Matrix;

/// <summary>
/// Implementation of <see cref="IMatrixSolver"/> using MathNet.Numerics.
/// Supports LU, QR, Cholesky, and SVD decomposition algorithms.
/// </summary>
public sealed class MatrixSolver : IMatrixSolver
{
    /// <inheritdoc/>
    public Result<Vector<double>> Solve(Matrix<double> a, Vector<double> b, SolvingAlgorithm algorithm)
    {
        try
        {
            return algorithm switch
            {
                SolvingAlgorithm.LU => SolveLU(a, b),
                SolvingAlgorithm.QR => SolveQR(a, b),
                SolvingAlgorithm.Cholesky => SolveCholesky(a, b),
                SolvingAlgorithm.SVD => SolveSVD(a, b),
                _ => SolveLU(a, b)
            };
        }
        catch (Exception ex)
        {
            return Result<Vector<double>>.Failure(
                new Error($"Solving failed: {ex.Message}", "SOLVE_ERROR", ex));
        }
    }

    /// <inheritdoc/>
    public Result<Vector<double>> SolveLU(Matrix<double> a, Vector<double> b)
    {
        if (a.RowCount != a.ColumnCount)
            return Result<Vector<double>>.Failure(
                new Error("LU requires square matrix", "NON_SQUARE"));

        if (Math.Abs(a.Determinant()) < 1e-12)
            return Result<Vector<double>>.Failure(
                new Error("Matrix is singular", "SINGULAR_MATRIX"));

        var solution = a.LU().Solve(b);
        return Result<Vector<double>>.Success(solution);
    }

    /// <inheritdoc/>
    public Result<Vector<double>> SolveQR(Matrix<double> a, Vector<double> b)
    {
        var solution = a.QR().Solve(b);
        return Result<Vector<double>>.Success(solution);
    }

    /// <inheritdoc/>
    public Result<Vector<double>> SolveCholesky(Matrix<double> a, Vector<double> b)
    {
        if (!a.IsSymmetric())
            return Result<Vector<double>>.Failure(
                new Error("Matrix must be symmetric for Cholesky", "NOT_SYMMETRIC"));

        var chol = a.Cholesky();
        if (chol.Determinant <= 0)
            return Result<Vector<double>>.Failure(
                new Error("Matrix not positive definite", "NOT_POSITIVE_DEFINITE"));

        var solution = chol.Solve(b);
        return Result<Vector<double>>.Success(solution);
    }

    /// <inheritdoc/>
    public Result<Vector<double>> SolveSVD(Matrix<double> a, Vector<double> b)
    {
        var solution = a.Svd().Solve(b);
        return Result<Vector<double>>.Success(solution);
    }

    /// <inheritdoc/>
    public double ConditionNumber(Matrix<double> matrix)
    {
        var svd = matrix.Svd();
        var s = svd.S;
        if (s.Count == 0)
            return double.PositiveInfinity;
        return s.Max() / s.Min(v => v > 1e-14 ? v : 1e-14);
    }
}