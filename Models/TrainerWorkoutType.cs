using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class TrainerWorkoutType
    {
        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int WorkoutTypeId { get; set; }

        public decimal PriceModifier { get; set; } = 1.0m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Trainer? Trainer { get; set; }
        public WorkoutType? WorkoutType { get; set; }
    }
}