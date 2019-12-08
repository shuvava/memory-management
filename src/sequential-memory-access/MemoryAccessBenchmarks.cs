using BenchmarkDotNet.Attributes;


namespace sequential_memory_access
{
    public class MemoryAccessBenchmarks
    {
        void AccessByRows(int n, int m)
        {
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[i, j] = 1;
                }
            }
        }


        void AccessByColumns(int n, int m)
        {
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[j, i] = 1;
                }
            }
        }


        [Benchmark]
        public void ByRows() => AccessByRows(5000, 5000);


        [Benchmark]
        public void ByColumns() => AccessByColumns(5000, 5000);
    }
}
