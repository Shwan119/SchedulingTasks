using Microsoft.EntityFrameworkCore;
using SchedulingTasks.Data;
using SchedulingTasks.Dto;
using SchedulingTasks.Exceptions;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;
using Endpoint = SchedulingTasks.Models.Endpoint;

namespace SchedulingTasks.Services
{
    public class EndpointService(ApplicationDbContext context, ILogger<EndpointService> logger) : IEndpointService
    {
        public async Task<IEnumerable<EndpointResponseDto>> GetAllEndpointsAsync()
        {
            try
            {
                var endpoints = await context.Endpoints
                    .AsNoTracking()
                    .ToListAsync();

                return endpoints.Select(endpoint => MapToResponseDto(endpoint)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all endpoints");
                throw;
            }
        }

        public async Task<Endpoint> GetEndpointByIdAsync(int id)
        {
            try
            {
                var endpoint = await context.Endpoints.FindAsync(id);

                if (endpoint == null)
                    throw new NotFoundException($"Endpoint with ID {id} not found");

                return endpoint;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving endpoint {EndpointId}", id);
                throw;
            }
        }

        public async Task<EndpointResponseDto> CreateEndpointAsync(EndpointDto endpointDto)
        {
            try
            {
                // Map from DTO to Entity
                var endpoint = MapToEntity(endpointDto);

                // Add the entity to the database
                context.Endpoints.Add(endpoint);

                // Save changes
                await context.SaveChangesAsync();

                // Map back to Response DTO
                return MapToResponseDto(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating endpoint");
                throw;
            }
        }

        public async Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint)
        {
            try
            {
                var existingEndpoint = await context.Endpoints.FindAsync(endpoint.Id);

                if (existingEndpoint == null)
                    throw new NotFoundException($"Endpoint with ID {endpoint.Id} not found");

                context.Entry(existingEndpoint).CurrentValues.SetValues(endpoint);
                await context.SaveChangesAsync();

                return existingEndpoint;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating endpoint {EndpointId}", endpoint.Id);
                throw;
            }
        }

        public async Task<bool> DeleteEndpointAsync(int id)
        {
            try
            {
                var endpoint = await context.Endpoints.FindAsync(id);

                if (endpoint == null) return false;

                context.Endpoints.Remove(endpoint);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting endpoint {EndpointId}", id);
                throw;
            }
        }

        private Endpoint MapToEntity(EndpointDto endpointDto)
        {
            return new Endpoint
            {
                Name = endpointDto.Name,
                Description = endpointDto.Description,
                BaseUrl = endpointDto.BaseUrl,
                Path = endpointDto.Path,
                HttpMethod = endpointDto.HttpMethod,
                TimeoutSeconds = endpointDto.TimeoutSeconds,
                IsActive = endpointDto.IsActive
            };
        }

        private EndpointResponseDto MapToResponseDto(Endpoint endpoint)
        {
            return new EndpointResponseDto
            {
                Id = endpoint.Id,
                Name = endpoint.Name,
                Description = endpoint.Description,
                FullUrl = $"{endpoint.BaseUrl.TrimEnd('/')}{endpoint.Path}",
                HttpMethod = endpoint.HttpMethod,
                TimeoutSeconds = endpoint.TimeoutSeconds,
                IsActive = endpoint.IsActive
            };
        }
    }
}
