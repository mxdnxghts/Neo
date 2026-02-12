using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Neo.Services;

namespace NeoBenchmark;

[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser(false)]
public class Benchmarks
{
    private List<int> _list;
    private const int Count = 10_000_000;
    private const string equationInput = "x - 2y + 3z = 4;5x - 6y + z = 8;9x + y + 11.5z =-  12.5;";

    [GlobalSetup]
    public void Setup()
    {
        _list = Enumerable.Range(0, Count).ToList();
    }

    [Benchmark]
    public void TestSolverSpeed()
    {
        var result = new Solver(this.equationInput);
    }
}