using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class BookingService(IUnitOfWork unitOfWork) : IBookingService
    {
        public void CreateBooking(Booking booking)
        {
            unitOfWork.Booking.Add(booking);
            unitOfWork.Save();
        }

        public IEnumerable<Booking> GetAllBookings(string userId = "", string statusFilterList = "")
        {
            var statusList = statusFilterList.ToLower().Split(',');
            if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
            {
                return unitOfWork.Booking.GetAll(b => statusList.Contains(b.Status.ToLower())
                    && b.UserId == userId, includeProperties: "User,Villa");
            }
            else
            {
                if (!string.IsNullOrEmpty(statusFilterList))
                {
                    return unitOfWork.Booking.GetAll(b => statusList.Contains(b.Status.ToLower())
                        , includeProperties: "User,Villa");
                }
                if (!string.IsNullOrEmpty(userId))
                {
                    return unitOfWork.Booking.GetAll(b => b.UserId == userId, includeProperties: "User,Villa");
                }
            }
            return unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
        }

        public Booking GetBookingById(int bookingId)
        {
            return unitOfWork.Booking.Get(b => b.Id == bookingId, includeProperties: "User,Villa");
        }

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
        {
            return unitOfWork.Booking.GetAll(v => v.VillaId == villaId && v.Status == Constants.StatusCheckedIn).Select(v => v.VillaNumber);
        }

        public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber = 0)
        {
            var bookingFromDb = unitOfWork.Booking.Get(b => b.Id == bookingId, tracked: true);
            if (bookingFromDb != null)
            {
                bookingFromDb.Status = bookingStatus;
                if (bookingStatus == Constants.StatusCheckedIn)
                {
                    bookingFromDb.VillaNumber = villaNumber;
                    bookingFromDb.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == Constants.StatusCompleted)
                {
                    bookingFromDb.ActualCheckOutDate = DateTime.Now;
                }
            }
            unitOfWork.Save();
        }

        public void UpdateStripePaymentId(int bookingId, string sessiongId, string paymentIntentId)
        {
            var bookingFromDb = unitOfWork.Booking.Get(b => b.Id == bookingId, tracked: true);
            if (bookingFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessiongId))
                {
                    bookingFromDb.StripeSessionId = sessiongId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.StripePaymentIntentId = paymentIntentId;
                    bookingFromDb.PaymentDate = DateTime.Now;
                    bookingFromDb.IsPaymentSuccessful = true;
                }
            }
            unitOfWork.Save();
        }
    }
}
