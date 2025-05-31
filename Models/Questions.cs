using System;

namespace LMSTT.Models
{
    public class Questions
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionOptions { get; set; }
        public string CorrectAnswer { get; set; }
        public int Points { get; set; } = 1;

        // Navigation property
        public Quizzes Quiz { get; set; }
    }
}
