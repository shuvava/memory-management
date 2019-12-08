using System;
using System.Threading;

using BenchmarkDotNet.Attributes;


namespace false_sharing_between_threads
{
    public class ThreadBenchmark
    {
        int[] _sharedData;


        [Params(0, 16)] public int gap = 0;


        [Params(1, 16)] public int offset = 1;


        [GlobalSetup]
        public void Setup()
        {
            _sharedData = new int[Environment.ProcessorCount * offset + gap * offset];
        }


        public long DoFalseSharingTest(int threadsCount, int size =
            100_000_000)
        {
            var workers = new Thread[threadsCount];

            for (var i = 0; i < threadsCount; ++i)
            {
                workers[i] = new Thread(idx =>
                {
                    var index = (int) idx + gap;

                    for (var j = 0; j < size; ++j)
                    {
                        _sharedData[index * offset] = _sharedData[index * offset] + 1;
                    }
                });
            }

            for (var i = 0; i < threadsCount; ++i)
            {
                workers[i].Start(i);
            }

            for (var i = 0; i < threadsCount; ++i)
            {
                workers[i].Join();
            }

            return 0;
        }


        [Benchmark]
        public long Test() => DoFalseSharingTest(Environment.ProcessorCount);
    }
}
