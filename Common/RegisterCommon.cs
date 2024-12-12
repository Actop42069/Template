using Common.Behaviors;
using Common.Interfaces;
using Common.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Common
{
    public static class RegisterCommon
    {
        public static IServiceCollection AddCommon(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationExceptionBehavior<,>));
            services.AddScoped<IEventDispatcherService, EventDispatcherService>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventDispatcherBehavior<,>));

            return services;
        }
    }
}
