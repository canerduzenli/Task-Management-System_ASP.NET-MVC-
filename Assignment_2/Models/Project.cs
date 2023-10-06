using Assignment_2.Areas.Identity.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Assignment_2.Models
{
    public class Project
    {
        [Required]
        [Key]
        [DisplayName("Project ID")]
        public Guid Id { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        [DisplayName("Project Title")]
        public string ProjectTitle { get; set; }

        public HashSet<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();

        public HashSet<ProjectUser>? ProjectUsers { get; set; } = new HashSet<ProjectUser>();
    }
}
