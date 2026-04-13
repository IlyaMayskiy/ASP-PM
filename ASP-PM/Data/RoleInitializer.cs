using ASP_PM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASP_PM.Data;

public static class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        string[] roleNames = { "Director", "ProjectManager", "Employee" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var directorEmail = "director@mail.ru";
        var directorUser = await userManager.FindByEmailAsync(directorEmail);
        if (directorUser == null)
        {
            var directorEmployee = new Employee
            {
                FirstName = "Director",
                SecondName = "Admin",
                Email = directorEmail
            };
            dbContext.Employees.Add(directorEmployee);
            await dbContext.SaveChangesAsync();

            directorUser = new AppUser
            {
                UserName = directorEmail,
                Email = directorEmail,
                EmployeeId = directorEmployee.Id
            };
            var createResult = await userManager.CreateAsync(directorUser, "1234");
            if (createResult.Succeeded)
            {
                directorEmployee.AppUserId = directorUser.Id;
                dbContext.Employees.Update(directorEmployee);
                await dbContext.SaveChangesAsync();

                await userManager.AddToRoleAsync(directorUser, "Director");
            }
        }
        else if (directorUser.EmployeeId == null)
        {
            var directorEmployee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Email == directorEmail);
            if (directorEmployee == null)
            {
                directorEmployee = new Employee
                {
                    FirstName = "Director",
                    SecondName = "Admin",
                    Email = directorEmail,
                    AppUserId = directorUser.Id
                };
                dbContext.Employees.Add(directorEmployee);
                await dbContext.SaveChangesAsync();
                directorUser.EmployeeId = directorEmployee.Id;
                await userManager.UpdateAsync(directorUser);
            }
            else
            {
                directorEmployee.AppUserId = directorUser.Id;
                directorUser.EmployeeId = directorEmployee.Id;
                dbContext.Employees.Update(directorEmployee);
                await userManager.UpdateAsync(directorUser);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}