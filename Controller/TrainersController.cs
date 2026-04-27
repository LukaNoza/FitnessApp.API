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
    public class TrainersController : ControllerBase
    {
        private readonly FitnessDbContext _context;

        public TrainersController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: api/trainers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.TrainerGyms)
                    .ThenInclude(tg => tg.Gym)
                .Select(t => new
                {
                    t.Id,
                    t.UserId,
                    Name = t.User!.FirstName + " " + t.User.LastName,
                    t.Specialization,
                    t.ExperienceYears,
                    t.HourlyRate,
                    t.Rating,
                    t.IsVerified,
                    Gyms = t.TrainerGyms!.Select(tg => new
                    {
                        tg.Gym!.Id,
                        tg.Gym.Name,
                        tg.Gym.City
                    }).ToList(),
                    WorkDays = t.TrainerGyms!.Select(tg => tg.WorkDays).FirstOrDefault(),
                    WorkStart = t.TrainerGyms!.Select(tg => tg.WorkStart).FirstOrDefault(),
                    WorkEnd = t.TrainerGyms!.Select(tg => tg.WorkEnd).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/trainers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.TrainerGyms)
                    .ThenInclude(tg => tg.Gym)
                .Include(t => t.TrainerWorkoutTypes)
                    .ThenInclude(twt => twt.WorkoutType)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.UserId,
                    Name = t.User!.FirstName + " " + t.User.LastName,
                    t.User!.Email,
                    t.User!.PhoneNumber,
                    t.Bio,
                    t.Specialization,
                    t.ExperienceYears,
                    t.HourlyRate,
                    t.Rating,
                    t.IsVerified,
                    Gyms = t.TrainerGyms!.Select(tg => new
                    {
                        tg.Gym!.Id,
                        tg.Gym.Name,
                        tg.Gym.City,
                        tg.Gym.Address,
                        tg.WorkDays,
                        tg.WorkStart,
                        tg.WorkEnd
                    }).ToList(),
                    WorkoutTypes = t.TrainerWorkoutTypes!.Select(twt => new
                    {
                        twt.WorkoutType!.Id,
                        twt.WorkoutType.Name,
                        twt.WorkoutType.Category,
                        twt.WorkoutType.Duration,
                        twt.PriceModifier,
                        FinalPrice = t.HourlyRate * twt.PriceModifier
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound(new { message = $"Trainer with id {id} not found" });
            }

            return Ok(trainer);
        }

        // GET: api/trainers/specialization/crossfit
        [HttpGet("specialization/{specialization}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainersBySpecialization(string specialization)
        {
            var trainers = await _context.Trainers
                .Include(t => t.User)
                .Where(t => t.Specialization.ToLower() == specialization.ToLower())
                .Select(t => new
                {
                    t.Id,
                    Name = t.User!.FirstName + " " + t.User.LastName,
                    t.Specialization,
                    t.ExperienceYears,
                    t.HourlyRate,
                    t.Rating
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // POST: api/trainers
        [HttpPost]
        public async Task<ActionResult<Trainer>> CreateTrainer(Trainer trainer)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(trainer.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Check if user is already a trainer
            if (await _context.Trainers.AnyAsync(t => t.UserId == trainer.UserId))
            {
                return BadRequest(new { message = "User is already a trainer" });
            }

            // Update user type
            user.UserType = "Trainer";

            trainer.CreatedAt = DateTime.UtcNow;
            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrainer), new { id = trainer.Id }, trainer);
        }

        // GET: api/trainers/5/availability?date=2026-03-20
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerAvailability(int id, DateTime date)
        {
            // First, find trainer's gym and schedule
            var trainerGym = await _context.TrainerGyms
                .FirstOrDefaultAsync(tg => tg.TrainerId == id);

            if (trainerGym == null)
            {
                return Ok(new List<object>()); // No schedule
            }

            // Get all available slots for this trainer
            var availableSlots = await _context.Schedules
                .Where(s => s.TrainerGymId == trainerGym.Id &&
                           s.StartDateTime.Date == date.Date &&
                           !s.IsBooked &&
                           s.CurrentBookings < s.MaxClients &&
                           s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.StartDateTime,
                    s.EndDateTime,
                    s.MaxClients,
                    AvailableSpots = s.MaxClients - s.CurrentBookings
                })
                .OrderBy(s => s.StartDateTime)
                .ToListAsync();

            return Ok(availableSlots);
        }
    }
}