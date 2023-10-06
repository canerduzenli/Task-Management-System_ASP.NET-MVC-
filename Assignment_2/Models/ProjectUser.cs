using Assignment_2.Areas.Identity.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_2.Models
{
    public class ProjectUser
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        [DisplayName("Project ID")]
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        [ForeignKey("SystemUsers")]
        [DisplayName("System User ID")]
        public string? SystemUserId { get; set; }
        public SystemUser? SystemUser { get; set; }
    }
}
