namespace LMSTT.ViewModels.Student
{
    public class CourseListViewModel
    {
        public IEnumerable<CourseCardViewModel> Courses { get; set; } = new List<CourseCardViewModel>();
        public bool IsArchived { get; set; }
        public string PageTitle { get; set; } = string.Empty;
    }
}
