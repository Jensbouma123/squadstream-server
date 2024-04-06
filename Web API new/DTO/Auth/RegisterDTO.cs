using System.ComponentModel.DataAnnotations;
using Web_API_new.Utilities;

namespace Web_API_new.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [MaxLength(128)]
        [RegularExpression(@"^\S+@\S+\.\S{2,}$")]
        public string Email { get; set; }
        
        [MaxLength(128)]
        [MinLength(12)]
        public string Password { get; set; }
        
        [MaxLength(128)]
        [MinLength(12)]
        public string RepeatPassword { get; set; }
        
        private string _firstName;
        [MaxLength(40)]
        [MinLength(1)]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = SanitizationUtility.SanitizeInput(value); }
        }

        private string _lastName;
        [MaxLength(40)]
        [MinLength(1)]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = SanitizationUtility.SanitizeInput(value); }
        }
    }
}