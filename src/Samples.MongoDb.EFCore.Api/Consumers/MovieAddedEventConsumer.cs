using MassTransit;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api.Events;
using Samples.MongoDb.EFCore.Api.Services;

namespace Samples.MongoDb.EFCore.Api.Consumers
{
    public class MovieAddedEventConsumer : IConsumer<MovieAddedEvent>
    {
        readonly IMovieInfoService _movieInfoService;
        readonly MediaLibraryDbContext _dbContext;
        public MovieAddedEventConsumer(
            IMovieInfoService movieInfoService,
           MediaLibraryDbContext dbContext)
        {
            _movieInfoService = movieInfoService;
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<MovieAddedEvent> context)
        {
            var movieAddModel = context.Message;
            var movie = await _dbContext.Movies.SingleOrDefaultAsync(m => m._id == movieAddModel.Id);
            if (movie != null && !string.IsNullOrWhiteSpace(movie.ImdbId))
            {
                var movieInfo = await _movieInfoService.GetMovieInfo(movie.ImdbId);
                if (movieInfo != null)
                {
                    movie.Rating = int.Parse(movieInfo.imdbRating);
                    movie.Synopsis = movie.Synopsis == null ? movieInfo.Plot : movie.Synopsis;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
