using System.ComponentModel.DataAnnotations;
using LMSTT.Settings;
using LMSTT.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMSTT.ViewModels.Teacher
{
    public class AddCourseViewModel
    {
        [Required(ErrorMessage = "Course name is required")]
        [MaxLength(250, ErrorMessage = "Course title cannot exceed 250 characters")]
        [Display(Name = "Course Name")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(10, ErrorMessage = "Course code cannot exceed 10 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic Year is required")]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        public IEnumerable<SelectListItem> AcademicYears { get; set; } = Enumerable.Empty<SelectListItem>();

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; } = Enumerable.Empty<SelectListItem>();

        [Display(Name = "Course Cover")]
        [AllowedExtentions(FileSettings.AllowedExtensions)]
        [MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile? cover { get; set; }
    }
}
