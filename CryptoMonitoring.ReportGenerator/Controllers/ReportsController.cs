using Microsoft.AspNetCore.Mvc;
using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Services;


namespace CryptoMonitoring.ReportGenerator.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
            => _reportService = reportService;

        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily([FromQuery] DateTime date)
        {
            var req = new ReportRequest
            {
                Type = ReportType.Daily,
                Date = date
            };
            var result = await _reportService.GenerateAsync(req);
            return File(result.Stream, result.ContentType, result.FileName);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var req = new ReportRequest
            {
                Type = ReportType.Dashboard,
                StartDate = start,
                EndDate = end
            };
            var html = await _reportService.GenerateDashboardAsync(req);
            return Content(html, "text/html");
        }
    }
}
