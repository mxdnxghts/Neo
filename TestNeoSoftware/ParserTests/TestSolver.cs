using MathNet.Numerics.LinearAlgebra;
using Neo.Services;

namespace TestNeoSoftware;

public class TestSolver
{
    // const string equationInput = "x - 2y + 3z = 4;5x - 6y + z = 8;9x + y + 11.5z =-  12.5;";

    const string equationInput = "1.3x - 2y + 3z = 4;5x - 6y + 7z = 8;9x + 10y + 11.5z =-  12.5;";
    const string matrixInput = "1.3 -2 3 4;5 -6 7 8;9 10 11.5 -12.5;";
    private Solver _solver;

    [SetUp]
    public void Setup()
    {
        //_parser = new Parser(equationInput);
        _solver = new Solver();
    }

    [Test]
    public void TestSolverToString()
    {
        var expected = "x: -1.5128844555278471\r\ny: -1.2315045719035744\r\nz: 1.167913549459684\r\n";
        string actual = _solver.Solve(equationInput);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestSolverOnUnit()
    {
        var expected = "x: -1.3333333333333333\r\ny: -2.416666666666667\r\nz: 0.1666666666666667\r\n";
        string actual = _solver.Solve(equationInput);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestSolverMatrixCtor()
    {
        var expected = "x: -1.3333333333333333\r\ny: -2.416666666666667\r\nz: 0.1666666666666667\r\n";
        var matrix = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { 1, 2, 3, },
            { 4, 5, 6, },
        });
        string actual = new Solver().Solve(matrix);

        Assert.That(actual, Is.EqualTo(expected));
    }


    [Test]
    public void TestSolverResult()
    {
        var expected = Vector<double>.Build.DenseOfArray(new[]
        {
            -1.5128844555278470,
            -1.2315045719035744,
            1.1679135494596841,
        });

        Vector<double> actual = new Solver().Solve(equationInput);

        Assert.That(actual, Is.EqualTo(expected));
    }
}