using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSTT.Models
{
    public class DiscussionMessage
    {
        public int Id { get; set; }
        
        [Required]
        public int DiscussionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string MessageText { get; set; }
        
        public DateTime SentAt { get; set; }

        // Navigation properties
        [ForeignKey("DiscussionId")]
        public Discussion Discussion { get; set; }
        
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User User { get; set; }
    }
} 