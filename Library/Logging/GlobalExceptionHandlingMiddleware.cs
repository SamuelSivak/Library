using Library.Exceptions;
using Microsoft.Extensions.Logging;

namespace Library.Logging
{
    
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
        
                await _next(context);
            }
            catch (Exception ex)
            {
               
                await HandleExceptionAsync(context, ex);
            }
        }

        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
          
            _logger.LogError(exception, "Unhandled exception: {Path}", context.Request.Path);

            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationEx:
                    
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Validation error",
                        Errors = validationEx.Errors
                    };
                    break;

                case KeyNotFoundException:
                  
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response = new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Resource not found"
                    };
                    break;

                default:
                    
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = "An unexpected error occurred",
                        Details = exception.Message
                    };
                    break;
            }
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
