namespace FitnessApp.API.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public Booking? Booking { get; set; }
    }
}