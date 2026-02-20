using Microsoft.Extensions.DependencyInjection;
using Neo.Application.Solver.Equation;
using Neo.CompositionRoot;
using Neo.Domain.Solution;

namespace Neo.ConsoleApp;

/// <summary>
/// Console application demonstrating dependency injection setup for the Neo equation solver.
/// </summary>
public class Program
{
    private static readonly string[] Examples = new[]
    {
        "2x + 3y = 5; x - y = 1",
        "x = 5",
        "2x = 10",
        "3x + 2y = 12; x - y = 1",
        "-2x + -3y = -5; x - y = 1",
        "2.5x = 7.5",
        "x + y + z = 6; 2x - y + z = 3; x + y - z = 2"
    };

    public static void Main(string[] args)
    {
        Console.WriteLine("=== Neo Linear Equation Solver ===");
        Console.WriteLine();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddNeoEquationSolver(options =>
        {
            options.EnableCaching = true;
            options.CacheTtl = TimeSpan.FromMinutes(30);
            options.DefaultAlgorithm = SolvingAlgorithm.LU;
            options.ValidationTolerance = 1e-10;
            options.MaxDegreeOfParallelism = -1; // Use system default
        });

        var serviceProvider = services.BuildServiceProvider();
        var solver = serviceProvider.GetRequiredService<IEquationSolver>();
        var performanceMonitor = serviceProvider.GetRequiredService<Infrastructure.Integration.PerformanceMonitor>();

        // Run examples
        Console.WriteLine("Running example equations:");
        Console.WriteLine(new string('-', 50));

        foreach (var example in Examples)
        {
            Console.WriteLine($"\nEquation: {example}");
            try
            {
                var result = solver.Solve(example);

                if (result.IsSuccess)
                {
                    Console.WriteLine("Solution:");
                    PrintSolution(result.Value);
                }
                else
                {
                    Console.WriteLine($"Error: [{result.Error!.Code}] {result.Error.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        // Display performance statistics
        Console.WriteLine();
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("Performance Statistics:");
        Console.WriteLine(new string('-', 50));

        var stats = performanceMonitor.GetStats("SolveString");
        Console.WriteLine($"Total operations: {stats.TotalOperations}");
        Console.WriteLine($"Success rate: {stats.SuccessRate:P1}");
        Console.WriteLine($"Average duration: {stats.AverageDuration.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Min duration: {stats.MinDuration.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Max duration: {stats.MaxDuration.TotalMilliseconds:F2} ms");

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void PrintSolution(Solution solution)
    {
        if (solution.Status != SolutionStatus.Success)
        {
            Console.WriteLine($"  Status: {solution.Status}");
            if (!string.IsNullOrEmpty(solution.Message))
            {
                Console.WriteLine($"  Message: {solution.Message}");
            }
            return;
        }

        foreach (var kvp in solution.Values)
        {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value:F6}");
        }
    }
}
