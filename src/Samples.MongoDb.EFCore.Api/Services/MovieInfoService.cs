using Flurl;
namespace Samples.MongoDb.EFCore.Api.Services
{
    public class MovieInfoService : IMovieInfoService
    {
       readonly HttpClient _httpClient;
        public MovieInfoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<MovieInfo?> GetMovieInfo(string imdbId)
        {
            return await _httpClient.GetFromJsonAsync<MovieInfo?>(_httpClient.BaseAddress.AppendQueryParam("i", imdbId));
        }
    }
}
