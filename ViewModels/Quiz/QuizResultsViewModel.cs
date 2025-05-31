using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Quiz
{
    public class QuizResultsViewModel
    {
        public string CourseTitle { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int TimeLimit { get; set; }
        public int QuestionsNumber { get; set; }
        public List<StudentQuizResultViewModel> StudentResults { get; set; } = new List<StudentQuizResultViewModel>();
    }

    public class StudentQuizResultViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime? SubmittedAt { get; set; }
        
        [Display(Name = "Completion Status")]
        public string CompletionStatus => Status == "Completed" ? "Completed" : "In Progress";
        
        [Display(Name = "Score")]
        public string DisplayScore => Status == "Completed" ? Score.ToString() : "N/A";
    }
} 