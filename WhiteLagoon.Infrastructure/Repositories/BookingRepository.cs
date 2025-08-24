using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repositories
{
    public class BookingRepository(ApplicationDbContext context) : GenericRepository<Booking>(context), IBookingRepository
    {
        public void Update(Booking entity)
        {
            context.Bookings.Update(entity);
        }

    }
}
