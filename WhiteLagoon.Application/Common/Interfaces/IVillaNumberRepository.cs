using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IVillaNumberRepository : IGenericRepository<VillaNumber>
    {
        void Update(VillaNumber entity);
    }
}
