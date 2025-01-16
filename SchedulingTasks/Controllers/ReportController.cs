using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController(IReportHubLibraryService reportHubLibraryService, ILogger<ReportController> logger, ICorrelationIdProvider correlationProvider) : ControllerBase
    {
        private static int _attemptCount = 0;

        [HttpPost("generate-scheduled")]
        public async Task<IActionResult> GenerateScheduledReports(CancellationToken cancellationToken)
        {
            var correlationId = correlationProvider.GetCorrelationId();

            try
            {
                _attemptCount++;

                if (_attemptCount == 1)
                    await Task.Delay(TimeSpan.FromSeconds(50), cancellationToken);

                // GetPendingAttestationsAsync
                logger.LogInformation($"Starting generating scheduled reports in the controller - {correlationId}");

                await reportHubLibraryService.GenerateScheduledReportsAsync();

                logger.LogInformation($"Starting generating scheduled reports in the controller - {correlationId}");

                return Ok(new { message = "Reports generated successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error generating scheduled reports - {correlationId}");
                return StatusCode(500, new { error = $"Failed to generate reports - {correlationId}" });
            }
        }
    }
}
