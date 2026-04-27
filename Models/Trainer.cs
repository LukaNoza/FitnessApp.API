using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class Trainer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(2000)]
        public string? Bio { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; }

        public decimal Rating { get; set; } = 0;

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; }
        public ICollection<TrainerGym>? TrainerGyms { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<TrainerWorkoutType>? TrainerWorkoutTypes { get; set; }
    }
}