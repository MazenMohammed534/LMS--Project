using System;
using System.Collections.Generic;

namespace LMSTT.Models
{
    public class Quizzes
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int QuestionsNumbers { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public int TimeLimit { get; set; }

        // Navigation properties
        public Course Course { get; set; }
        public User Creator { get; set; }
        public ICollection<Questions> Questions { get; set; }
    }
}
