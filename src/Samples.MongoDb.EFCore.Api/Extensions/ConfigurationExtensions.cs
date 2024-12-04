using System.Runtime.CompilerServices;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T? GetJobSchedule<T>(this IConfiguration configuration, String jobName)
        {
            return configuration.GetValue<T>($"{jobName}:Schedule");
        }
    }
}
