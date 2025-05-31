using System;

namespace LMSTT.ViewModels.Quiz
{
    public class StudentQuizViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public int QuestionsNumber { get; set; }
        public string Status { get; set; }  // Published or Not Published
        public int TimeLimit { get; set; }
        public bool IsCompleted { get; set; }  // To determine if quiz is completed
        public int Score { get; set; }  // Student's score for this quiz
        public string ActionButtonText => IsCompleted ? "View Questions" : "Take Quiz";
        public string ActionUrl => IsCompleted ? 
            $"/Quiz/ViewQuestions/{Id}" : 
            $"/Quiz/TakeQuiz/{Id}";
    }
} 