using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApp.API.Data;
using FitnessApp.API.Models;
using FitnessApp.API.Services;

namespace FitnessApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly FitnessDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(FitnessDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Create user
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Password = registerDto.Password,
                UserType = registerDto.UserType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ==========================================
            // თუ ტრენერია, შექმენი Trainer ჩანაწერიც
            // ==========================================
            if (user.UserType == "Trainer")
            {
                var trainer = new Trainer
                {
                    UserId = user.Id,
                    Bio = null,
                    Specialization = "General",
                    ExperienceYears = 0,
                    HourlyRate = 0,
                    Rating = 0,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
            }

            // Create token
            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserType = user.UserType,
                Token = token
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify password
            if (user.Password != loginDto.Password)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is disabled" });
            }

            // Create token
            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserType = user.UserType,
                Token = token
            });
        }
    }
}