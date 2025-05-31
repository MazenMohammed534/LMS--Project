using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSTT.Models
{
    public class Discussion
    {
        public int Id { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [Required]
        public int CreatedById { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
        
        [ForeignKey("CreatedById")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User CreatedBy { get; set; }
        
        public ICollection<DiscussionMessage> Messages { get; set; }
    }
} 