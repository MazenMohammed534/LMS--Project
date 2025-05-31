using System;
using System.Collections.Generic;
using LMSTT.Models;

namespace LMSTT.ViewModels.Assignment
{
    public class StudentTasksViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<StudentTaskInfo> Tasks { get; set; }
    }

    public class StudentTaskInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public SubmissionStatus CompletionStatus { get; set; }
        public ActionStatus ActionStatus { get; set; }
        public bool CanSubmit => CompletionStatus == SubmissionStatus.NotCompleted || 
                                (CompletionStatus == SubmissionStatus.NotCompleted && ActionStatus == ActionStatus.Rejected);
        public bool CanViewSubmission => CompletionStatus == SubmissionStatus.Completed;
    }
} 