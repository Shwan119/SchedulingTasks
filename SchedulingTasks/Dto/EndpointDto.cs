namespace SchedulingTasks.Dto
{
    public class EndpointDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseUrl { get; set; }
        public string Path { get; set; }
        public string HttpMethod { get; set; } // GET, POST, PUT, etc.
        public int TimeoutSeconds { get; set; }
        public bool IsActive { get; set; }
    }
}