using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Models
{
    public class Movie
    {
        [Required]
        public required long _id { get; set; }

        [Required]
        public required string Title { get; set; }

        public string? ImdbId { get; set; }

        public double? Rating { get; set; }

        public string? Synopsis { get; set; }

        [Required]
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeModified { get; set; }
    }
}
