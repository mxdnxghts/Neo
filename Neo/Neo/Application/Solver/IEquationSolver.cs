using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application.Solver;

public interface IEquationSolver
{
    // Single equation system solving
    Result<Solution> Solve(string equationInput);
    Result<Solution> Solve(EquationSystem system);
    Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants);

    // Async variants
    Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken cancellationToken = default);
    Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken cancellationToken = default);

    // Batch solving
    Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    // Streaming (for large datasets)
    IAsyncEnumerable<Solution> SolveStreamAsync(
        IAsyncEnumerable<string> equationStream,
        CancellationToken cancellationToken = default);

    // Advanced solving options
    Result<Solution> SolveWithOptions(string input, SolvingOptions options);
    Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm);
}
