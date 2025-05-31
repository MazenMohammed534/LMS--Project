using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSTT.Models
{
    public enum SubmissionStatus
    {
        NotCompleted,
        Completed
    }

    public enum ActionStatus
    {
        NoAction,
        Approved,
        Rejected
    }

    public class AssignmentSubmissions
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string SubmittedText { get; set; }

        public string? SubmittedFile { get; set; } = string.Empty; //Original Name

        public string? StoredFileName { get; set; } = string.Empty; //Fake Name that Stored in the server

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SubmittedAt { get; set; }

        [Required]
        public SubmissionStatus CompletionStatus { get; set; }

        [Required]
        public ActionStatus ActionStatus { get; set; } = ActionStatus.NoAction;

        // Navigation properties
        public virtual Assignments Assignment { get; set; }
        public virtual User Student { get; set; }
    }
}
