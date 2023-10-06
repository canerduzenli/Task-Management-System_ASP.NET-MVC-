using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Assignment_2.Models
{
    public class Ticket
    {
        [Required]
        [Key]
        [DisplayName("Ticket ID")]
        public Guid Id { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        [DisplayName("Ticket Title")]
        public string TicketTitle { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }

        [Required]
        [Range(1, 999)]
        [DisplayName("Required Hours")]
        public int RequiredHours { get; set; }

        [Display(Name = "Ticket Completed")]
        [Required]
        public bool IsComplete { get; set; } = false;

        [ForeignKey("Project")]
        [DisplayName("Project ID")]
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        // Developers
        public HashSet<TicketUser>? TicketUsers { get; set; } = new HashSet<TicketUser>();
    }

    public enum TicketPriority
    {
        High,
        Medium,
        Low
    }
}
