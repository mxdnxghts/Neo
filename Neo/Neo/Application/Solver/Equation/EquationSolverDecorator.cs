using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;

namespace Neo.Application.Solver.Equation;

public abstract class EquationSolverDecorator : IEquationSolver
{
    protected readonly IEquationSolver _inner;

    public EquationSolverDecorator(IEquationSolver inner)
    {
        _inner = inner;
    }

    public virtual Result<Solution> Solve(string equationInput)
    {
        return _inner.Solve(equationInput);
    }

    public virtual Result<Solution> Solve(EquationSystem system)
    {
        return _inner.Solve(system);
    }

    public virtual Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants)
    {
        return _inner.Solve(coefficients, constants);
    }

    public virtual Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken cancellationToken = default)
    {
        return _inner.SolveAsync(equationInput, cancellationToken);
    }

    public virtual Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken cancellationToken = default)
    {
        return _inner.SolveAsync(system, cancellationToken);
    }

    public virtual Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(IEnumerable<string> inputs, CancellationToken cancellationToken = default)
    {
        return _inner.SolveBatchAsync(inputs, cancellationToken);
    }

    public virtual IAsyncEnumerable<Solution> SolveStreamAsync(IAsyncEnumerable<string> equationStream, CancellationToken cancellationToken = default)
    {
        return _inner.SolveStreamAsync(equationStream, cancellationToken);
    }

    public virtual Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm)
    {
        return _inner.SolveWithAlgorithm(input, algorithm);
    }

    public virtual Result<Solution> SolveWithOptions(string input, SolvingOptions options)
    {
        return _inner.SolveWithOptions(input, options);
    }
}
