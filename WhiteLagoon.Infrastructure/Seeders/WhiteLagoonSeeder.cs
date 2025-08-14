using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Seeders
{
    internal class WhiteLagoonSeeder(ApplicationDbContext context) : IWhiteLagoonSeeder
    {
        public async Task Seed()
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
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
            }
        }
    }
}
