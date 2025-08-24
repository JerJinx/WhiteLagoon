using WhiteLagoon.Application.Common.DTO;

namespace WhiteLagoon.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDto> GetBookingRadialChartData();
        Task<RadialBarChartDto> GetRegisteredUserChartData();
        Task<RadialBarChartDto> GetRevenueChartData();
        Task<PieChartDto> GetBookingPieChartData();
        Task<LineChartDto> GetMemberAndBookingLineChartData();

    }
}
