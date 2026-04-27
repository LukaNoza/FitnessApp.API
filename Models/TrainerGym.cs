using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class TrainerGym
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int GymId { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkDays { get; set; } = string.Empty; // "Mon,Wed,Fri"

        [Required]
        public TimeSpan WorkStart { get; set; }

        [Required]
        public TimeSpan WorkEnd { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Trainer? Trainer { get; set; }
        public Gym? Gym { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
    }
}