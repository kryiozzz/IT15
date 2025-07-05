using System;
using System.ComponentModel.DataAnnotations;

namespace Opti.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }  // "Admin", "Worker", or "Customer"

        public DateTime CreatedAt { get; set; }

        // When the user last logged in
        public DateTime? LastLoginDate { get; set; }

        // When the user last logged out
        public DateTime? LastLogoutDate { get; set; }

        // If the user account is active
        public bool IsActive { get; set; } = true;
    }
}