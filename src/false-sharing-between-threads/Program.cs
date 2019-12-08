using System;

using BenchmarkDotNet.Running;


namespace false_sharing_between_threads
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ThreadBenchmark>();
        }
    }
}
