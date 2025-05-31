using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Admin
{
    public class EditAdminViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; } // Optional: Only update if not empty
    }
} 