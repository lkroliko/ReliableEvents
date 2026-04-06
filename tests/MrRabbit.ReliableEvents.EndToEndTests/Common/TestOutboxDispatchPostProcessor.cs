namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestOutboxDispatchPostProcessor : IOutboxDispatchPostProcessor
    {
        public virtual Task ProcessAsync(PostOutboxDispatchContext context) => Task.CompletedTask;
    }