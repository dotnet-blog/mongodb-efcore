using Flurl;
using Microsoft.Extensions.Caching.Distributed;
using Samples.MongoDb.EFCore.Api.Extensions;
namespace Samples.MongoDb.EFCore.Api.Services
{
    public class MovieInfoService : IMovieInfoService
    {
       readonly HttpClient _httpClient;
        readonly IDistributedCache _distributedCache;
        public MovieInfoService(
            HttpClient httpClient,
            IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _distributedCache = distributedCache;
        }
        public async Task<MovieInfo?> GetMovieInfo(string imdbId)
        {
            if(string.IsNullOrWhiteSpace(imdbId))
                return null;

            var movieInfo = await _distributedCache.GetCached<MovieInfo>(
                imdbId,
                () => _httpClient.GetFromJsonAsync<MovieInfo?>(_httpClient.BaseAddress.AppendQueryParam("i", imdbId)),
                TimeSpan.FromHours(1));

            return movieInfo;
        }
    }
}
