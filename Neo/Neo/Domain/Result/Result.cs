using System;

namespace Neo.Domain.Result;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Uses the functional Result pattern to avoid exceptions for expected failures.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// Gets the value if the operation succeeded. Null if failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error details if the operation failed. Null if succeeded.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A new <see cref="Result{T}"/> with IsSuccess=true.</returns>
    public static Result<T> Success(T value) => new(value, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <returns>A new <see cref="Result{T}"/> with IsSuccess=false.</returns>
    public static Result<T> Failure(Error error) => new(default, error);

    /// <summary>
    /// Creates a failed result with the specified message and code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new <see cref="Result{T}"/> with IsSuccess=false.</returns>
    public static Result<T> Failure(string message, string code = "GENERIC_ERROR") =>
        new(default, new Error(message, code));

    /// <summary>
    /// Gets the value if successful, or throws an exception if failed.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException(Error?.Message ?? "Unknown error");
        return Value!;
    }

    /// <summary>
    /// Maps the success value to a new type using the specified function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="onSuccess">Function to transform the success value.</param>
    /// <param name="onFailure">Function to handle the error.</param>
    /// <returns>The transformed result.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    /// <summary>
    /// Maps the success value to a new type, preserving failure state.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">Function to transform the success value.</param>
    /// <returns>A new <see cref="Result{TResult}"/>.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        IsSuccess ? Result<TResult>.Success(mapper(Value!)) : Result<TResult>.Failure(Error!);

    /// <summary>
    /// Binds to a function that returns a Result, flattening the result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">Function that returns a Result.</param>
    /// <returns>A flattened <see cref="Result{TResult}"/>.</returns>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) =>
        IsSuccess ? binder(Value!) : Result<TResult>.Failure(Error!);
}

/// <summary>
/// Represents the result of an operation without a return value.
/// </summary>
public sealed class Result
{
    /// <summary>
    /// Gets the error details if the operation failed. Null if succeeded.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

    private Result(Error? error) => Error = error;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A new <see cref="Result"/> with IsSuccess=true.</returns>
    public static Result Success() => new(null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <returns>A new <see cref="Result"/> with IsSuccess=false.</returns>
    public static Result Failure(Error error) => new(error);

    /// <summary>
    /// Creates a failed result with the specified message and code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new <see cref="Result"/> with IsSuccess=false.</returns>
    public static Result Failure(string message, string code = "GENERIC_ERROR") =>
        new(new Error(message, code));
}