namespace SchedulingTasks.Exceptions
{
    public class ValidationException(string message) : IOException(message)
    {
    }
}
