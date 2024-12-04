using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Samples.MongoDb.EFCore.Api.Middlewares
{
    public class CorrelationIdHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private string _headerKey;

        public CorrelationIdHeaderMiddleware(RequestDelegate next, string headerKey)
        {
            _next = next;
            _headerKey = headerKey;
        }
        public async Task Invoke(HttpContext httpContext, IAuthorizationService authorizationService)
        {
            if (!httpContext.Request.Headers.ContainsKey(_headerKey))
            {
                var correlationId = Guid.NewGuid().ToString("D");
                httpContext.Request.Headers.Add(_headerKey, correlationId);
            }

            await _next(httpContext);
        }
    }
}
