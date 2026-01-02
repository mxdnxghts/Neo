using MathNet.Numerics.LinearAlgebra;
using Neo.Services;
using Neo.Utilities;

namespace TestNeoSoftware;

public class TestEquationParser
{
    const string equationInput = "x - 2y + 3z = 4;5x - 6y + z = 8;9x + y + 11.5z =-  12.5;";
    const string matrixInput = "1.3 -2 3 4;5 -6 7 8;9 10 11.5 -12.5;";
    private Solver _solver;

    [SetUp]
    public void Setup()
    {
        _solver = new Solver();
        //_parser = new Parser(equationInput);
    }

    [Test]
    public void TestMatrixConversion()
    {
        var correctMatrix = new[,]
        {
            { 1d, -2, 3 },
            { 5, -6, 1 },
            { 9, 1, 11.5d },
        };

        var expectedMatrix = Matrix<double>.Build.DenseOfArray(correctMatrix);
        // var actualMatrix = _parser.MatrixConversion();
        Matrix<double> actualMatrix = null;

        Assert.That(actualMatrix, Is.EqualTo(expectedMatrix));
    }

    [Test]
    public void TestVectorConversion()
    {
        var correctVector = new[]
        {
            4, 8, -12.5d,
        };
        var expectedVector = Vector<double>.Build.DenseOfArray(correctVector);
        // var actualVector = _parser.VectorConversion();
        Vector<double> actualVector = null;

        Assert.That(actualVector, Is.EqualTo(expectedVector));
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
        // var actual = new Parser(equationInput);
        Matrix<double> actual = null;

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestSolve()
    {
        var eq = "-X+2y +3z - 4t = 7;3x - 2y-z+t = 8;9x +12y - 3z + 4t = 5;6x -7y +2z -9t = 18;";
        var expected = "string";
        var solver = _solver.Solve(eq);
        //string actual = _solver.Solve(eq);
        var parser = new Parser(eq);
        Assert.That(parser.MatrixConversion(eq.GetUnknownVariables()).ToString(), Is.EqualTo(expected));
    }
}