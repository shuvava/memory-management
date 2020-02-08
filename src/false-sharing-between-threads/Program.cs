using BenchmarkDotNet.Running;


namespace false_sharing_between_threads
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ThreadBenchmark>();
        }
    }
}
