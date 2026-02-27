using Neo.Application.Solver.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Telemetry;

namespace NeoTelemetry.ApiService.Endpoints;

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
            using var activity = telemetry.StartSolveActivity("API.SolveEquation", request.InputHash);
            
            try
            {
                var result = solver.Solve(request.Input);
                
                return result.IsSuccess 
                    ? Results.Ok(new SolveResponse(result.Value))
                    : Results.BadRequest(new { error = result.Error.ToString() });
            }
            catch (Exception ex)
            {
                telemetry.RecordError("API.SolveEquation", ex.GetType().Name, ex.Message);
                return Results.Problem(ex.Message);
            }
        })
        .WithName("SolveEquation")
        .WithOpenApi();

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
                    ? Results.Ok(new BatchSolveResponse(result.Value))
                    : Results.BadRequest(new { error = result.Error.ToString() });
            }
            catch (Exception ex)
            {
                telemetry.RecordError("API.SolveEquationBatch", ex.GetType().Name, ex.Message);
                return Results.Problem(ex.Message);
            }
        })
        .WithName("SolveEquationBatch")
        .WithOpenApi();

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithOpenApi();
    }
}

/// <summary>
/// Request to solve a single equation system.
/// </summary>
/// <param name="Input">The equation input string (e.g., "2x + 3y = 5; x - y = 1").</param>
/// <param name="InputHash">Optional hash for caching and correlation.</param>
public record SolveRequest(string Input, string? InputHash = null);

/// <summary>
/// Response containing the solution.
/// </summary>
/// <param name="Solution">The solution result.</param>
public record SolveResponse(Solution Solution);

/// <summary>
/// Request to solve multiple equations.
/// </summary>
/// <param name="Inputs">Semicolon-separated equation inputs.</param>
public record BatchRequest(string? Inputs);

/// <summary>
/// Response containing multiple solutions.
/// </summary>
/// <param name="Solutions">The list of solutions.</param>
public record BatchSolveResponse(IReadOnlyList<Solution> Solutions);
