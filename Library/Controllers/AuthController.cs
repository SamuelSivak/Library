using Library.DTOs;
using Library.Models;
using Library.Repositories;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthController(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if (await _userRepository.UsernameExistsAsync(registerDto.Username))
            {
                return BadRequest("Username is already taken.");
            }

            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                return BadRequest("Email is already registered.");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
                Role = "User"
            };

            await _userRepository.CreateAsync(user);

            return new UserDTO
            {
                Username = user.Username,
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                Role = user.Role
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Ivalid username or password.");
            }

            var result = PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash);

            if (!result)
            {
                return Unauthorized("invalid username or password.");
            }

            return new UserDTO
            {
                Username = user.Username,
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                Role = user.Role
            };
        }
    }
}
