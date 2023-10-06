using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Assignment_2.Models;  

namespace Assignment_2.Areas.Identity.Data
{
    public class TaskManagementSystemContext : IdentityDbContext<SystemUser>
    {
        public TaskManagementSystemContext(DbContextOptions<TaskManagementSystemContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Additional customizations for Project and TaskModel can be added here, if needed
        }

        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<Ticket> Tickets { get; set; } = default!;
        public DbSet<ProjectUser> ProjectUsers { get; set; } = default!;
        public DbSet<TicketUser> TicketUsers { get; set; } = default!;
    }
}
