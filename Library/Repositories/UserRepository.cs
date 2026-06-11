using Library.DataContext;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Library.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LibraryContext _context;

        public UserRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var lower = usernameOrEmail.ToLower().Trim();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == lower || u.Email.ToLower() == lower);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var lower = username.ToLower().Trim();
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == lower);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var lower = email.ToLower().Trim();
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == lower);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
