using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Quiz
{
    public class TeacherQuizViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CourseName { get; set; }
        public DateTime DueDate { get; set; }
        public int QuestionsNumber { get; set; }
        public string Status { get; set; }
        public int TimeLimit { get; set; }
    }
} 