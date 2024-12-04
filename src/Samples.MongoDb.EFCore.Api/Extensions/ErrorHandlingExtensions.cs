using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Versioning;
using static MassTransit.ValidationResultExtensions;
using System.IO;
using System.Text.Json;
using Serilog.Core;
using Samples.MongoDb.EFCore.Api.Dtos;
using FluentValidation;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static void UseExceptionHandling(
            this WebApplication app,
            string correlationKey)
        {
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;
                 
                // TODO: filter error message based on environment

                var result = new ErrorModel()
                {
                    CorrelationId = context.Request.Headers[correlationKey],
                    Message = exception.Message
                };
                context.Response.ContentType = "application/json";

                if (exception is FluentValidation.ValidationException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
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
