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
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

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
        const string librarianEmail = "librarian@library.local";

        if (await userManager.FindByEmailAsync(librarianEmail) is null)
        {
            var librarian = new ApplicationUser
            {
                UserName = librarianEmail,
                Email = librarianEmail,
                FirstName = "Главный",
                LastName = "Библиотекарь",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(librarian, "Lib123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(librarian, Roles.Librarian);
        }
    }
}
