using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Services
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddMyServices(this IServiceCollection services)
        {
            services.AddScoped<IActiveUser, ActiveUserService>();
            services.AddSingleton<IUserService, UserServiceJson>();
            services.AddScoped<ISaledsService, SaledServiceJson>();
            return services;
        }
    }
}
