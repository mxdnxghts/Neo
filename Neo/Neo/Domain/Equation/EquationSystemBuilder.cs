using Neo.Domain.Equation.Variables;
using Neo.Domain.Result;
using System;
using System.Collections.Generic;

namespace Neo.Domain.Equation;

public class EquationSystemBuilder
{
    private readonly List<LinearEquation> _equations = [];
    private readonly HashSet<Variable> _variables = [];

    public EquationSystemBuilder AddEquation(LinearEquation equation)
    {
        _equations.Add(equation);
        foreach (var variable in equation.Variables)
        {
            _variables.Add(variable);
        }
        return this;
    }

    public EquationSystemBuilder AddEquation(string equationString)
    {
        // Will use proper parser later
        throw new NotImplementedException();
    }

    public Result<EquationSystem> Build()
    {
        try
        {
            var system = new EquationSystem(_equations);
            return Result<EquationSystem>.Success(system);
        }
        catch (Exception ex)
        {
            return Result<EquationSystem>.Failure(
                new Error($"Failed to build equation system: {ex.Message}", "BUILD_ERROR", ex));
        }
    }
}