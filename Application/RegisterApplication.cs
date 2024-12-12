using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Common;
using FluentValidation;

namespace Application
{
    public static class RegisterApplication
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddCommon();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
