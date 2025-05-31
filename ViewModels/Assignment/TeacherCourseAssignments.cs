using System.Collections.Generic;

namespace LMSTT.ViewModels.Assignment
{
    public class TeacherCourseAssignments
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<AssignmentViewModel> Tasks { get; set; }
    }
} 