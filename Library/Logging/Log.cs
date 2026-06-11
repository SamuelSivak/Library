namespace Library.Logging
{
 
    /// Reprezentuje log záznam v databáze
    
    public class Log
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Info"; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
    }
}
