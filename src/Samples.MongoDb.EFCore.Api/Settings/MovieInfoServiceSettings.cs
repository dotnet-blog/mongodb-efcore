namespace Samples.MongoDb.EFCore.Api.Settings
{
    /// <summary>
    /// Movie info service configuration section model
    /// </summary>
    public class MovieInfoServiceSettings
    {
        /// <summary>
        /// Movie info service url
        /// </summary>
        public required Uri Url { get; set; }

        /// <summary>
        /// Movie info service API key
        /// </summary>
        public required string ApiKey { get; set; }
    }
}
