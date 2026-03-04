using BenchmarkDotNet.Running;
using NeoBenchmark.Batch;
using NeoBenchmark.Caching;
using NeoBenchmark.EndToEnd;
using NeoBenchmark.Memory;
using NeoBenchmark.Parsing;
using NeoBenchmark.Solving;

namespace NeoBenchmark;

/// <summary>
/// Neo Benchmark Suite - Performance testing for the equation solver system.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("       Neo Benchmark Suite");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("Select benchmark category to run:");
        Console.WriteLine("  1. Parsing Benchmarks");
        Console.WriteLine("  2. Solving Benchmarks");
        Console.WriteLine("  3. Caching Benchmarks");
        Console.WriteLine("  4. Batch Processing Benchmarks");
        Console.WriteLine("  5. Memory Allocation Benchmarks");
        Console.WriteLine("  6. End-to-End Benchmarks");
        Console.WriteLine("  7. Run All Benchmarks");
        Console.WriteLine("  8. Quick Run (selected benchmarks)");
        Console.WriteLine();
        Console.Write("Enter choice (1-8): ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                RunParsingBenchmarks();
                break;
            case "2":
                RunSolvingBenchmarks();
                break;
            case "3":
                RunCachingBenchmarks();
                break;
            case "4":
                RunBatchBenchmarks();
                break;
            case "5":
                RunMemoryBenchmarks();
                break;
            case "6":
                RunEndToEndBenchmarks();
                break;
            case "7":
                RunAllBenchmarks();
                break;
            case "8":
                RunQuickBenchmarks();
                break;
            default:
                Console.WriteLine("Invalid choice. Running all benchmarks by default.");
                RunAllBenchmarks();
                break;
        }

        Console.WriteLine();
        Console.WriteLine("Benchmark run complete. Press any key to exit...");
        Console.ReadKey();
    }

    private static void RunParsingBenchmarks()
    {
        Console.WriteLine("\n>>> Running Parsing Benchmarks...\n");
        BenchmarkRunner.Run<ParsingBenchmarks>();
    }

    private static void RunSolvingBenchmarks()
    {
        Console.WriteLine("\n>>> Running Solving Benchmarks...\n");
        BenchmarkRunner.Run<SolvingBenchmarks>();
    }

    private static void RunCachingBenchmarks()
    {
        Console.WriteLine("\n>>> Running Caching Benchmarks...\n");
        BenchmarkRunner.Run<CachingBenchmarks>();
    }

    private static void RunBatchBenchmarks()
    {
        Console.WriteLine("\n>>> Running Batch Processing Benchmarks...\n");
        BenchmarkRunner.Run<BatchBenchmarks>();
    }

    private static void RunMemoryBenchmarks()
    {
        Console.WriteLine("\n>>> Running Memory Allocation Benchmarks...\n");
        BenchmarkRunner.Run<MemoryBenchmarks>();
    }

    private static void RunEndToEndBenchmarks()
    {
        Console.WriteLine("\n>>> Running End-to-End Benchmarks...\n");
        BenchmarkRunner.Run<EndToEndBenchmarks>();
    }

    private static void RunAllBenchmarks()
    {
        Console.WriteLine("\n>>> Running All Benchmarks...\n");
        
        var allTypes = new[]
        {
            typeof(ParsingBenchmarks),
            typeof(SolvingBenchmarks),
            typeof(CachingBenchmarks),
            typeof(BatchBenchmarks),
            typeof(MemoryBenchmarks),
            typeof(EndToEndBenchmarks)
        };

        foreach (var type in allTypes)
        {
            Console.WriteLine($"\n--- Running {type.Name} ---\n");
            BenchmarkRunner.Run(type);
        }
    }

    private static void RunQuickBenchmarks()
    {
        Console.WriteLine("\n>>> Running Quick Benchmarks (selected subset)...\n");
        
        // Run a quick subset for development/testing
        BenchmarkRunner.Run<ParsingBenchmarks>();
        BenchmarkRunner.Run<SolvingBenchmarks>();
    }
}
