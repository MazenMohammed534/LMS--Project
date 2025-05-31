using System.Collections.Generic;

namespace LMSTT.ViewModels.Quiz
{
    public class QuizQuestionsViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int TimeLimit { get; set; }
        public int TotalQuestions { get; set; }
        public int RemainingTime { get; set; }
        public List<QuizQuestionItem> Questions { get; set; }
    }

    public class QuizQuestionItem
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int QuestionNumber { get; set; }
    }
} 