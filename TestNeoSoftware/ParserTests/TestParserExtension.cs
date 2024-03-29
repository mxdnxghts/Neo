﻿using MathNet.Numerics.LinearAlgebra;
using Neo.Services;

namespace TestNeoSoftware;

public class TestParserExtension
{
    const string equationInput = " - 2y + 3z = 4; - 6y + z = 8;9x + y + 11.5z =-  12.5;";
    const string matrixInput = "2 3 4;6 7 8;10 11 12;";

    [SetUp]
    public void Setup()
    {
        // _parser = new Parser(equationInput);
    }

    [Test]
    public void TestOnUnitVariable()
    {
        var correctMatrix = new[,]
        {
            { 1d, -2, 3 },
            { 5, -6, 1 },
            { 9, 1, 11.5d },
        };
        var expected = Matrix<double>.Build.DenseOfArray(correctMatrix);
        // var actualMatrix = _parser.MatrixConversion();
        Matrix<double> actual = null;

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestSolverOnUnit()
    {
        var expected = "x: -1.3333333333333333\r\ny: -2.416666666666667\r\nz: 0.1666666666666667\r\n";
        string actual = new Solver(equationInput);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestOnZeroVariable()
    {
        var expected = $"0 {equationInput}";
        // var actual = equationInput.OnZeroVariable();
        var actual = string.Empty;

        Assert.AreEqual(expected, actual);
    }
}