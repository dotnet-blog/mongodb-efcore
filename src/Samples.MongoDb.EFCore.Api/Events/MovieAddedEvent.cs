namespace Samples.MongoDb.EFCore.Api.Events
{
    public class MovieAddedEvent
    {
        public long Id { get; set; }

        public MovieAddedEvent()
        {

        }

        public MovieAddedEvent(long id)
        {
            Id = id;
        }
    }
}
