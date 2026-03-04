using Neo.Application.Solver.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeoTelemetry.ApiService.Endpoints;

/// <summary>
/// DTO for equation solving response - JSON serializable.
/// </summary>
public record SolveResponseDto(
    int VariableCount,
    int EquationCount,
    string Status,
    Dictionary<string, double> Values,
    string? AlgorithmUsed,
    string? Message);

/// <summary>
/// DTO for batch solving response - JSON serializable.
/// </summary>
public record BatchSolveResponseDto(
    List<BatchSolutionDto> Solutions);

/// <summary>
/// DTO for a single solution in batch response.
/// </summary>
public record BatchSolutionDto(
    int VariableCount,
    int EquationCount,
    string Status,
    Dictionary<string, double> Values,
    string? AlgorithmUsed,
    string? Message);

/// <summary>
/// Extension methods for mapping equation solver endpoints.
/// </summary>
public static class EquationEndpoints
{
    /// <summary>
    /// Maps equation solver endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapEquationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/equations/solve", async (
            SolveRequest request,
            IEquationSolver solver,
            NeoTelemetryService telemetry,
            CancellationToken ct) =>
        {
            try
            {
                var result = await solver.SolveAsync(request.Input, ct);

                return result.IsSuccess
                    ? Results.Ok(CreateSolveResponseDto(result.Value!))
                    : Results.BadRequest(new { error = result.Error?.ToString() ?? "Unknown error" });
            }
            catch (OperationCanceledException)
            {
                return Results.Problem("Request was cancelled", statusCode: 499);
            }
            catch (Exception ex)
            {
                return Results.Problem(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while solving the equation",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/500"
                });
            }
        })
        .WithName("SolveEquation")
        .AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            // Per-endpoint tweaks
            operation.Summary = "Solves system equations";
            return Task.CompletedTask;
        });

        app.MapGet("/api/equations/solve-batch", async (
            [AsParameters] BatchRequest request,
            IEquationSolver solver,
            NeoTelemetryService telemetry,
            CancellationToken ct) =>
        {
            using var activity = telemetry.StartSolveActivity("API.SolveEquationBatch");

            try
            {
                var inputs = request.Inputs?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                var result = await solver.SolveBatchAsync(inputs, ct);

                return result.IsSuccess
                    ? Results.Ok(CreateBatchSolveResponseDto(result.Value!))
                    : Results.BadRequest(new { error = result.Error?.ToString() ?? "Unknown error" });
            }
            catch (OperationCanceledException)
            {
                return Results.Problem("Request was cancelled", statusCode: 499);
            }
            catch (Exception ex)
            {
                telemetry.RecordError("API.SolveEquationBatch", ex.GetType().Name, ex.Message);
                return Results.Problem(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while solving the equations",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/500"
                });
            }
        })
        .WithName("SolveEquationBatch")
        .AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            // Per-endpoint tweaks
            operation.Summary = "Solves systems equations";
            return Task.CompletedTask;
        });

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                // Per-endpoint tweaks
                operation.Summary     = "Checks health of the API";
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Creates a JSON-serializable DTO from a Solution.
    /// </summary>
    private static SolveResponseDto CreateSolveResponseDto(Solution solution)
    {
        return new SolveResponseDto(
            VariableCount: solution.OriginalSystem.VariableCount,
            EquationCount: solution.OriginalSystem.EquationCount,
            Status: solution.Status.ToString(),
            Values: solution.Values.ToDictionary(kv => kv.Key.Name, kv => kv.Value),
            AlgorithmUsed: solution.AlgorithmUsed?.ToString(),
            Message: solution.Message);
    }

    /// <summary>
    /// Creates a JSON-serializable DTO from a collection of Solutions.
    /// </summary>
    private static BatchSolveResponseDto CreateBatchSolveResponseDto(IReadOnlyList<Solution> solutions)
    {
        var batchSolutions = solutions.Select(s => new BatchSolutionDto(
            VariableCount: s.OriginalSystem.VariableCount,
            EquationCount: s.OriginalSystem.EquationCount,
            Status: s.Status.ToString(),
            Values: s.Values.ToDictionary(kv => kv.Key.Name, kv => kv.Value),
            AlgorithmUsed: s.AlgorithmUsed?.ToString(),
            Message: s.Message)).ToList();

        return new BatchSolveResponseDto(batchSolutions);
    }
}

/// <summary>
/// Request to solve a single equation system.
/// </summary>
/// <param name="Input">The equation input string (e.g., "2x + 3y = 5; x - y = 1").</param>
/// <param name="InputHash">Optional hash for caching and correlation.</param>
public record SolveRequest(string Input, string? InputHash = null);

/// <summary>
/// Request to solve multiple equations.
/// </summary>
/// <param name="Inputs">Semicolon-separated equation inputs.</param>
public record BatchRequest(string? Inputs);
