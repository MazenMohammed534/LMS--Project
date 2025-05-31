using System.ComponentModel.DataAnnotations;

namespace LMSTT.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public int? TeacherId { get; set; }
        public DateTime Created_at { get; set; }
        public bool IsArchived { get; set; }
        public string cover { get; set; } = string.Empty;
        public int AcademicYearId { get; set; }
        public int DepartmentId { get; set; }

        // Navigation properties
        public AcademicYear AcademicYear { get; set; } = default!;
        public Department Department { get; set; } = default!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
        public ICollection<Assignments> Assignments { get; set; } = new List<Assignments>();
        public User? Teacher { get; set; }
    }
}
