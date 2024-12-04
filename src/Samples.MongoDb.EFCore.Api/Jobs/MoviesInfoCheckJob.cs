using Microsoft.EntityFrameworkCore;
using Quartz;
using Samples.MongoDb.EFCore.Api.Services;

namespace Samples.MongoDb.EFCore.Api.Jobs
{
    public class MoviesInfoCheckJob : IJob
    {
        readonly MediaLibraryDbContext _mediaLibraryDbContext;
        readonly IMovieInfoService _movieInfoService;
        public MoviesInfoCheckJob(
            MediaLibraryDbContext mediaLibraryDbContext,
            IMovieInfoService movieInfoService)
        {
            _mediaLibraryDbContext = mediaLibraryDbContext;
            _movieInfoService = movieInfoService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var movies = await _mediaLibraryDbContext.Movies.Where(m =>
            (m.Synopsis == null || m.Synopsis == string.Empty) && m.DateTimeModified == null).ToArrayAsync();
            foreach (var movie in movies)
            {
                if (movie.ImdbId != null && movie.ImdbId != string.Empty)
                {
                    var movieInfo = await _movieInfoService.GetMovieInfo(movie.ImdbId);
                    if (movieInfo != null)
                    {
                        movie.Rating = double.Parse(movieInfo.imdbRating);
                        movie.Synopsis = movieInfo.Plot;
                        movie.DateTimeModified = DateTime.UtcNow;
                        await _mediaLibraryDbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
