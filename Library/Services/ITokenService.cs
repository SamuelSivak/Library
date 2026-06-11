using Library.Models;

namespace Library.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
