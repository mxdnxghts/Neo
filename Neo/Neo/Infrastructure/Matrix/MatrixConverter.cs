using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Matrix;

public sealed class MatrixConverter : IMatrixConverter, IDisposable
{
    private readonly ArrayPool<double> _arrayPool;
    private double[]? _rentedCoefficients;
    private double[]? _rentedConstants;
    private bool _disposed;

    public MatrixConverter(ArrayPool<double>? arrayPool = null)
    {
        _arrayPool = arrayPool ?? ArrayPool<double>.Shared;
    }

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

    public (double[,] Coefficients, double[] Constants) ToArrays(EquationSystem system)
    {
        ThrowIfDisposed();

        var normalized = system.Normalize();
        var equationCount = normalized.EquationCount;
        var variableCount = normalized.VariableCount;

        // Rent arrays
        _rentedCoefficients = _arrayPool.Rent(equationCount * variableCount);
        _rentedConstants = _arrayPool.Rent(equationCount);

        try
        {
            // Fill arrays
            FillArrays(normalized, _rentedCoefficients, _rentedConstants);

            // Convert to 2D array and return copy
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

    private void FillArrays(EquationSystem system, double[] coefficientArray, double[] constantArray)
    {
        // Use parallel for large systems
        if (system.EquationCount > 10)
        {
            Parallel.For(0, system.EquationCount, i =>
            {
                var equation = system.Equations[i];
                var rowStart = i * system.VariableCount;

                for (int j = 0; j < system.VariableCount; j++)
                {
                    coefficientArray[rowStart + j] = equation.GetCoefficient(system.Variables[j]);
                }

                constantArray[i] = equation.Constant;
            });
        }
        else
        {
            for (int i = 0; i < system.EquationCount; i++)
            {
                var equation = system.Equations[i];
                var rowStart = i * system.VariableCount;

                for (int j = 0; j < system.VariableCount; j++)
                {
                    coefficientArray[rowStart + j] = equation.GetCoefficient(system.Variables[j]);
                }

                constantArray[i] = equation.Constant;
            }
        }
    }

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
        if (_disposed)
            throw new ObjectDisposedException(nameof(MatrixConverter));
    }

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
