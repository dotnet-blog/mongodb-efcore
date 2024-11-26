using AutoMapper;
using Samples.MongoDb.EFCore.Api.Dtos;
using Samples.MongoDb.EFCore.Api.Models;

namespace Samples.MongoDb.EFCore.Api.Mappings
{
    public class MovieMappings : Profile
    {
        public MovieMappings()
        {
            CreateMap<MovieAddModel, Movie>()
                .ForMember(d => d._id, m => m.MapFrom(Guid.NewGuid().ToString()));
        }
    }
}
