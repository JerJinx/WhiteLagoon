using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class AmenityRepository(ApplicationDbContext context) : GenericRepository<Amenity>(context), IAmenityRepository
    {
        public void Update(Amenity entity)
        {
            context.Amenities.Update(entity);
        }
    }
}
