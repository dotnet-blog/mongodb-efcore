using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Versioning;
using static MassTransit.ValidationResultExtensions;
using System.IO;
using System.Text.Json;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static void UseExceptionHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;
                var result = new ErrorModel();
                //context.Response.ContentType = "application/json";

                //if (exception is ApplicationAuthenticationException)
                //{
                //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //}

                var stream = context.Response.Body;

                await JsonSerializer.SerializeAsync(stream, result, new JsonSerializerOptions
                {
                    WriteIndented = true
                }).ConfigureAwait(false);
            }));
        }
    }
}
