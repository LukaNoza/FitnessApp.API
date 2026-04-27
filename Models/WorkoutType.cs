using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class WorkoutType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Range(1, 5)]
        public int DifficultyLevel { get; set; } = 1;

        [Required]
        public int Duration { get; set; } // წუთებში

        public int? CaloriesBurn { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<TrainerWorkoutType>? TrainerWorkoutTypes { get; set; }
    }
}