using Application.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
                configuration.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            });
            services.AddValidatorsFromAssembly(
    Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
