using AutoMapper;
using Samples.MongoDb.EFCore.Api.Dtos.MediaLibrary;
using Samples.MongoDb.EFCore.Api.Models;

namespace Samples.MongoDb.EFCore.Api.Mappings
{
    public class MovieMappings : Profile
    {
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=net-9.0">
        /// Check Guid.ToString Method documentation for guid string formats
        /// </see>
        public MovieMappings()
        {
            CreateMap<MovieAddModel, Movie>()
                .ForMember(d => d._id, m => m.Ignore());

            CreateMap<Movie, MovieViewModel>()
                .ForMember(d => d.Id, m => m.MapFrom(s => s._id));
        }
    }
}
