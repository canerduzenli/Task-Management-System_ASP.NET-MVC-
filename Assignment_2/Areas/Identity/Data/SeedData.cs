using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Assignment_2.Areas.Identity.Data
{
    public static class SeedData
    {
        public async static Task Initialize(IServiceProvider serviceProvider)
        {
            //TaskManagementSystemContext context = new TaskManagementSystemContext(serviceProvider.GetRequiredService<DbContextOptions<TaskManagementSystemContext>>());

            UserManager<SystemUser> userManager = serviceProvider.GetRequiredService<UserManager<SystemUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string administratorRole = "Administrator";
            string projectManagerRole = "Project Manager";
            string developerRole = "Developer";

            if (!await roleManager.RoleExistsAsync(administratorRole))
            {
                await roleManager.CreateAsync(new IdentityRole(administratorRole));
            }

            if (!await roleManager.RoleExistsAsync(projectManagerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(projectManagerRole));
            }

            if (!await roleManager.RoleExistsAsync(developerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(developerRole));
            }


            string adminEmail = "admin@email.com";
            SystemUser adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new SystemUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(adminUser, "@Admin2023");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, administratorRole);
                }
            }
        }
    }
}
