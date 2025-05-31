using System;

namespace LMSTT.Models
{
    public class QuizSubmissions
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int StudentId { get; set; }
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public string CompletionStatus { get; set; }

        // Navigation properties
        public Quizzes Quiz { get; set; }
        public User Student { get; set; }
        public Questions Question { get; set; }
    }
}
