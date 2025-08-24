using Microsoft.Extensions.DependencyInjection;
using WhiteLagoon.Application.Services.Implementation;
using WhiteLagoon.Application.Services.Interface;

namespace WhiteLagoon.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IVillaService, VillaService>();
            services.AddScoped<IVillaNumberService, VillaNumberService>();
            services.AddScoped<IAmenityService, AmenityService>();
            services.AddScoped<IBookingService, BookingService>();
        }
    }
}
