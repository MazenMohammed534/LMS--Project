using System;
using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Assignment
{
    public class AssignmentViewModel
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
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }
        
        // Additional display properties
        public string CourseName { get; set; }
        public string TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 