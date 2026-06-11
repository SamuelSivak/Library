using Library.Exceptions;

namespace Library.Logging
{
    
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        
        public async Task InvokeAsync(HttpContext context, ILoggerService loggerService)
        {
            try
            {
        
                await _next(context);
            }
            catch (Exception ex)
            {
               
                await HandleExceptionAsync(context, ex, loggerService);
            }
        }

        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILoggerService loggerService)
        {
          
            await loggerService.LogErrorAsync($"Unhandled exception: {context.Request.Path}", exception);

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
