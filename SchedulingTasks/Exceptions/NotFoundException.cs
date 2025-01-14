namespace SchedulingTasks.Exceptions
{
    public class NotFoundException(string message) : IOException(message)
    {
    }
}
