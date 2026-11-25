using JobDrop.Application.Contracts.Geography;
using JobDrop.Application.Contracts.Persistence;
using JobDrop.Application.Features.Auth;
using JobDrop.Infrastructure.Geography;
using JobDrop.Infrastructure.Identity;
using JobDrop.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobDrop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            opt.UseSqlServer(cfg.GetConnectionString("Default"),
                sql => sql.UseNetTopologySuite()
                          .EnableRetryOnFailure()
                          .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<AppIdentityUser>(o =>
        {
            o.Password.RequiredLength = 10;
            o.Password.RequireUppercase = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireDigit = true;
            o.Password.RequireNonAlphanumeric = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            o.User.RequireUniqueEmail = true;
            o.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<AppIdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        services.AddOptions<JwtOptions>().BindConfiguration("Jwt").ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<GeographyOptions>().BindConfiguration("Geography").ValidateDataAnnotations().ValidateOnStart();
        services.AddSingleton<JwtTokenService>();

        services.AddHttpClient<GisHttpAdapters>();
        services.AddScoped<IGeocodingProvider>(sp => sp.GetRequiredService<GisHttpAdapters>());
        services.AddScoped<ICommuteRouter>(sp => sp.GetRequiredService<GisHttpAdapters>());
        services.AddScoped<IIsochroneProvider>(sp => sp.GetRequiredService<GisHttpAdapters>());
        services.AddScoped<IOperatingAreaQuery>(sp => sp.GetRequiredService<GisHttpAdapters>());

        // Sprint 4: Billing, Notifications, Workers
        services.AddOptions<JobDrop.Infrastructure.Billing.PaymobOptions>().BindConfiguration("Paymob");
        services.AddHttpClient<JobDrop.Application.Contracts.Billing.IPaymobClient, JobDrop.Infrastructure.Billing.PaymobClient>();
        services.AddScoped<JobDrop.Application.Contracts.Billing.IEntitlementResolver, JobDrop.Infrastructure.Billing.EntitlementResolver>();
        services.AddScoped<JobDrop.Application.Contracts.Notifications.IEmailSender, JobDrop.Infrastructure.Notifications.SmtpEmailSender>();
        services.AddScoped<JobDrop.Application.Contracts.Notifications.IWhatsAppSender, JobDrop.Infrastructure.Notifications.WhatsAppSender>();
        services.AddScoped<JobDrop.Application.Contracts.Notifications.IPushSender, JobDrop.Infrastructure.Notifications.WebPushSender>();
        services.AddHostedService<BackgroundJobs.OutboxNotificationWorker>();
        services.AddHostedService<BackgroundJobs.DocumentScanWorker>();
        services.AddHostedService<BackgroundJobs.ExportJobWorker>();
        services.AddHostedService<BackgroundJobs.AccountDeletionPurgeWorker>();

        services.AddTransient<IRequestHandler<RegisterCommand, Guid>, RegisterHandler>();
        services.AddTransient<IRequestHandler<LoginCommand, AuthResponse>, LoginHandler>();
        services.AddTransient<IRequestHandler<RefreshCommand, AuthResponse>, RefreshHandler>();

        services.AddHostedService<BackgroundJobs.SlaExpirationRefundWorker>();
        services.AddHostedService<BackgroundJobs.ReverseDropExpiryWorker>();
        return services;
    }
}