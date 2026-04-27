using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int GymId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? Client { get; set; }
        public Trainer? Trainer { get; set; }
        public Gym? Gym { get; set; }
        public Review? Review { get; set; }
    }
}