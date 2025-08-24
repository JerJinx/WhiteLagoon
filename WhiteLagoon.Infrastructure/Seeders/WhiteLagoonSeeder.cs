using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Seeders
{
    internal class WhiteLagoonSeeder(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager) : IWhiteLagoonSeeder
    {
        public async Task Seed()
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }

            if (!roleManager.RoleExistsAsync(Constants.Role_Admin).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(Constants.Role_Admin)).Wait();
                roleManager.CreateAsync(new IdentityRole(Constants.Role_Customer)).Wait();

                await userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    Name = "Admin Jer",
                    NormalizedEmail = "ADMIN@TEST.COM",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PhoneNumber = "1234567890"
                }, "P@ssw0rd");

                ApplicationUser user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
                await userManager.AddToRoleAsync(user, Constants.Role_Admin);

            }

            if (await context.Database.CanConnectAsync())
            {
                if (!context.Villas.Any())
                {
                    var villasData = await File.ReadAllTextAsync("../WhiteLagoon.Infrastructure/Seeders/SeedData/Villas.json");

                    var villas = JsonSerializer.Deserialize<List<Villa>>(villasData);

                    if (villas == null) return;

                    context.Villas.AddRange(villas);

                    await context.SaveChangesAsync();
                }
                if (!context.VillaNumbers.Any())
                {
                    var villaNumbersData = await File.ReadAllTextAsync("../WhiteLagoon.Infrastructure/Seeders/SeedData/VillaNumbers.json");

                    var villaNumbers = JsonSerializer.Deserialize<List<VillaNumber>>(villaNumbersData);

                    if (villaNumbers == null) return;

                    context.VillaNumbers.AddRange(villaNumbers);

                    await context.SaveChangesAsync();
                }
                if (!context.Amenities.Any())
                {

                    var amenityData = await File.ReadAllTextAsync("../WhiteLagoon.Infrastructure/Seeders/SeedData/Amenities.json");

                    var amenities = JsonSerializer.Deserialize<List<Amenity>>(amenityData);

                    if (amenities == null) return;

                    context.Amenities.AddRange(amenities);

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
