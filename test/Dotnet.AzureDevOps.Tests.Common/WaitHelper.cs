namespace Dotnet.AzureDevOps.Tests.Common;

public static class WaitHelper
{
    public static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan pollInterval)
    {
        DateTime start = DateTime.UtcNow;
        while(DateTime.UtcNow - start < timeout)
        {
            if(await condition())
                return;
            await Task.Delay(pollInterval);
        }
        throw new TimeoutException("The condition was not met within the specified timeout.");
    }
}
