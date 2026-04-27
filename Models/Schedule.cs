using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainerGymId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public bool IsBooked { get; set; } = false;

        public int MaxClients { get; set; } = 1;

        public int CurrentBookings { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public TrainerGym? TrainerGym { get; set; }
    }
}