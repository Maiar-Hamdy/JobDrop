using FluentValidation;
using JobDrop.Application.Behaviors;
using JobDrop.Application.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JobDrop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }
}