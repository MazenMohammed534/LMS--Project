using System;

namespace LMSTT.ViewModels.Quiz
{
    public class QuizResultViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public DateTime DueDate { get; set; }
        public int QuestionsNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Score { get; set; }
    }
} 