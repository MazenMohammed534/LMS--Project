using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Discussion
{
    public class DiscussionViewModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DiscussionMessageViewModel> Messages { get; set; } = new List<DiscussionMessageViewModel>();
    }

    public class DiscussionMessageViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserPhotoUrl { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class SendMessageViewModel
    {
        [Required]
        public int DiscussionId { get; set; }

        [Required]
        [StringLength(1000)]
        public string MessageText { get; set; }
    }
} 