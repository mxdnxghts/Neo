using System;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Neo.Storage;
using Neo.Utilities;

namespace Neo.Services;

/// <summary>
/// class finds unknown variables of equation/s
/// </summary>
public sealed class Solver
{
    private readonly StorageHandler _storage;
    /// <summary>
    /// passed recognized string (expected system linear equations)
    /// </summary>
    private string _input;

    private string _constantInput;

    private bool _isMatrix;

    /// <summary>
    /// instance of <see cref="Parser"/>
    /// </summary>
    private Parser _parser;

    /// <summary>
    /// implicitly converts instance of <see cref="Solver"/> to string by method <see cref="ToString"/>
    /// </summary>
    /// <param name="solver">instance of <see cref="Solver"/></param>
    /// <returns>converted to <see cref="string"/> instance of <see cref="Solver"/></returns>
    public static implicit operator string(Solver solver) => solver.ToString();

    /// <summary>
    /// implicitly converts instance of <see cref="Solver"/> to <see cref="Vector{T}"/>
    /// </summary>
    /// <param name="solver">instance of <see cref="Solver"/></param>
    /// <returns>converted to <see cref="Vector{T}"/> instance of <see cref="Solver"/></returns>
    public static implicit operator Vector<double>(Solver solver) => solver.Result;

    /// <summary>
    /// returns instance of <see cref="Solver"/> with different implicit and explicit operators
    /// </summary>
    public Solver()
    {
    }

    public Solver(StorageHandler storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// returns instance of <see cref="Solver"/> with solved matrix
    /// </summary>
    /// <param name="input"><see cref="_input"/></param>
    public async Task<Solver> SolveAsync(string input)
    {
        _input = SetInputConfiguration(input);

        if (string.IsNullOrEmpty(_input))
            return this;

        _parser = new Parser(_input);
        try
        {
            return await SolveAsync(_input.GetUnknownVariables(), null);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        return this;
    }

    /// <summary>
    /// returns instance of <see cref="Solver"/> with solved matrix
    /// </summary>
    /// <param name="matrix">matrix for conversion to <see cref="_input"/></param>
    /// <returns></returns>
    public async Task<Solver> SolveAsync(Matrix<double> matrix)
    {
        _isMatrix = true;
        _parser = new Parser(string.Empty);
        try
        {
            return await SolveAsync(null, matrix);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        return this;
    }

    private string SetInputConfiguration(string input)
    {
        _constantInput = input;
        _input = input.Replace("\n", Parser.SplitSymbol.ToString()).ToLower();
        _input = _input.AppendZeroCoefficients(input.GetUnknownVariables()).RemoveWhiteSpacesNearSeparator();

        if (_input is "no text" or "" || _input.IsTrash())
            return string.Empty;

        return _input;
    }

    /// <summary>
    /// returns instance of <see cref="Solver"/> with solved matrix
    /// </summary>
    /// <param name="input"><see cref="_input"/></param>
    public Solver Solve(string input)
    {
        _input = SetInputConfiguration(input);

        if (string.IsNullOrEmpty(_input))
            return this;

        _parser = new Parser(_input);
        try
        {
            return Solve(_input.GetUnknownVariables(), null);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        return this;
    }

    /// <summary>
    /// returns instance of <see cref="Solver"/> with solved matrix
    /// </summary>
    /// <param name="matrix">matrix for conversion to <see cref="_input"/></param>
    /// <returns></returns>
    public Solver Solve(Matrix<double> matrix)
    {
        _isMatrix = true;
        _parser = new Parser(string.Empty);
        try
        {
            return Solve(null, matrix);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        return this;
    }

    /// <summary>
    /// solves system linear equations. set values to <see cref="Result"/>, <see cref="LeftSide"/>, <see cref="RightSide"/>
    /// </summary>
    private async Task<Solver> SolveAsync(string unknownVariables, Matrix<double> matrix)
    {
        LeftSide = _parser.MatrixConversion(unknownVariables, matrix);
        if (LeftSide is null)
        {
            Error.Message = $"{nameof(LeftSide)} is null.";
            return this;
        }

        RightSide = _parser.VectorConversion(unknownVariables, matrix);
        if (RightSide is null)
        {
            Error.Message = $"{nameof(RightSide)} is null.";
            return this;
        }

        try
        {
            Result = LeftSide.Solve(RightSide);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        var saved = await _storage.SaveAsync(this);
        if (saved.IsFailed)
            Error.Message = "Failed to save";

        return this;
    }

    /// <summary>
    /// solves system linear equations. set values to <see cref="Result"/>, <see cref="LeftSide"/>, <see cref="RightSide"/>
    /// </summary>
    private Solver Solve(string unknownVariables, Matrix<double> matrix)
    {
        LeftSide = _parser.MatrixConversion(unknownVariables, matrix);
        if (LeftSide is null)
        {
            Error.Message = $"{nameof(LeftSide)} is null.";
            return this;
        }

        RightSide = _parser.VectorConversion(unknownVariables, matrix);
        if (RightSide is null)
        {
            Error.Message = $"{nameof(RightSide)} is null.";
            return this;
        }

        try
        {
            Result = LeftSide.Solve(RightSide);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }

        var saved = _storage.Save(this);
        if (saved.IsFailed)
            Error.Message = "Failed to save";

        return this;
    }

    /// <summary>
    /// presents matrix (digits before "=") of system linear equation
    /// </summary>
    public Matrix<double> LeftSide { get; private set; }

    /// <summary>
    /// presents result's sequence (digits after "=") of system linear equation
    /// </summary>
    public Vector<double> RightSide { get; private set; }

    /// <summary>
    /// presents results of unknown variables of system linear equations
    /// </summary>
    private Vector<double> Result { get; set; }

    public override string ToString()
    {
        if (Error.Message is not null)
            return $"{Error.Message}\n{Error.InnerMessage}\n{Error.ArgValues}";

        var sb = new StringBuilder();
        if (_isMatrix)
        {
            var index = 1;
            for (var i = 0; i < Result.Count; i++, index++)
                sb.AppendLine($"x{index}: {Result[i]}");
            return sb.ToString();
        }

        for (var i = 0; i < Result.Count; i++)
            sb.AppendLine($"{_constantInput.GetUnknownVariables()[i]}: {Result[i]}");

        return sb.ToString();
    }
}