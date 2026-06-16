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
            
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            
            context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);

            
            
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
