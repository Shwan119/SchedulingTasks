using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingTasks.Models
{
    public class Endpoint : BaseEntity
    {
        public Endpoint()
        {
            Tasks = new HashSet<ScheduledTask>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseUrl { get; set; }
        public string Path { get; set; }
        public string HttpMethod { get; set; }
        public int TimeoutSeconds { get; set; }
        public bool IsActive { get; set; }
        public string FullUrl => $"{BaseUrl.TrimEnd('/')}{Path}";


        // Navigation properties
        public ICollection<ScheduledTask> Tasks { get; set; }
    }
}