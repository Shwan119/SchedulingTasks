using SchedulingTasks.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingTasks.Dto
{
    public class EndpointResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullUrl { get; set; }
        public string HttpMethod { get; set; }
        public int TimeoutSeconds { get; set; }
        public bool IsActive { get; set; }
    }
}