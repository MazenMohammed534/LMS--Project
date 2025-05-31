using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMSTT.Models
{
    public class Assignments
    {
        public int Id { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public int CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }

        // Navigation properties
        public Course Course { get; set; }
        public User Creator { get; set; }
        public virtual ICollection<AssignmentSubmissions> Submissions { get; set; } = new List<AssignmentSubmissions>();
    }
}
