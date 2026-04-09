using BenchmarkDotNet.Running;
using MrRabbit.ReliableEvents.Benchmarks.Benchmarks;

BenchmarkRunner.Run<OutboxStoreAttachEventsBenchmark>();