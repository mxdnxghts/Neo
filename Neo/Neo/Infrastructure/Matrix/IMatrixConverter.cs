using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;

namespace Neo.Infrastructure.Matrix;

public interface IMatrixConverter
{
    Result<Matrix<double>> ToMathNetMatrix(EquationSystem system);
    Result<Vector<double>> ToMathNetVector(EquationSystem system);
    (double[,] Coefficients, double[] Constants) ToArrays(EquationSystem system);
}
