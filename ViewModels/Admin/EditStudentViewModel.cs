using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Admin
{
    public class EditStudentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
} 