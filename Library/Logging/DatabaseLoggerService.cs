using Library.DataContext;

namespace Library.Logging
{
  
    public class DatabaseLoggerService : ILoggerService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseLoggerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        
        public async Task LogAsync(string message, string level = "Info", string? exception = null, string? stackTrace = null)
        {
            try
            { 
                // pls remember => DbContext SCOPED middleware SINGLETON
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

                    var log = new Log
                    {
                        Message = message,
                        Level = level,
                        Exception = exception,
                        StackTrace = stackTrace,
                        Timestamp = DateTime.UtcNow
                    };

                    context.Logs.Add(log);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Fallback - shit hit the vent
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] failed to log: {message}");
                Console.WriteLine($"eror: {ex.Message}");
            }
        }

        
        public async Task LogErrorAsync(string message, Exception ex)
        {
            await LogAsync(message, "Error", ex.Message, ex.StackTrace);
        }

        
        public async Task LogWarningAsync(string message)
        {
            await LogAsync(message, "Warning");
        }

        
        public async Task LogInfoAsync(string message)
        {
            await LogAsync(message, "Info");
        }
    }
}
