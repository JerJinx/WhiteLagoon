using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController(IUnitOfWork unitOfWork) : Controller
    {

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = unitOfWork.Booking.GetAll(b => 
                    b.Status == Constants.StatusApproved || 
                    b.Status == Constants.StatusCheckedIn)
                .ToList();


            foreach (var villa in villaList)
            {
                int roomsAvailable = Constants.VillaRoomsAvailableCount(
                    villa.Id,
                    villaNumberList,
                    checkInDate,
                    nights,
                    bookedVillas);

                villa.IsAvailable = roomsAvailable > 0;
            }

            HomeVM homeVM = new()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate
            };

            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
