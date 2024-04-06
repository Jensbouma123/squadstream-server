using System;
using System.ComponentModel.DataAnnotations;
using Web_API_new.Models;
using Web_API_new.Utilities;

namespace Web_API_new.DTOs
{
    public class TeamDTO
    {
        public Guid Id { get; set; }
        
        private string _name;
        [Required]
        [MaxLength(30)]
        [MinLength(1)]
        [RegularExpression(@"^[A-Za-z0-9' -]+$")]

        public string Name
        {
            get { return _name; }
            set { _name = SanitizationUtility.SanitizeInput(value); }
        }
        
        public int PlayersCount { get; set; }
    }
}