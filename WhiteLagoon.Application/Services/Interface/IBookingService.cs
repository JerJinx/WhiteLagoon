using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);
        IEnumerable<Booking> GetAllBookings(string userId = "", string statusFilterList = "");
        Booking GetBookingById(int bookingId);
        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdateStripePaymentId(int bookingId, string sessiongId, string paymentIntentId);
        IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}
