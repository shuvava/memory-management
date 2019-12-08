using System;

using BenchmarkDotNet.Running;


namespace sequential_memory_access
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MemoryAccessBenchmarks>();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
