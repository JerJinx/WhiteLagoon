using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController(IUnitOfWork unitOfWork) : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdendity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdendity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ApplicationUser user = unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = unitOfWork.Villa.Get(v => v.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Email = user.Email,
                Name = user.Name,
                Phone = user.PhoneNumber,
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = unitOfWork.Villa.Get(v => v.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = Constants.StatusPending;
            booking.BookingDate = DateTime.Now;

            var villaNumberList = unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = unitOfWork.Booking.GetAll(b =>
                    b.Status == Constants.StatusApproved ||
                    b.Status == Constants.StatusCheckedIn)
                .ToList();

            int roomsAvailable = Constants.VillaRoomsAvailableCount(
                villa.Id,
                villaNumberList,
                booking.CheckInDate,
                booking.Nights,
                bookedVillas);
            if (roomsAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";

                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaid = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            unitOfWork.Booking.Add(booking);
            unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100), // Convert to cents
                    Currency = "php",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description,
                    },
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = unitOfWork.Booking.Get(b => b.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.Status == Constants.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, Constants.StatusApproved, 0);
                    unitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    unitOfWork.Save();
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = unitOfWork.Booking.Get(b => b.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == Constants.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = unitOfWork.VillaNumber.GetAll(v =>
                    v.VillaId == bookingFromDb.VillaId &&
                    availableVillaNumber.Any(a => a == v.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            unitOfWork.Booking.UpdateStatus(booking.Id, Constants.StatusCheckedIn, booking.VillaNumber);
            unitOfWork.Save();
            TempData["Success"] = "Booking Update Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            unitOfWork.Booking.UpdateStatus(booking.Id, Constants.StatusCompleted, booking.VillaNumber);
            unitOfWork.Save();
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            unitOfWork.Booking.UpdateStatus(booking.Id, Constants.StatusCancelled, booking.VillaNumber);
            unitOfWork.Save();
            TempData["Success"] = "Booking Cancelled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = unitOfWork.VillaNumber.GetAll(v => v.VillaId == villaId);
            var checkedInVilla = unitOfWork.Booking.GetAll(v => v.VillaId == villaId && v.Status == Constants.StatusCheckedIn).Select(v => v.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;

            if (User.IsInRole(Constants.Role_Admin))
            {
                bookings = unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                bookings = unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(b => b.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = bookings });
        }

        #endregion
    }
}
