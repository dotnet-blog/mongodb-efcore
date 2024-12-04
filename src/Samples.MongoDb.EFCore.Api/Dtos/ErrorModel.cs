using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Dtos
{
    public class ErrorModel
    {
        [Required]
        public required string CorrelationId { get; set; }

        [Required]
        public required string Message { get; set; }
        public string? Trace { get; set; }
    }
}
