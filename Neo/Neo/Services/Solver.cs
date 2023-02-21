﻿using System;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Neo.Utilities;

namespace Neo.Services;

/// <summary>
/// class finds unknown variables of equation/s
/// </summary>
public sealed class Solver : IDisposable
{
    /// <summary>
    /// passed recognized string (expected system linear equations)
    /// </summary>
    private readonly string _input;

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
    /// explicitly converts passed <see cref="_input"/> to <see cref="Solver"/>
    /// </summary>
    /// <param name="solver">instance of <see cref="Solver"/></param>
    /// <returns>instance of <see cref="Solver"/></returns>
    public static explicit operator Solver(string input) => new(input);

    /// <summary>
    /// returns instance of <see cref="Solver"/> with different implicit and explicit operators
    /// </summary>
    /// <param name="input"><see cref="_input"/></param>
    public Solver(string input)
    {
        _input = input.Replace("\n", Parser.SplitSymbol.ToString()).ToLower();

        if (_input is "no text" or "" || _input.IsTrash())
        {
            Error.Message = "didn't read anything.";
            Error.ArgValues = input;
            return;
        }


        _parser = new Parser(_input);
        try
        {
            Solve();
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    public Solver(Matrix<double> matrix)
    {
        _parser = new Parser(string.Empty);
        try
        {
            Solve(false, matrix);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
        }
    }

    /// <summary>
    /// solves system linear equations. set values to <see cref="Result"/>, <see cref="LeftSide"/>, <see cref="RightSide"/>
    /// </summary>
    private void Solve(bool str = true, Matrix<double> matrix = null)
    {
        LeftSide = _parser.MatrixConversion(str, matrix);
        if (LeftSide is null)
        {
            Error.Message = $"{nameof(LeftSide)} is null.";
            Dispose();
            return;
        }

        RightSide = _parser.VectorConversion(str, matrix);
        if (RightSide is null)
        {
            Error.Message = $"{nameof(RightSide)} is null.";
            Dispose();
            return;
        }

        try
        {
            Result = LeftSide.Solve(RightSide);
        }
        catch (Exception exception)
        {
            Error.Message = exception.Message;
            Error.InnerMessage = exception.InnerException?.Message;
            Dispose();
        }
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
        for (var i = 0; i < Result.Count; i++)
            sb.AppendLine($"{_input.GetUnknownVariables()[i]}: {Result[i]}");

        return sb.ToString();
    }

    private void ReleaseUnmanagedResources()
    {
        LeftSide = null;
        RightSide = null;
        Result = null;
        _parser = null;
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Solver()
    {
        Dispose(false);
    }
}