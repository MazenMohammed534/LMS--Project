using System;

namespace LMSTT.ViewModels.Student
{
    public class StudentSubjectCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int MaterialCount { get; set; }
        public int QuizCount { get; set; }
        public int TaskCount { get; set; }
        public int DiscussionCount { get; set; }

        // URLs for navigation
        public string CourseMaterialsUrl { get; set; } = string.Empty;
        public string CourseQuizzesUrl { get; set; } = string.Empty;
        public string CourseTasksUrl { get; set; } = string.Empty;
        public string CourseDiscussionsUrl { get; set; } = string.Empty;
    }
} 