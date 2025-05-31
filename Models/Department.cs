using System.ComponentModel.DataAnnotations;

namespace LMSTT.Models
{
    public class Department
    {
        public int Id { get; set; }

        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
