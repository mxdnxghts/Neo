using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Domain.Equation;

// Based on old code: system of equations
public sealed class EquationSystem
{
    private readonly List<LinearEquation> _equations;
    private readonly List<Variable> _variables;

    public IReadOnlyList<LinearEquation> Equations => _equations;
    public IReadOnlyList<Variable> Variables => _variables;

    public int EquationCount => _equations.Count;
    public int VariableCount => _variables.Count;

    public EquationSystem(IEnumerable<LinearEquation> equations)
    {
        if (equations is null)
            throw new ArgumentNullException(nameof(equations));

        _equations = [.. equations];
        if (_equations.Count == 0)
            throw new ArgumentException("At least one equation is required", nameof(equations));

        // From old code: GetUnknownVariables() gets variables from longest equation
        _variables = ExtractAllVariables(_equations);

        Validate();
    }

    private List<Variable> ExtractAllVariables(List<LinearEquation> equations)
    {
        // From old code: GetUnknownVariables gets distinct letters from longest string
        // We'll get all variables from all equations
        var allVariables = new HashSet<Variable>();

        foreach (var equation in equations)
        {
            foreach (var variable in equation.Variables)
            {
                allVariables.Add(variable);
            }
        }

        // From old code: variables ordered by appearance in input
        // For now, order alphabetically
        return allVariables.OrderBy(v => v.Name).ToList();
    }

    private void Validate()
    {
        // From old code: IsTrash() validation
        if (_equations.Count > 100) // Arbitrary limit
            throw new InvalidOperationException("Too many equations");

        if (_variables.Count > 26) // a-z
            throw new InvalidOperationException("Too many variables (max 26)");

        // Check if system can be solved
        if (_equations.Count < _variables.Count)
            throw new InvalidOperationException("Underdetermined system: more variables than equations");
    }

    // From old code: AppendZeroCoefficients ensures all equations have all variables
    public EquationSystem Normalize()
    {
        var normalizedEquations = new List<LinearEquation>();

        foreach (var equation in _equations)
        {
            var normalizedEquation = equation;
            foreach (var variable in _variables)
            {
                normalizedEquation = normalizedEquation.WithZeroCoefficient(variable);
            }
            normalizedEquations.Add(normalizedEquation);
        }

        return new EquationSystem(normalizedEquations);
    }

    // From old code: MatrixConversion prepares for MathNet
    public (double[,] Coefficients, double[] Constants) ToMatrix()
    {
        var normalized = Normalize();
        var coefficients = new double[normalized.EquationCount, normalized.VariableCount];
        var constants = new double[normalized.EquationCount];

        for (int i = 0; i < normalized.EquationCount; i++)
        {
            var equation = normalized.Equations[i];
            for (int j = 0; j < normalized.VariableCount; j++)
            {
                coefficients[i, j] = equation.GetCoefficient(normalized.Variables[j]);
            }
            constants[i] = equation.Constant;
        }

        return (coefficients, constants);
    }

    // From old code: Check if matrix is square
    public bool IsSquare => EquationCount == VariableCount;

    public override string ToString()
    {
        return string.Join(Environment.NewLine, _equations.Select(e => e.ToString()));
    }
}
