using BenchmarkDotNet.Running;
using MrRabbit.ReliableEvents.Benchmarks.Benchmarks;
BenchmarkRunner.Run<OutboxStoreAttachEventsWithoutHandlerBenchmark>();
BenchmarkRunner.Run<OutboxStoreAttachEventsBenchmark>();
//BenchmarkRunner.Run<OutboxStoreDispatchBenchmark>();