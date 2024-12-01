using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Dtos
{
    public class MovieAddModel
    {
        [Required]
        public required string Title { get; set; }

        public string? ImdbId { get; set; }

        [Required]
        public required string Synopsis { get; set; }
    }
}
