using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Samples.MongoDb.EFCore.Api.Dtos;
using Serilog.Events;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static string GetLogCorrelationId(this HttpContext context)
        {
            string correlationId = string.Empty;
            var correlationIdProperty = context.Items.SingleOrDefault(i => i.Key.ToString() == "Serilog_CorrelationId").Value as LogEventProperty;
            if (correlationIdProperty != null)
            {
                var correlationIdPropertyValue = ((ScalarValue)correlationIdProperty.Value)?.Value as String;
                if (!string.IsNullOrWhiteSpace(correlationIdPropertyValue))
                    correlationId = correlationIdPropertyValue;
            }
            return correlationId;
        }

        public static void UseExceptionHandling(
            this WebApplication app)
        {
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;
                string correlationId = context.GetLogCorrelationId();
                var result = new ErrorModel()
                {
                    CorrelationId = correlationId,
                    Error = app.Environment.IsProduction() ? $"An error occurred, please contact support with reference number {correlationId}" : exception.Message,
                    Trace = app.Environment.IsProduction() ? null : exception.StackTrace
                };

                context.Response.ContentType = "application/json";

                if (exception is FluentValidation.ValidationException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    result.Error = exception.Message;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                var stream = context.Response.Body;
                await JsonSerializer.SerializeAsync(stream, result, new JsonSerializerOptions
                {
                    WriteIndented = true
                }).ConfigureAwait(false);
            }));
        }
    }
}
