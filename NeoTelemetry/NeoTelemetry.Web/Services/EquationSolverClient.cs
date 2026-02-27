using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace NeoTelemetry.Web.Services;

/// <summary>
/// Client for communicating with the Neo equation solver API.
/// </summary>
/// <param name="httpClient">The HTTP client.</param>
public class EquationSolverClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <summary>
    /// Solves a system of linear equations.
    /// </summary>
    /// <param name="equationInput">The equation input (e.g., "2x + 3y = 5; x - y = 1").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The solution result, or null if failed.</returns>
    public async Task<SolutionResult?> SolveAsync(string equationInput, CancellationToken cancellationToken = default)
    {
        var request = new SolveRequest(equationInput, GenerateHash(equationInput));
        
        var response = await _httpClient.PostAsJsonAsync("/api/equations/solve", request, cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var solveResponse = await response.Content.ReadFromJsonAsync<SolveResponse>(cancellationToken: cancellationToken);
        return solveResponse?.ToSolutionResult();
    }

    /// <summary>
    /// Solves multiple equations in batch.
    /// </summary>
    /// <param name="equationInputs">The equation inputs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of solutions.</returns>
    public async Task<IReadOnlyList<SolutionResult>> SolveBatchAsync(
        IEnumerable<string> equationInputs, 
        CancellationToken cancellationToken = default)
    {
        var inputs = string.Join(";", equationInputs);
        var response = await _httpClient.GetAsync($"/api/equations/solve-batch?inputs={Uri.EscapeDataString(inputs)}", cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var batchResponse = await response.Content.ReadFromJsonAsync<BatchSolveResponse>(cancellationToken: cancellationToken);
        return batchResponse?.Solutions.Select(s => new SolutionResult(s.OriginalSystem.VariableCount, s.Status.ToString())).ToList() 
            ?? Enumerable.Empty<SolutionResult>().ToList();
    }

    /// <summary>
    /// Checks if the API is healthy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if healthy.</returns>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateHash(string input) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}

/// <summary>
/// Request to solve equations.
/// </summary>
/// <param name="Input">The equation input.</param>
/// <param name="InputHash">Optional hash for correlation.</param>
public record SolveRequest(string Input, string? InputHash = null);

/// <summary>
/// Response from the solve endpoint.
/// </summary>
/// <param name="Solution">The solution.</param>
public record SolveResponse(Neo.Domain.Solution.Solution Solution)
{
    public SolutionResult ToSolutionResult() =>
        new(Solution.OriginalSystem.VariableCount, Solution.Status.ToString());
};

/// <summary>
/// Batch solve response.
/// </summary>
/// <param name="Solutions">The solutions.</param>
public record BatchSolveResponse(IReadOnlyList<Neo.Domain.Solution.Solution> Solutions);

/// <summary>
/// Simplified solution result for the UI.
/// </summary>
/// <param name="VariableCount">Number of variables.</param>
/// <param name="Status">Solution status.</param>
public record SolutionResult(int VariableCount, string Status);
