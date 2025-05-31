using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Teacher
{
    public class TeacherDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalTasks { get; set; }
        public int TotalQuizzes { get; set; }
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }
    }
}
