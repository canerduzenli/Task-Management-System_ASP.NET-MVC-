using Assignment_2.Areas.Identity.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_2.Models
{
    public class TicketUser
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("TicketId")]
        [DisplayName("Ticket ID")]
        public Guid? TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        [ForeignKey("SystemUserId")]
        [DisplayName("System User ID")]
        public string? SystemUserId { get; set; }
        public SystemUser? SystemUser { get; set; }
    }
}
