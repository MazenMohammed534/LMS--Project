using System.ComponentModel.DataAnnotations;

namespace LMSTT.Attributes
{
    public class AllowedExtentionsAttribute : ValidationAttribute
    {
        private readonly string _allowedExtensions;

        public AllowedExtentionsAttribute(string allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid
            (object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                var allowedExtensions = _allowedExtensions.Split(',');
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new ValidationResult($"Allowed file types are: {string.Join(", ", allowedExtensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
