# prerequisite

install `dotnet tool install -g BenchmarkDotNet.Tool`

[more info](https://benchmarkdotnet.org/articles/guides/how-to-run.html)

# Run dotnet benchmark

1. compile release version of application
1. goto binary folder
1. run `dotnet benchmark MyAssemblyWithBenchmarks.dll --filter *`
