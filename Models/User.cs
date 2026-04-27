using System.ComponentModel.DataAnnotations;

namespace FitnessApp.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string UserType { get; set; } = "Client";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Trainer? Trainer { get; set; }
        public ICollection<Booking>? ClientBookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}