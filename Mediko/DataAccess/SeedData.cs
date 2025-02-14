using Mediko.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Mediko.DataAccess
{
    public static class SeedData
    {
        public static async Task InitializeRolesAndAdminUserAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

  
            var adminEmail = "mediko@kocaeli.edu.tr";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = "mediko",
                    Email = adminEmail,
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(adminUser);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            var userEmail = "user@mediko.com";
            var existingUser = await userManager.FindByEmailAsync(userEmail);

            if (existingUser == null)
            {
                var defaultUser = new User
                {
                    UserName = "iboibo",
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(defaultUser);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, "User");
                }
            }

        }
    }
}
