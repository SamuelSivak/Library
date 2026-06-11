using System.IO;
using System.Threading.Tasks;

namespace Library.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<(Stream fileStream, string contentType)> DownloadFileAsync(string fileId);
        Task<bool> DeleteFileAsync(string fileId);
        Task<bool> FileExistsAsync(string fileId);
    }
}
