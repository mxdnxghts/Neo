using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;

namespace Neo.Application.Validators;

public interface ISolutionValidator
{
    Result<bool> Validate(EquationSystem system, Solution solution, double tolerance = 1e-10);
    SolutionStatus DetermineStatus(EquationSystem system, Matrix<double> a, Vector<double> b);
}
