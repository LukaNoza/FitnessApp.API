using FitnessApp.API.Data;
using FitnessApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace FitnessApp.API.Controllers
{
    /// <summary>
    /// Bookings Controller - ჯავშნების მართვა
    /// URL: /api/bookings
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        // DbContext - მონაცემთა ბაზასთან კავშირი
        private readonly FitnessDbContext _context;

        // კონსტრუქტორი - Dependency Injection-ით ვიღებთ DbContext-ს
        public BookingsController(FitnessDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: api/bookings
        // ყველა ჯავშნის მიღება (ადმინისთვის)
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            // Include() - მოაქვს დაკავშირებული მონაცემები (Eager Loading)
            // ThenInclude() - მოაქვს დაკავშირებულის დაკავშირებული მონაცემები
            var bookings = await _context.Bookings
                .Include(b => b.Client)                    // Client-ის მონაცემები
                .Include(b => b.Trainer)                   // Trainer-ის მონაცემები
                    .ThenInclude(t => t.User)              // Trainer-ის User-ის მონაცემები (სახელი, გვარი)
                .Include(b => b.Gym)                       // Gym-ის მონაცემები
                .OrderByDescending(b => b.BookingDate)     // უახლესი თარიღი პირველად
                .Select(b => new                           // Anonymous Object - მხოლოდ საჭირო ველები
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    // Client-ის სრული სახელი, თუ არ არსებობს "Unknown"
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    // Trainer-ის სრული სახელი User-იდან
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .ToListAsync();  // ასინქრონულად აქცევს List-ად

            return Ok(bookings);  // 200 OK
        }

        // ==========================================
        // GET: api/bookings/5
        // ერთი კონკრეტული ჯავშნის მიღება ID-ით
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Trainer)
                    .ThenInclude(t => t.User)
                .Include(b => b.Gym)
                .Where(b => b.Id == id)                    // ფილტრაცია ID-ით
                .Select(b => new
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .FirstOrDefaultAsync();  // პირველი ან null

            // თუ ვერ მოიძებნა, დააბრუნე 404 Not Found
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with id {id} not found" });
            }

            return Ok(booking);  // 200 OK
        }

        // ==========================================
        // GET: api/bookings/client/3
        // კონკრეტული კლიენტის ყველა ჯავშანი
        // ==========================================
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookingsByClient(int clientId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Trainer)
                    .ThenInclude(t => t.User)
                .Include(b => b.Gym)
                .Where(b => b.ClientId == clientId)        // ფილტრაცია ClientId-ით
                .OrderByDescending(b => b.BookingDate)     // უახლესი პირველად
                .Select(b => new
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ==========================================
        // GET: api/bookings/trainer/1
        // კონკრეტული ტრენერის ყველა ჯავშანი
        // ==========================================
        [HttpGet("trainer/{trainerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookingsByTrainer(int trainerId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Gym)
                .Where(b => b.TrainerId == trainerId)      // ფილტრაცია TrainerId-ით
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ==========================================
        // GET: api/bookings/date/2026-03-26
        // კონკრეტულ თარიღზე ყველა ჯავშანი
        // ==========================================
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookingsByDate(DateTime date)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Trainer)
                    .ThenInclude(t => t.User)
                .Include(b => b.Gym)
                .Where(b => b.BookingDate == date)         // ფილტრაცია თარიღით
                .OrderBy(b => b.StartTime)                 // დროის მიხედვით ზრდადობით
                .Select(b => new
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ==========================================
        // GET: api/bookings/status/pending
        // კონკრეტული სტატუსის ყველა ჯავშანი
        // ==========================================
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookingsByStatus(string status)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Trainer)
                    .ThenInclude(t => t.User)
                .Include(b => b.Gym)
                .Where(b => b.Status.ToLower() == status.ToLower())  // Case-insensitive შედარება
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    b.Id,
                    b.ClientId,
                    b.TrainerId,
                    b.GymId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalAmount,
                    b.CreatedAt,
                    ClientName = b.Client != null ? b.Client.FirstName + " " + b.Client.LastName : "Unknown",
                    TrainerName = b.Trainer != null && b.Trainer.User != null
                        ? b.Trainer.User.FirstName + " " + b.Trainer.User.LastName
                        : "Unknown",
                    GymName = b.Gym != null ? b.Gym.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ==========================================
        // POST: api/bookings
        // ახალი ჯავშნის შექმნა
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<object>> CreateBooking(Booking booking)
        {
            // ==========================================
            // 1. ModelState-ის შემოწმება
            // ==========================================
            // ModelState ამოწმებს Data Annotations-ებს ([Required], [MaxLength] და ა.შ.)
            if (!ModelState.IsValid)
            {
                // თუ შეცდომებია, გადააკეთე სტრუქტურირებულ ფორმატში
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            // ==========================================
            // 2. Client-ის არსებობის შემოწმება
            // ==========================================
            var client = await _context.Users.FindAsync(booking.ClientId);
            if (client == null)
            {
                return BadRequest(new { message = "Client not found" });
            }

            // ⚠️ IMPORTANT: client.UserType შემოწმება წაშლილია!
            // ახლა ადმინს და ტრენერსაც შეუძლია კლიენტისთვის ჯავშნის შექმნა

            // ==========================================
            // 3. Trainer-ის არსებობის შემოწმება
            // ==========================================
            var trainer = await _context.Trainers.FindAsync(booking.TrainerId);
            if (trainer == null)
            {
                return BadRequest(new { message = "Trainer not found" });
            }

            // ==========================================
            // 4. Gym-ის არსებობის შემოწმება
            // ==========================================
            var gym = await _context.Gyms.FindAsync(booking.GymId);
            if (gym == null)
            {
                return BadRequest(new { message = "Gym not found" });
            }

            // ==========================================
            // 5. დროის ვალიდურობის შემოწმება
            // ==========================================
            // დასაწყისი უნდა იყოს დასრულებაზე ადრე
            if (booking.StartTime >= booking.EndTime)
            {
                return BadRequest(new { message = "Start time must be before end time" });
            }

            // ==========================================
            // 6. თარიღის ვალიდურობის შემოწმება
            // ==========================================
            // არ შეიძლება წარსულში ჯავშნის შექმნა
            if (booking.BookingDate < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Cannot book sessions in the past" });
            }

            // ==========================================
            // 7. გადაფარვის (overlap) შემოწმება
            // ==========================================
            // ვამოწმებთ არის თუ არა უკვე არსებული ჯავშანი იმავე დროს
            var existingBooking = await _context.Bookings
                .Where(b => b.TrainerId == booking.TrainerId &&
                            b.GymId == booking.GymId &&
                            b.BookingDate == booking.BookingDate &&
                            b.StartTime < booking.EndTime &&      // ძველი იწყება ახალზე ადრე
                            b.EndTime > booking.StartTime)       // ძველი მთავრდება ახლის დაწყების შემდეგ
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                return BadRequest(new { message = "This time slot is already booked" });
            }

            // ==========================================
            // 8. ჯამური თანხის გამოთვლა
            // ==========================================
            // გამოვთვლით საათების რაოდენობას და ვამრავლებთ საათობრივ განაკვეთზე
            var hours = (booking.EndTime - booking.StartTime).TotalHours;
            booking.TotalAmount = trainer.HourlyRate * (decimal)hours;

            // ==========================================
            // 9. შექმნის თარიღის დაყენება და შენახვა
            // ==========================================
            booking.CreatedAt = DateTime.UtcNow;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();  // აქ ხდება მონაცემთა ბაზაში ჩაწერა

            // ==========================================
            // 10. შედეგის დაბრუნება (DTO - Data Transfer Object)
            // ==========================================
            // ვაბრუნებთ Anonymous Object-ს რათა თავიდან ავიცილოთ circular references
            var result = new
            {
                booking.Id,
                booking.ClientId,
                booking.TrainerId,
                booking.GymId,
                booking.BookingDate,
                booking.StartTime,
                booking.EndTime,
                booking.Status,
                booking.TotalAmount,
                booking.CreatedAt,
                ClientName = client.FirstName + " " + client.LastName,
                TrainerName = (await _context.Users.FindAsync(trainer.UserId))?.FirstName + " " +
                              (await _context.Users.FindAsync(trainer.UserId))?.LastName ?? "Unknown",
                GymName = gym.Name
            };

            // 201 Created - აბრუნებს შექმნილი რესურსის ლოკაციას
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, result);
        }

        // ==========================================
        // PUT: api/bookings/5
        // ჯავშნის სრულად განახლება
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, Booking booking)
        {
            // ID-ების შეუსაბამობის შემოწმება
            if (id != booking.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            // ვამოწმებთ არსებობს თუ არა ჯავშანი
            var existingBooking = await _context.Bookings.FindAsync(id);
            if (existingBooking == null)
            {
                return NotFound(new { message = $"Booking with id {id} not found" });
            }

            // ველების განახლება
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.StartTime = booking.StartTime;
            existingBooking.EndTime = booking.EndTime;
            existingBooking.Status = booking.Status;
            existingBooking.TotalAmount = booking.TotalAmount;

            // Entity State-ის მონიშვნა როგორც Modified
            _context.Entry(existingBooking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();  // ცვლილებების შენახვა
            }
            catch (DbUpdateConcurrencyException)  // კონკურენტული ცვლილების შემთხვევაში
            {
                if (!BookingExists(id))
                {
                    return NotFound();  // თუ ჯავშანი წაშლილია
                }
                throw;  // სხვა შემთხვევაში გადააგდე ექსეფშენი
            }

            return NoContent();  // 204 No Content - წარმატებით განახლდა
        }

        // PATCH: api/bookings/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Trainer)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new { message = $"Booking with id {id} not found" });
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled", "NoShow" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Allowed: Pending, Confirmed, Completed, Cancelled, NoShow" });
            }

            var oldStatus = booking.Status;

            // 👇 თუ სტატუსი არის "Cancelled", წაშალე ჯავშანი
            if (status == "Cancelled")
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Booking cancelled and removed from database successfully",
                    deleted = true,
                    bookingId = id
                });
            }

            booking.Status = status;
            await _context.SaveChangesAsync();

            // 🔔 გაგზავნე Notification კლიენტისთვის (როცა ტრენერი ადასტურებს)
            if (status == "Confirmed" && oldStatus == "Pending")
            {
                // ვქმნით Notification-ს Frontend-ისთვის (Backend-დან)
                // ეს უნდა შეინახოს Notification-ების ცხრილში
                var notification = new FitnessApp.API.Models.Notification
                {
                    UserId = booking.ClientId,
                    Title = "✅ Booking Confirmed!",
                    Message = $"Your booking with {booking.Trainer?.User?.FirstName} {booking.Trainer?.User?.LastName} on {booking.BookingDate:yyyy-MM-dd} at {booking.StartTime} has been confirmed.",
                    BookingId = booking.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Booking status updated to {status}",
                deleted = false,
                bookingId = id,
                status = status
            });
        }

        // ==========================================
        // DELETE: api/bookings/5
        // ჯავშნის წაშლა
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with id {id} not found" });
            }

            _context.Bookings.Remove(booking);  // ჯავშნის წაშლა
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking deleted successfully" });
        }

        // ==========================================
        // Private Helper Method
        // ამოწმებს არსებობს თუ არა ჯავშანი მოცემული ID-ით
        // ==========================================
        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}



//📊 HTTP Methods და Status Codes
//Method	Endpoint	რას აკეთებს	წარმატების Status
//GET	/api/bookings	ყველა ჯავშანი	200 OK
//GET	/api/bookings/5	ერთი ჯავშანი ID-ით	200 OK / 404 Not Found
//GET	/api/bookings/client/3	კლიენტის ჯავშნები	200 OK
//GET	/api/bookings/trainer/1	ტრენერის ჯავშნები	200 OK
//GET	/api/bookings/date/2026-03-26	თარიღის ჯავშნები	200 OK
//GET	/api/bookings/status/pending	სტატუსის ჯავშნები	200 OK
//POST	/api/bookings	ახალი ჯავშანი	201 Created / 400 Bad Request
//PUT	/api/bookings/5	სრული განახლება	204 No Content / 404 Not Found
//PATCH	/api/bookings/5/status	სტატუსის განახლება	200 OK / 400 Bad Request
//DELETE	/api/bookings/5	წაშლა	200 OK / 404 Not Found
//🔑 მნიშვნელოვანი ცნებები
//ტერმინი	ახსნა
//[HttpGet] აღნიშნავს რომ ეს მეთოდი პასუხობს GET რექვესტებს
//[HttpPost] აღნიშნავს რომ ეს მეთოდი პასუხობს POST რექვესტებს
//async Task<ActionResult>	ასინქრონული მეთოდი, რომელიც აბრუნებს HTTP პასუხს
//Include()	Eager Loading - დაკავშირებული მონაცემების ჩატვირთვა
//Select()	Projection - მხოლოდ საჭირო ველების არჩევა
//CreatedAtAction()	აბრუნებს 201 Created + Location header-ს
//NoContent()	აბრუნებს 204 No Content (წარმატებული განახლება)
//NotFound()	აბრუნებს 404 Not Found
//BadRequest()	აბრუნებს 400 Bad Request