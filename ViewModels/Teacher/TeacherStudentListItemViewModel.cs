namespace LMSTT.ViewModels.Teacher
{
    public class TeacherStudentListItemViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }
} 