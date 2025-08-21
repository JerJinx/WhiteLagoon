using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController(IUnitOfWork unitOfWork) : Controller
    {
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new DateTime(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetBookingRadialChartData()
        {
            var totalBookings = unitOfWork.Booking.GetAll(b => b.Status != Constants.StatusPending || b.Status == Constants.StatusCancelled);

            var countByCurrentMonth = totalBookings.Count(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBookings.Count(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth));
        }


        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            var totalUsers = unitOfWork.User.GetAll();

            var countByCurrentMonth = totalUsers.Count(b => b.CreatedAt >= currentMonthStartDate && b.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(b => b.CreatedAt >= previousMonthStartDate && b.CreatedAt <= currentMonthStartDate);

           

            return Json(GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            var totalBookings = unitOfWork.Booking.GetAll(b => b.Status != Constants.StatusPending || b.Status == Constants.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(b => b.TotalCost));

            var countByCurrentMonth = totalBookings
                    .Where(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now)
                    .Sum(b => b.TotalCost);

            var countByPreviousMonth = totalBookings
                .Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate)
                    .Sum(b => b.TotalCost);



            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            var totalBookings = unitOfWork.Booking.GetAll(b => 
                b.BookingDate >= DateTime.Now.AddDays(-30) &&
                (b.Status != Constants.StatusPending || b.Status == Constants.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId)
                    .Where(x => x.Count() == 1)
                    .Select(x => x.Key)
                    .ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartVM pieChartVM = new()
            {
                Labels = ["New Customer Bookings", "Returning Customer Bookings"],
                Series = [bookingsByNewCustomer, bookingsByReturningCustomer]
            };

            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            var bookingData = unitOfWork.Booking.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) &&
                b.BookingDate.Date <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });

            var customerData = unitOfWork.User.GetAll(b => b.CreatedAt >= DateTime.Now.AddDays(-30) &&
                b.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });

            var leftJoin = bookingData.GroupJoin(customerData,
                b => b.DateTime,
                c => c.DateTime,
                (b, c) => new
                {
                    b.DateTime,
                    b.NewBookingCount,
                    NewCustomerCount = c.Select(x => x.NewCustomerCount).FirstOrDefault()
                });

            var rightJoin = customerData.GroupJoin(bookingData,
                c => c.DateTime,
                b => b.DateTime,
                (c, b) => new
                {
                    c.DateTime,
                    NewBookingCount = b.Select(x => x.NewBookingCount).FirstOrDefault(),
                    c.NewCustomerCount
                });

            var mergeData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergeData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergeData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergeData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();


            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Customer",
                    Data = newCustomerData
                }
            };

            LineChartVM lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };



            return Json(lineChartVM);
        }

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double currentMonthCount, double previousMonthCount)
        {
            RadialBarChartVM radialBarChartVM = new();
            int increaseDecreaseRatio = 100;
            if (previousMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - previousMonthCount) / previousMonthCount * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarChartVM.HasRatioIncreased = currentMonthCount > previousMonthCount;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };

            return radialBarChartVM;
        }
    }
}
