using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Student
{
    public class JoinCourseViewModel
    {
        [Required(ErrorMessage = "Course code is required")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;
    }
} 