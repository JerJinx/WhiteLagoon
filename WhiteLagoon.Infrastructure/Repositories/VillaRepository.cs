using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class VillaRepository(ApplicationDbContext context) : GenericRepository<Villa>(context), IVillaRepository
    {
        private readonly ApplicationDbContext _context = context;
        public void Update(Villa entity)
        {
            _context.Villas.Update(entity);
        }
    }
}
