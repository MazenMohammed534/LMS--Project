using LMSTT.Models;

namespace LMSTT.ViewModels.Teacher
{
    public class CourseStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }

    public class StudentInfo
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }
} 