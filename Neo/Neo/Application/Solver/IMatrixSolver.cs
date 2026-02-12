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

    // Iterative methods for large sparse systems
    Result<Vector<double>> SolveIterative(
        Matrix<double> a,
        Vector<double> b,
        IterativeSolverOptions options);

    // Least squares for overdetermined systems
    Result<Vector<double>> SolveLeastSquares(Matrix<double> a, Vector<double> b);

    // Condition number and matrix properties
    double ConditionNumber(Matrix<double> matrix);
    bool IsIllConditioned(Matrix<double> matrix, double threshold = 1e10);
}