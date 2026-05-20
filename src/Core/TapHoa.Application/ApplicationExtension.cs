using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TapHoa.Application.Common;

namespace TapHoa.Application;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationExtension).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationExtension).Assembly);

        return services;
    }
}
