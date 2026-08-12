using Library.Core.Constants;
using Library.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Library.Api.Data;

public class DbSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
    )
    {
        // 1. Создаём роли если не существуют
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Создаём администратора по умолчанию
        const string adminEmail = "admin@library.local";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Главный",
                LastName = "Администратор",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
