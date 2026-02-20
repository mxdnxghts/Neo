using System;
using System.Collections.Generic;

namespace Neo.Domain.Result;

/// <summary>
/// Represents an error that occurred during an operation.
/// Contains error code, message, optional exception, and context.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Gets a standardized error for null input.
    /// </summary>
    public static Error NullInput => new("Input is null", "NULL_INPUT");

    /// <summary>
    /// Gets a standardized error for empty input.
    /// </summary>
    public static Error EmptyInput => new("Input is empty", "EMPTY_INPUT");

    /// <summary>
    /// Gets a standardized error for invalid format.
    /// </summary>
    public static Error InvalidFormat => new("Invalid equation format", "INVALID_FORMAT");

    /// <summary>
    /// Gets a standardized error for underdetermined systems.
    /// </summary>
    public static Error Underdetermined => new("Underdetermined system", "UNDERDETERMINED");

    /// <summary>
    /// Gets a standardized error for overdetermined systems.
    /// </summary>
    public static Error Overdetermined => new("Overdetermined system", "OVERDETERMINED");

    /// <summary>
    /// Gets a standardized error for singular matrices.
    /// </summary>
    public static Error SingularMatrix => new("Matrix is singular", "SINGULAR_MATRIX");

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the underlying exception, if any.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the UTC timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets additional context data for the error.
    /// </summary>
    public Dictionary<string, object> Context { get; private set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="exception">Optional inner exception.</param>
    public Error(string message, string code, Exception? exception = null)
    {
        Message = message;
        Code = code;
        Exception = exception;
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{Code}] {Message}";
}