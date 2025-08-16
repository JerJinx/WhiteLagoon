using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class VillaNumberRepository(ApplicationDbContext context) : GenericRepository<VillaNumber>(context), IVillaNumberRepository
    {
        private readonly ApplicationDbContext _context = context;
        public void Update(VillaNumber entity)
        {
            _context.VillaNumbers.Update(entity);
        }
    }
}
