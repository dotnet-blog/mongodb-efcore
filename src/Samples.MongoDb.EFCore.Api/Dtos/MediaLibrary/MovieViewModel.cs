using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Dtos.MediaLibrary
{
    public class MovieViewModel
    {
        [Required]
        public required string Id { get; set; }

        [Required]
        public required string Title { get; set; }

        public string? ImdbId { get; set; }

        public double? Rating { get; set; }

        public string? Synopsis { get; set; }
    }
}
