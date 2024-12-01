using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Dtos
{
    public class MovieViewModel
    {
        [Required]
        public required string Id { get; set; }

        [Required]
        public required string Title { get; set; }

        public string? ImdbId { get; set; }

        [Required]
        public required double Rating { get; set; }

        [Required]
        public required string Synopsis { get; set; }
    }
}
