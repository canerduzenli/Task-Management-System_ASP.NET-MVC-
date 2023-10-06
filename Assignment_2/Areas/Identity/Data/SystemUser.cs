using Assignment_2.Models;
using Microsoft.AspNetCore.Identity;

namespace Assignment_2.Areas.Identity.Data
{
    // Manager (SystemUser) can have many Projects
    // Developer (SystemUser) can have many Projects

    public class SystemUser : IdentityUser
    {
        public HashSet<ProjectUser>? ProjectUsers { get; set; } = new HashSet<ProjectUser>();
        public HashSet<TicketUser>? TicketUsers { get; set; } = new HashSet<TicketUser>();
    }
}
