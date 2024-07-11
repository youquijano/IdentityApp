using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]        
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; }

        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime DateExpireUtc { get; set; }
        public bool IsExpire => DateTime.UtcNow >= DateExpireUtc;

        [ForeignKey("UserId")]
        public User User { get; set; }



    }
}
