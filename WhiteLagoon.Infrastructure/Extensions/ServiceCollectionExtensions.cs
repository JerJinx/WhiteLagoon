using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repositories;
using WhiteLagoon.Infrastructure.Seeders;

namespace WhiteLagoon.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(option =>
        {
            option.AccessDeniedPath = "/Account/AccessDenied";
            option.LoginPath = "/Account/Login";
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 6;
        });

        services.AddScoped<IWhiteLagoonSeeder, WhiteLagoonSeeder>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
