using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class ApplicationUserRepository(ApplicationDbContext context) : GenericRepository<ApplicationUser>(context), IApplicationUserRepository
    {
    }
}
