using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMSTT.ViewModels.Teacher
{
    public class UpdateCourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [MaxLength(250, ErrorMessage = "Course title cannot exceed 250 characters")]
        [Display(Name = "Course Name")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic Year is required")]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public SelectList? AcademicYears { get; set; }
        public SelectList? Departments { get; set; }
    }
} 