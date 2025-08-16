using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IVillaRepository : IGenericRepository<Villa>
    {
        void Update(Villa entity);
    }
}
