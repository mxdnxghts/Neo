using Neo.Domain.Equation;
using Neo.Domain.Solution;
using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Result;

namespace Neo.Application.Validators;

/// <summary>
/// Validates solutions by computing residuals and analyzing matrix rank.
/// </summary>
public sealed class SolutionValidator : ISolutionValidator
{
    /// <inheritdoc/>
    public Result<bool> Validate(EquationSystem system, Solution solution, double tolerance = 1e-10)
    {
        if (solution.Status != SolutionStatus.Success)
            return Result<bool>.Failure(
                new Error("Cannot validate non‑success solution", "INVALID_STATE"));

        var (coeffs, constants) = system.ToMatrix();
        var a = Matrix<double>.Build.DenseOfArray(coeffs);
        var b = Vector<double>.Build.DenseOfArray(constants);

        var x = Vector<double>.Build.Dense(system.Variables.Count);
        for (int i = 0; i < system.Variables.Count; i++)
            x[i] = solution.GetValue(system.Variables[i]);

        var residual = a * x - b;
        var maxError = residual.InfinityNorm();

        return maxError <= tolerance
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error($"Residual too large: {maxError}", "VALIDATION_FAILED"));
    }

    /// <inheritdoc/>
    public SolutionStatus DetermineStatus(EquationSystem system, Matrix<double> a, Vector<double> b)
    {
        var augmented = a.Append(b.ToColumnMatrix()).AsArray();
        var rankA = a.Rank();
        var rankAug = Matrix<double>.Build.DenseOfArray(augmented).Rank();

        if (rankA < rankAug)
            return SolutionStatus.NoSolution;

        if (rankA < system.VariableCount)
            return SolutionStatus.InfiniteSolutions;

        return SolutionStatus.Success;
    }
}