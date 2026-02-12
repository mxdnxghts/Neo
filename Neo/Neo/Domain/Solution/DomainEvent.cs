using Neo.Domain.Equation;
using Neo.Domain.Result;
using System;

namespace Neo.Domain.Solution;

public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public class EquationSystemCreated : DomainEvent
{
    public EquationSystem System { get; }

    public EquationSystemCreated(EquationSystem system)
    {
        System = system;
    }
}

public class EquationSystemSolved : DomainEvent
{
    public EquationSystem System { get; }
    public Solution Solution { get; }

    public EquationSystemSolved(EquationSystem system, Solution solution)
    {
        System = system;
        Solution = solution;
    }
}

public class EquationSystemValidationFailed : DomainEvent
{
    public EquationSystem System { get; }
    public Error Error { get; }

    public EquationSystemValidationFailed(EquationSystem system, Error error)
    {
        System = system;
        Error = error;
    }
}
