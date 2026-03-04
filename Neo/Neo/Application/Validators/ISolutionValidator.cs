using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;

namespace Neo.Application.Validators;

/// <summary>
/// Interface for validating solutions to linear equation systems.
/// </summary>
public interface ISolutionValidator
{
    /// <summary>
    /// Validates a solution by computing residuals (Ax - b) and checking against tolerance.
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="solution">The solution to validate.</param>
    /// <param name="tolerance">The maximum acceptable residual error. Default: 1e-10.</param>
    /// <returns><c>true</c> if the solution is valid; otherwise, an error.</returns>
    Result<bool> Validate(EquationSystem system, Solution solution, double tolerance = 1e-10);

    /// <summary>
    /// Determines the solution status by analyzing matrix rank.
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="a">The coefficient matrix.</param>
    /// <param name="b">The constants vector.</param>
    /// <returns>The determined <see cref="SolutionStatus"/>.</returns>
    SolutionStatus DetermineStatus(EquationSystem system, Matrix<double> a, Vector<double> b);
}