using System.ComponentModel.DataAnnotations;

namespace LMSTT.Models
{
    public class CourseMaterial
    {
        public int Id { get; set; }
        public string? FileName { get; set; } = string.Empty; //Original Name
        public string? StoredFileName { get; set; } = string.Empty; //Fake Name that Stored in the server
        public string? ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int CourseId { get; set; }

        // Navigation property
        public Course Course { get; set; } = null!;
    }
}
