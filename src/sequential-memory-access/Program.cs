using BenchmarkDotNet.Running;


namespace sequential_memory_access
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MemoryAccessBenchmarks>();
        }
    }
}
