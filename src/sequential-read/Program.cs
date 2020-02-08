using BenchmarkDotNet.Running;


namespace sequential_read
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SequentialReadBenchmark>();
        }
    }
}
