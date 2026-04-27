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
    public class ScheduleController : ControllerBase
    {
        private readonly FitnessDbContext _context;

        public ScheduleController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: api/schedule/available?trainerId=1&gymId=1&date=2026-03-25
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableSlots(
            int trainerId, int gymId, DateTime date)
        {
            // 1. მიიღე ტრენერის ინფორმაცია
            var trainer = await _context.Trainers
                .Include(t => t.TrainerGyms)
                .FirstOrDefaultAsync(t => t.Id == trainerId);

            if (trainer == null)
            {
                return BadRequest(new { message = "Trainer not found" });
            }

            // 2. მიიღე ტრენერ-დარბაზის კავშირი
            var trainerGym = trainer.TrainerGyms?.FirstOrDefault(tg => tg.GymId == gymId);
            if (trainerGym == null)
            {
                return Ok(new List<object>());
            }

            // 3. სამუშაო საათები
            var startHour = trainerGym.WorkStart.Hours;
            var endHour = trainerGym.WorkEnd.Hours;
            var maxClients = 3; // მაქსიმუმი კლიენტების სლოტზე

            // 4. მიიღე უკვე დაჯავშნილი სლოტები
            var bookedSlots = await _context.Bookings
                .Where(b => b.TrainerId == trainerId &&
                            b.GymId == gymId &&
                            b.BookingDate == date)
                .ToListAsync();

            var slots = new List<object>();

            // 5. შექმენი სლოტები და გამოთვალე ხელმისაწვდომი ადგილები
            for (int hour = startHour; hour < endHour; hour++)
            {
                var startTime = new TimeSpan(hour, 0, 0);
                var endTime = new TimeSpan(hour + 1, 0, 0);

                // დათვალე რამდენი ჯავშანია ამ სლოტზე
                var currentBookings = bookedSlots.Count(b =>
                    b.StartTime < endTime && b.EndTime > startTime);

                var availableSpots = maxClients - currentBookings;

                if (availableSpots > 0)
                {
                    slots.Add(new
                    {
                        Id = hour,
                        StartTime = startTime.ToString(@"hh\:mm"),
                        EndTime = endTime.ToString(@"hh\:mm"),
                        AvailableSpots = availableSpots
                    });
                }
            }

            return Ok(slots);
        }
    }
}