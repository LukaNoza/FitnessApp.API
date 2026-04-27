using FitnessApp.API.Data;
using FitnessApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GymsController : ControllerBase
    {
        private readonly FitnessDbContext _context;

        public GymsController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: api/gyms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gym>>> GetGyms()
        {
            return await _context.Gyms
                .Where(g => g.IsActive)
                .ToListAsync();
        }

        // GET: api/gyms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Gym>> GetGym(int id)
        {
            var gym = await _context.Gyms
                .Include(g => g.TrainerGyms)
                    .ThenInclude(tg => tg.Trainer)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gym == null)
            {
                return NotFound(new { message = $"Gym with id {id} not found" });
            }

            return Ok(gym);
        }

        // GET: api/gyms/city/tbilisi
        [HttpGet("city/{city}")]
        public async Task<ActionResult<IEnumerable<Gym>>> GetGymsByCity(string city)
        {
            var gyms = await _context.Gyms
                .Where(g => g.City.ToLower() == city.ToLower() && g.IsActive)
                .ToListAsync();

            return Ok(gyms);
        }

        // GET: api/gyms/5/trainers
        [HttpGet("{id}/trainers")]
        public async Task<ActionResult<IEnumerable<object>>> GetGymTrainers(int id)
        {
            var trainers = await _context.TrainerGyms
                .Where(tg => tg.GymId == id)
                .Include(tg => tg.Trainer)
                    .ThenInclude(t => t.User)
                .Select(tg => new
                {
                    tg.Trainer!.Id,
                    Name = tg.Trainer.User!.FirstName + " " + tg.Trainer.User.LastName,
                    tg.Trainer.Specialization,
                    tg.Trainer.ExperienceYears,
                    tg.Trainer.HourlyRate,
                    tg.Trainer.Rating,
                    tg.WorkDays,
                    tg.WorkStart,
                    tg.WorkEnd
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // POST: api/gyms
        [HttpPost]
        public async Task<ActionResult<Gym>> CreateGym(Gym gym)
        {
            gym.CreatedAt = DateTime.UtcNow;
            _context.Gyms.Add(gym);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGym), new { id = gym.Id }, gym);
        }

        // PUT: api/gyms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGym(int id, Gym gym)
        {
            if (id != gym.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existingGym = await _context.Gyms.FindAsync(id);
            if (existingGym == null)
            {
                return NotFound(new { message = $"Gym with id {id} not found" });
            }

            existingGym.Name = gym.Name;
            existingGym.Address = gym.Address;
            existingGym.City = gym.City;
            existingGym.PhoneNumber = gym.PhoneNumber;
            existingGym.OpenTime = gym.OpenTime;
            existingGym.CloseTime = gym.CloseTime;
            existingGym.IsActive = gym.IsActive;

            _context.Entry(existingGym).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/gyms/5 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGym(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym == null)
            {
                return NotFound(new { message = $"Gym with id {id} not found" });
            }

            gym.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gym deactivated successfully" });
        }
    }
}