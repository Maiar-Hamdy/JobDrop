using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.Persistence;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppIdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        var roles = new[] { "JobSeeker", "CompanyOwner", "CompanyMember", "PlatformAdmin" };
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.Plans.AnyAsync())
        {
            db.Plans.AddRange(
                new JobDrop.Domain.Entities.Billing.Plan { Code = "free", DisplayName = "Free", PriceAmount = 0m, Period = "monthly" },
                new JobDrop.Domain.Entities.Billing.Plan { Code = "pro", DisplayName = "Pro", PriceAmount = 99m, Period = "monthly" },
                new JobDrop.Domain.Entities.Billing.Plan { Code = "pro-plus", DisplayName = "Pro Plus", PriceAmount = 199m, Period = "monthly" }
            );
            await db.SaveChangesAsync();
        }
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppIdentityRole { Name = role });
                logger.LogInformation("Seeded role {Role}", role);
            }
        }
    }
}