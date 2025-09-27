using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ShortenedUrl
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        [StringLength(50)]
        public string? Purpose { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool IsValid => !IsExpired && !IsUsed;
    }
}