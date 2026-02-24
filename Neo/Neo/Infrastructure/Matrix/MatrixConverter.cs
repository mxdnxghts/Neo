using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Matrix;

/// <summary>
/// High-performance implementation of <see cref="IMatrixConverter"/>.
/// Uses <see cref="ArrayPool{T}"/> to minimize allocations during matrix conversion.
/// </summary>
public sealed class MatrixConverter : IMatrixConverter, IDisposable
{
    private readonly ArrayPool<double> _arrayPool;
    private double[]? _rentedCoefficients;
    private double[]? _rentedConstants;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MatrixConverter"/> class.
    /// </summary>
    /// <param name="arrayPool">Optional custom array pool. Uses shared pool if not specified.</param>
    public MatrixConverter(ArrayPool<double>? arrayPool = null)
    {
        _arrayPool = arrayPool ?? ArrayPool<double>.Shared;
    }

    /// <inheritdoc/>
    public Result<Matrix<double>> ToMathNetMatrix(EquationSystem system)
    {
        try
        {
            var (coefficients, constants) = ToArrays(system);
            return Result<Matrix<double>>.Success(
                Matrix<double>.Build.DenseOfArray(coefficients));
        }
        catch (Exception ex)
        {
            return Result<Matrix<double>>.Failure(
                new Error($"Failed to convert to matrix: {ex.Message}", "MATRIX_CONVERSION_ERROR", ex));
        }
    }

    /// <inheritdoc/>
    public Result<Vector<double>> ToMathNetVector(EquationSystem system)
    {
        try
        {
            var (_, constants) = ToArrays(system);
            return Result<Vector<double>>.Success(
                Vector<double>.Build.DenseOfArray(constants));
        }
        catch (Exception ex)
        {
            return Result<Vector<double>>.Failure(
                new Error($"Failed to convert to vector: {ex.Message}", "VECTOR_CONVERSION_ERROR", ex));
        }
    }

    /// <inheritdoc/>
    public (double[,] Coefficients, double[] Constants) ToArrays(EquationSystem system)
    {
        ThrowIfDisposed();

        var normalized = system.Normalize();
        var equationCount = normalized.EquationCount;
        var variableCount = normalized.VariableCount;

        _rentedCoefficients = _arrayPool.Rent(equationCount * variableCount);
        _rentedConstants = _arrayPool.Rent(equationCount);

        try
        {
            FillArrays(normalized, _rentedCoefficients, _rentedConstants);

            var coefficientsArray = new double[equationCount, variableCount];
            Buffer.BlockCopy(_rentedCoefficients, 0, coefficientsArray, 0,
                equationCount * variableCount * sizeof(double));

            var constantsArray = new double[equationCount];
            Array.Copy(_rentedConstants, constantsArray, equationCount);

            return (coefficientsArray, constantsArray);
        }
        finally
        {
            ReturnRentedArrays();
        }
    }

    /// <summary>
    /// Fills the coefficient and constant arrays from the equation system.
    /// Uses parallel processing for large systems.
    /// </summary>
    /// <param name="system">The normalized equation system.</param>
    /// <param name="coefficientArray">The coefficient array to fill.</param>
    /// <param name="constantArray">The constant array to fill.</param>
    private void FillArrays(EquationSystem system, double[] coefficientArray, double[] constantArray)
    {
        if (system.EquationCount > 10)
        {
            Parallel.For(0, system.EquationCount, i =>
            {
                FillArray(system, i, coefficientArray, constantArray);
            });
        }
        else
        {
            for (int i = 0; i < system.EquationCount; i++)
            {
                FillArray(system, i, coefficientArray, constantArray);
            }
        }
    }

    private void FillArray(EquationSystem system, int equationIndex, double[] coefficientArray, double[] constantArray)
    {
        var equation = system.Equations[equationIndex];
        var rowStart = equationIndex * system.VariableCount;

        for (int j = 0; j < system.VariableCount; j++)
        {
            coefficientArray[rowStart + j] = equation.GetCoefficient(system.Variables[j]);
        }

        constantArray[equationIndex] = equation.Constant;
    }

    /// <summary>
    /// Returns rented arrays to the pool.
    /// </summary>
    private void ReturnRentedArrays()
    {
        if (_rentedCoefficients != null)
        {
            _arrayPool.Return(_rentedCoefficients);
            _rentedCoefficients = null;
        }

        if (_rentedConstants != null)
        {
            _arrayPool.Return(_rentedConstants);
            _rentedConstants = null;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            ReturnRentedArrays();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~MatrixConverter()
    {
        Dispose();
    }
}