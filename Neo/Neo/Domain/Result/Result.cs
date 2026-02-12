using System;

namespace Neo.Domain.Result;

// Replace old global Error class with functional Result pattern
public sealed class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;

    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
    public static Result<T> Failure(string message, string code = "GENERIC_ERROR") =>
        new(default, new Error(message, code));

    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException(Error?.Message ?? "Unknown error");
        return Value!;
    }

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        IsSuccess ? Result<TResult>.Success(mapper(Value!)) : Result<TResult>.Failure(Error!);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) =>
        IsSuccess ? binder(Value!) : Result<TResult>.Failure(Error!);
}

// Non-generic result for operations without return value
public sealed class Result
{
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(Error? error) => Error = error;

    public static Result Success() => new(null);
    public static Result Failure(Error error) => new(error);
    public static Result Failure(string message, string code = "GENERIC_ERROR") =>
        new(new Error(message, code));
}