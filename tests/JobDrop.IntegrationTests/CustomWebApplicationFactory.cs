using System;
using System.Linq;
using JobDrop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobDrop.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"JobDropTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. Remove ALL EF Core registrations, especially SQL Server
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                (d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("IDbContextOptionsConfiguration")) ||
                (d.ImplementationType?.FullName?.Contains("SqlServer") == true)
            ).ToList();
            foreach (var d in toRemove) services.Remove(d);

            // 2. Add InMemory with a unique DB name
            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // 3. Build the provider, create schema, seed roles
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppIdentityRole>>();
            foreach (var role in new[] { "JobSeeker", "CompanyOwner", "CompanyMember", "PlatformAdmin" })
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    roleManager.CreateAsync(new AppIdentityRole { Name = role }).GetAwaiter().GetResult();
            }
        });
    }
}