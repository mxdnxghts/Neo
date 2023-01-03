﻿using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace Neo.Services;

public sealed class EquationParser
{
    private readonly MatrixParser _matrixParser;

    public EquationParser(string input)
    {
        _matrixParser = new MatrixParser(input.GetDigits());
    }

    public Matrix<double> MatrixConversion() => _matrixParser.MatrixConversion();

    public Vector<double> VectorConversion() => _matrixParser.VectorConversion();
}

public static partial class ListExtension
{
    public static string GetDigits(this string input)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            // if input[i] neither is digit nor is split symbol ";"
            // cycle is iterating
            if (!char.IsDigit(input[i]) && input[i] != MatrixParser.SplitSymbol)
                continue;

            // else adds to string builder input[i] 
            if (char.IsDigit(input[i]) || input[i] == MatrixParser.SplitSymbol)
                sb.Append(input[i]);

            // stop cycle if "i" more than length of input string
            if (i >= input.Length - 1)
                break;

            // if next element of input neither is digit nor is split symbol ";"
            // adds whitespace
            if (!char.IsDigit(input[i + 1]) && input[i + 1] != MatrixParser.SplitSymbol)
                sb.Append(' ');
        }

        return sb.ToString();
    }
}