using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.DTO;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;

namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController(IDashboardService dashboardService) : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetBookingRadialChartData()
        {
            return Json(await dashboardService.GetBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            return Json(await dashboardService.GetRegisteredUserChartData());
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await dashboardService.GetRevenueChartData());
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            return Json(await dashboardService.GetBookingPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await dashboardService.GetMemberAndBookingLineChartData());
        }
    }
}
