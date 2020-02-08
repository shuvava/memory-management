using System;

using BenchmarkDotNet.Attributes;


namespace sequential_read
{
    public class SequentialReadBenchmark
    {
        private OneLineStruct[] _arr;

        [Params(256, 512, 4096, 16384)] public int Size;


        [GlobalSetup]
        public void Setup()
        {
            _arr = new OneLineStruct[Size];
            var random = new Random(42);
            for(int j=0; j< Size; ++j)
            {
                _arr[j] = new OneLineStruct
                {
                    data1 = random.Next(1000)
                };
            }
        }
        public static long OneLineStructSequentialReadPattern(OneLineStruct[] tab)
        {
            long sum = 0;
            int n = tab.Length;

            for (int i = 0; i < n; ++i)
            {
                unchecked
                {
                    sum += tab[i].data1;
                }
            }

            return sum;
        }


        [Benchmark]
        public long SequentialReadPattern() => OneLineStructSequentialReadPattern(_arr);
    }
}
