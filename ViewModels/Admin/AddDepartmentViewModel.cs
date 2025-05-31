using System.ComponentModel.DataAnnotations;

namespace LMSTT.ViewModels.Admin
{
    public class AddDepartmentViewModel
    {
        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(250)]
        public string Name { get; set; }
    }
}