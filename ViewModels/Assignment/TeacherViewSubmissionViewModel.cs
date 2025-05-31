using System;
using System.Collections.Generic;
using LMSTT.Models;

namespace LMSTT.ViewModels.Assignment
{
    public class TeacherViewSubmissionViewModel
    {
        public int AssignmentId { get; set; }
        public string CourseTitle { get; set; }
        public string AssignmentTitle { get; set; }
        public List<StudentSubmissionInfo> StudentSubmissions { get; set; }
    }

    public class StudentSubmissionInfo
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime DueDate { get; set; }
        public SubmissionStatus CompletionStatus { get; set; }
        public ActionStatus ActionStatus { get; set; }
        public bool CanBeApproved => CompletionStatus == SubmissionStatus.Completed && ActionStatus == ActionStatus.NoAction;
        public bool CanBeRejected => CompletionStatus == SubmissionStatus.Completed && ActionStatus == ActionStatus.NoAction;
    }
} 