using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Admin
{
    public class AddAdminViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
} 