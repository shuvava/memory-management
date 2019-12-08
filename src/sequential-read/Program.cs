using System;

using BenchmarkDotNet.Running;


namespace sequential_read
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SequentialReadBenchmark>();
        }
    }
}
