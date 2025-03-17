

namespace SchedulingTasks.Controllers
{
    public class testing
    {
        Who is performing:
It appears you are the one performing this change to unsubscribe users from reports they haven't used in the past 90 days and implementing the email notification system.
How you are doing this change:
You've implemented two acceptance criteria:


AC1: You're unsubscribing users from reports they haven't accessed in the last 90 days
AC2: You've set up email notifications that will automatically trigger to both the affected users and their managers after revoking report access


How you validate it:
You haven't specifically mentioned your validation method. To properly validate these changes, you might consider:


Running a test to confirm users inactive for 90+ days are correctly identified
Verifying that access is properly revoked for these users
Testing the email notification system to ensure both users and managers receive the appropriate notifications
Checking logs or system records to confirm the process works as expected
Possibly implementing a small pilot with a subset of users before full deployment



    Who is performing the change:
A scheduled computer program automatically handles this task.It runs at specific times based on your system settings.
How the change works:

The program finds users who haven't looked at their reports in the last 90 days
It automatically removes these unused report subscriptions
After removing access, it sends notification emails to both the affected users and their managers


How it's validated:

The system keeps detailed records of what it's doing
It checks if it's the right day to run this cleanup
It counts how many subscriptions were found and removed
It tracks whether emails were successfully sent
If something goes wrong, it records the error so you can check what happened
    }
}