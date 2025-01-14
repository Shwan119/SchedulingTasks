using SchedulingTasks.Dto;
using Endpoint = SchedulingTasks.Models.Endpoint;

namespace SchedulingTasks.Interfaces
{
    public interface IEndpointService
    {
        Task<IEnumerable<EndpointResponseDto>> GetAllEndpointsAsync();
        Task<Endpoint> GetEndpointByIdAsync(int id);
        Task<EndpointResponseDto> CreateEndpointAsync(EndpointDto endpoint);
        Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint);
        Task<bool> DeleteEndpointAsync(int id);
    }
}
