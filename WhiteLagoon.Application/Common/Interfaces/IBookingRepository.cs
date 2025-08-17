using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository :IGenericRepository<Booking>
    {
        void Update(Booking entity);
        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdateStripePaymentId(int bookingId, string sessiongId, string paymentIntentId);
    }
}
