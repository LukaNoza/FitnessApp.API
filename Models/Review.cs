using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Booking? Booking { get; set; }
        public User? Client { get; set; }
        public Trainer? Trainer { get; set; }
    }
}