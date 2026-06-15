namespace Library.Services
{
    public interface IBookAnalyticsService
    {
        Task UpdateAnalyticsForBookAsync(int bookId);
        Task IncrementViewsAsync(int bookId);
    }
}
