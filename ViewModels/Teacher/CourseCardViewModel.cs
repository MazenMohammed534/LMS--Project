namespace LMSTT.ViewModels.Teacher
{
    public class CourseCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Cover { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int EnrollmentsCount { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string AcademicYearName { get; set; } = string.Empty;
    }
}
