using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Models
{
    public class Movie
    {
        [Required]
        public required Guid _id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Rated { get; set; }

        [Required]
        public required string Synopsis { get; set; }
    }
}
