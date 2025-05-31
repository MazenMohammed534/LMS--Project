using System.ComponentModel.DataAnnotations;

namespace LMSTT.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }

        // Navigation properties
        public User Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}
