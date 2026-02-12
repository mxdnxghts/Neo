using System;
using System.Collections.Generic;

namespace Neo.Domain.Result;

public sealed record Error
{

    // Common error codes from old system analysis
    public static Error NullInput => new("Input is null", "NULL_INPUT");
    public static Error EmptyInput => new("Input is empty", "EMPTY_INPUT");
    public static Error InvalidFormat => new("Invalid equation format", "INVALID_FORMAT");
    public static Error Underdetermined => new("Underdetermined system", "UNDERDETERMINED");
    public static Error Overdetermined => new("Overdetermined system", "OVERDETERMINED");
    public static Error SingularMatrix => new("Matrix is singular", "SINGULAR_MATRIX");

    public string Message { get; }
    public string Code { get; }
    public Exception Exception { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Dictionary<string, object> Context { get; private set; } = new();

    public Error(string message, string code, Exception? exception = null)
    {
        Message = message;
        Code = code;
        Exception = exception;
    }

    public override string ToString() => $"[{Code}] {Message}";
}