namespace Library.Logging
{
    
    public interface ILoggerService
    {
        
        Task LogAsync(string message, string level = "Info", string? exception = null, string? stackTrace = null);

        
        Task LogErrorAsync(string message, Exception ex);

        
        Task LogWarningAsync(string message);

        
        Task LogInfoAsync(string message);
    }
}
