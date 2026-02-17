using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Result;

namespace Neo.Application.Solver;

public interface IMatrixSolver
{
    // Different solving algorithms
    Result<Vector<double>> Solve(Matrix<double> a, Vector<double> b, SolvingAlgorithm algorithm);
    Result<Vector<double>> SolveLU(Matrix<double> a, Vector<double> b);
    Result<Vector<double>> SolveQR(Matrix<double> a, Vector<double> b);
    Result<Vector<double>> SolveCholesky(Matrix<double> a, Vector<double> b);
    Result<Vector<double>> SolveSVD(Matrix<double> a, Vector<double> b);

    // Condition number and matrix properties
    double ConditionNumber(Matrix<double> matrix);
}