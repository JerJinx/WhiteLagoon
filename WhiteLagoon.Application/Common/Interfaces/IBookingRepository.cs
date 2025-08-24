using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository :IGenericRepository<Booking>
    {
        void Update(Booking entity);
    }
}
