using System.ComponentModel.DataAnnotations;

namespace Samples.MongoDb.EFCore.Api.Dtos
{
    public class ErrorModel
    {
        [Required]
        public required string CorrelationId { get; set; }

        [Required]
        public required string Error { get; set; }
        public string? Trace { get; set; }

        /// <summary>
        /// Additional messages
        /// </summary>
        public IEnumerable<ErrorMessageModel> Messages { get; set; }
    }

    /// <summary>
    /// Inner error message model
    /// </summary>
    public class ErrorMessageModel
    {
        /// <summary>
        /// Field name
        /// </summary>
        public String Field { get; }

        /// <summary>
        /// Value
        /// </summary>
        public String Value { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public ErrorMessageModel(string field, string value)
        {
            Field = field;
            Value = value;
        }
    }
}
