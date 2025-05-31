using LMSTT.Models;
using System;

namespace LMSTT.ViewModels.Teacher
{
    public class SubjectCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string CourseCover { get; set; } = string.Empty;
        
        // Statistics
        public int StudentCount { get; set; }
        public int MaterialCount { get; set; }
        public int QuizCount { get; set; }
        public int TaskCount { get; set; }

        // Navigation URLs
        public string CourseStudentsUrl { get; set; } = string.Empty;
        public string CourseMaterialsUrl { get; set; } = string.Empty;
        public string CourseQuizzesUrl { get; set; } = string.Empty;
        public string CourseTasksUrl { get; set; } = string.Empty;
    }
} 