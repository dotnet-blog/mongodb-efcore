using MassTransit;
using Samples.MongoDb.EFCore.Api.Events;

namespace Samples.MongoDb.EFCore.Api.Consumers
{
    public class MovieAddedEventConsumer : IConsumer<MovieAddedEvent>
    {
        public async Task Consume(ConsumeContext<MovieAddedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
