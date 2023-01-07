﻿using System;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Neo.Utilities;

namespace Neo.Services;

/// <summary>
/// class finds unknown variables of equation
/// </summary>
public sealed class Solver : IDisposable
{
    private readonly string _input;
    private Parser _parser;

    public static string Input { get; private set; }

    public static implicit operator string(Solver solver) => solver.ToString();
    public static explicit operator Solver(string input) => new(input);

    public Solver(string input)
    {
        _input = input.Replace("\n", Parser.SplitSymbol.ToString()).ToLower();
        Input = _input;
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

    private void Solve()
    {
        LeftSide = _parser.MatrixConversion();
        RightSide = _parser.VectorConversion();
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
    /// describe left side (matrix) of equation
    /// </summary>
    public Matrix<double> LeftSide { get; private set; }

    /// <summary>
    /// describe right side (results) of equation
    /// </summary>
    public Vector<double> RightSide { get; private set; }

    /// <summary>
    /// describe results of unknown variables of system linear equations
    /// </summary>
    public Vector<double> Result { get; private set; }

    public override string ToString()
    {
        if (Error.Message is not null)
            return Error.Message;
        if (Error.InnerMessage is not null)
            return Error.InnerMessage;

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