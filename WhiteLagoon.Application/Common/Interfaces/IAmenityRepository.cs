using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IAmenityRepository : IGenericRepository<Amenity>
    {
        void Update(Amenity entity);
    }
}
