using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Quiz
{
    public class AddQuestionViewModel
    {
        public int QuizId { get; set; }
        public int CurrentQuestionNumber { get; set; }
        public int TotalQuestions { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        [Display(Name = "Question")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Option 1 is required")]
        [Display(Name = "Option 1")]
        public string Option1 { get; set; }

        [Required(ErrorMessage = "Option 2 is required")]
        [Display(Name = "Option 2")]
        public string Option2 { get; set; }

        [Required(ErrorMessage = "Option 3 is required")]
        [Display(Name = "Option 3")]
        public string Option3 { get; set; }

        [Required(ErrorMessage = "Option 4 is required")]
        [Display(Name = "Option 4")]
        public string Option4 { get; set; }

        [Required(ErrorMessage = "Answer is required")]
        [Display(Name = "Answer")]
        public string Answer { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Points")]
        public int Points { get; set; } = 1;
    }
} 