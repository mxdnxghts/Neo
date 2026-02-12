using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;

namespace Neo.Application.Validators;

public interface ISolutionValidator
{
    Result<bool> ValidateSolution(
        EquationSystem originalSystem,
        Solution solution,
        double tolerance = 1e-10);

    Result<bool> ValidateResiduals(
        Matrix<double> coefficients,
        Vector<double> constants,
        Vector<double> solution,
        double tolerance = 1e-10);

    SolutionStatus DetermineSolutionStatus(
        EquationSystem system,
        Matrix<double> coefficients,
        Vector<double> constants);

    // Check for special cases
    bool IsHomogeneous(EquationSystem system);
    bool IsConsistent(EquationSystem system, double tolerance = 1e-10);
    bool HasUniqueSolution(EquationSystem system);
}
