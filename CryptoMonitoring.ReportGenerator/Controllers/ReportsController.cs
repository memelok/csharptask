using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CryptoMonitoring.ReportGenerator.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> _logger;
        private readonly IReportService _reportService;

        public ReportsController(ILogger<ReportsController> logger, IReportService reportService)
        {
            _logger = logger;
            _reportService = reportService;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily([FromQuery] DateTime date)
        {
            _logger.LogInformation("Daily report requested for {Date}", date);

            try
            {
                var req = new ReportRequest
                {
                    Type = ReportType.Daily,
                    Date = date
                };

                req.NormalizeDatesToUtc();

                var result = await _reportService.GenerateAsync(req);
                return File(result.Stream, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily report");
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            _logger.LogInformation("Dashboard requested: {Start} - {End}", start, end);

            try
            {
                var req = new ReportRequest
                {
                    Type = ReportType.Dashboard,
                    StartDate = start,
                    EndDate = end
                };

                req.NormalizeDatesToUtc();


                _logger.LogInformation("Calling GenerateDashboardAsync");
                var html = await _reportService.GenerateDashboardAsync(req);
                _logger.LogInformation("Dashboard HTML generated");

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard");
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
