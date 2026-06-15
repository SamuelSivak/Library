using Library.Models;
using System.Threading.Tasks;

namespace Library.Services
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user);
    }
}
