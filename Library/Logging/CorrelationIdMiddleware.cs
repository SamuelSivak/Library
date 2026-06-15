using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Library.Logging
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request already contains a Correlation ID
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Add the Correlation ID to the response headers
            context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);

            // Push CorrelationId property into Serilog's LogContext.
            // Using statement ensures it is automatically removed from context when request processing completes.
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
