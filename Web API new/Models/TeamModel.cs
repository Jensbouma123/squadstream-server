using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API_new.Models;

public class TeamModel
{
 
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Name { get; set; }
    
    [Required]
    public string LeaderId { get; set; }

    [ForeignKey("LeaderId")]
    public virtual UserModel Leader { get; set; }

    public virtual ICollection<UserModel> Players { get; set; }
}