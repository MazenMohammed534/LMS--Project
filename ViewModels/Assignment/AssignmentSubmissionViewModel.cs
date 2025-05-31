using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LMSTT.Models;

namespace LMSTT.ViewModels.Assignment
{
    public class AssignmentSubmissionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please provide submission text")]
        public string SubmittedText { get; set; }

        // For file upload
        public IFormFile SubmissionFile { get; set; }

        public string SubmittedFileName { get; set; }

        public string StoredFileName { get; set; }

        public DateTime SubmittedAt { get; set; }

        public SubmissionStatus CompletionStatus { get; set; }

        public ActionStatus ActionStatus { get; set; }

        // Navigation property
        public Assignments Assignment { get; set; }
        public User Student { get; set; }
    }
} 