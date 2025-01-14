using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Dto;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Validators;
using System.ComponentModel.DataAnnotations;

namespace SchedulingTasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController(IEndpointService endpointService, ILogger<EndpointsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllEndpoints()
        {
            var endpoints = await endpointService.GetAllEndpointsAsync();

            logger.LogInformation("Fetched all endpoints successfully.");

            return Ok(endpoints);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEndpoint([FromBody] EndpointDto endpointDto)
        {
            await new EndpointDtoValidator().ValidateAndThrowAsync(endpointDto);

            var createdEndpoint = await endpointService.CreateEndpointAsync(endpointDto);

            logger.LogInformation($"Endpoint '{createdEndpoint.Name}' created successfully with ID {createdEndpoint.Id}.");

            return CreatedAtAction(nameof(GetAllEndpoints), new { id = createdEndpoint.Id }, createdEndpoint);
        }
    }
}