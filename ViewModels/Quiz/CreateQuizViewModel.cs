using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Quiz
{
    public class CreateQuizViewModel
    {
        [Required(ErrorMessage = "Please select a course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Number of questions is required")]
        [Range(1, 100, ErrorMessage = "Number of questions must be between 1 and 100")]
        [Display(Name = "Questions Number")]
        public int QuestionsNumber { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Time limit is required")]
        [Display(Name = "Time Limit")]
        [DataType(DataType.Time)]
        public TimeSpan TimeLimit { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; }

        // For dropdown lists
        public SelectList CoursesList { get; set; }
        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Published", Text = "Published" },
            new SelectListItem { Value = "Not Published", Text = "Not Published" }
        };
    }
} 