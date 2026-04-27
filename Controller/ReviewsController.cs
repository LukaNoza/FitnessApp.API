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
    public class ReviewsController : ControllerBase
    {
        private readonly FitnessDbContext _context;

        public ReviewsController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews/trainer/5
        [HttpGet("trainer/{trainerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetReviewsByTrainer(int trainerId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TrainerId == trainerId)
                .Include(r => r.Client)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.BookingId,
                    r.ClientId,
                    r.TrainerId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    ClientName = r.Client != null ? r.Client.FirstName + " " + r.Client.LastName : "Unknown"
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<object>> GetReviewByBooking(int bookingId)
        {
            var review = await _context.Reviews
                .Where(r => r.BookingId == bookingId)
                .Select(r => new
                {
                    r.Id,
                    r.BookingId,
                    r.ClientId,
                    r.TrainerId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (review == null)
                return NotFound(new { message = "No review found for this booking" });

            return Ok(review);
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<object>> CreateReview(Review review)
        {
            try
            {
                Console.WriteLine("=== CREATE REVIEW START ===");
                Console.WriteLine($"BookingId: {review.BookingId}");
                Console.WriteLine($"Rating: {review.Rating}");
                Console.WriteLine($"Comment: {review.Comment}");

                // 1. Check if booking exists
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.Id == review.BookingId);

                if (booking == null)
                {
                    Console.WriteLine("Booking not found!");
                    return BadRequest(new { message = "Booking not found" });
                }

                Console.WriteLine($"Booking found - Status: {booking.Status}, ClientId: {booking.ClientId}, TrainerId: {booking.TrainerId}");

                // 2. Check if booking is completed
                if (booking.Status != "Completed")
                {
                    Console.WriteLine("Booking is not completed!");
                    return BadRequest(new { message = "You can only review completed sessions" });
                }

                // 3. Check if review already exists
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.BookingId == review.BookingId);

                if (existingReview != null)
                {
                    Console.WriteLine("Review already exists!");
                    return BadRequest(new { message = "Review already exists for this booking" });
                }

                // 4. Set review properties
                review.ClientId = booking.ClientId;
                review.TrainerId = booking.TrainerId;
                review.CreatedAt = DateTime.UtcNow;

                Console.WriteLine($"Saving review - ClientId: {review.ClientId}, TrainerId: {review.TrainerId}, Rating: {review.Rating}");

                // 5. Save review
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // 6. Update trainer rating
                var trainer = await _context.Trainers.FindAsync(review.TrainerId);
                if (trainer != null)
                {
                    var allReviews = await _context.Reviews
                        .Where(r => r.TrainerId == review.TrainerId)
                        .ToListAsync();

                    if (allReviews.Any())
                    {
                        var avg = allReviews.Average(r => (decimal)r.Rating);
                        trainer.Rating = Math.Round(avg, 2);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Trainer rating updated to: {trainer.Rating}");
                    }
                }

                Console.WriteLine("=== CREATE REVIEW SUCCESS ===");
                return Ok(new { message = "Review submitted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // DELETE: api/reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(new { message = "Review not found" });

            var trainerId = review.TrainerId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update trainer's average rating
            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer != null)
            {
                var allReviews = await _context.Reviews
                    .Where(r => r.TrainerId == trainerId)
                    .ToListAsync();

                if (allReviews.Any())
                {
                    var averageRating = allReviews.Average(r => (decimal)r.Rating);
                    trainer.Rating = Math.Round(averageRating, 2);
                }
                else
                {
                    trainer.Rating = 0;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Review deleted successfully" });
        }
    }
}