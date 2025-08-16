using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class VillaNumberRepository(ApplicationDbContext context) : GenericRepository<VillaNumber>(context), IVillaNumberRepository
    {
        public void Update(VillaNumber entity)
        {
            context.VillaNumbers.Update(entity);
        }
    }
}
