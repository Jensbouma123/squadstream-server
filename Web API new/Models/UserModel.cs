    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Identity;

    namespace Web_API_new.Models;

    public class UserModel : IdentityUser
    {
        
        [PersonalData]   
        [Required]
        [MinLength(1)]
        [MaxLength(40)]
        public string? FirstName { get; set; }
        
        [PersonalData]
        [Required]
        [MinLength(1)]
        [MaxLength(40)]
        public string? LastName { get; set; }

        public virtual TeamModel? Team { get; set; }
        
        public bool TwoFactorEnabled { get; set; }
        
        public string? TwoFactorSecretKey { get; set; }
        
    }