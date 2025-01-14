using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttestationController(IReportHubLibraryService reportHubLibraryService, ILogger<AttestationController> logger) : ControllerBase
    {
        [HttpPost("process-reminders")]
        public async Task<IActionResult> ProcessReminders([FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            try
            {
                // GetPendingAttestationsAsync
                logger.LogInformation($"Starting fetching pending attestation in the controller - {correlationId}");

                await reportHubLibraryService.GetPendingAttestationsAsync();

                logger.LogInformation($"Completed fetching pending attestation in the controller - {correlationId}");

                // GetPendingAttestationsAsync
                logger.LogInformation($"Starting attestation reminder processing in the controller - {correlationId}");

                await reportHubLibraryService.GetPendingAttestationsAsync();

                logger.LogInformation($"Attestation reminder processing completed successfully in the controller - {correlationId}");

                return Ok(new { message = $"Reminders processed successfully - {correlationId}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing attestation reminders - {correlationId}");
                return StatusCode(500, new { error = "Failed to process reminders - {correlationId}" });
            }
        }
    }
}
