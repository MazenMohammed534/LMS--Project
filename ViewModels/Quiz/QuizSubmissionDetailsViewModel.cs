using System;
using System.Collections.Generic;

namespace LMSTT.ViewModels.Quiz
{
    public class QuizSubmissionDetailsViewModel
    {
        public string QuizTitle { get; set; }
        public int TimeLimit { get; set; }
        public int TotalQuestions { get; set; }
        public int Score { get; set; }
        public int CourseId { get; set; }
        public List<QuestionSubmissionViewModel> Questions { get; set; }
    }

    public class QuestionSubmissionViewModel
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string StudentAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
} 