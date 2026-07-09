namespace NtfyDesktop.NET.Test;
using NtfyDesktop.NET.Helper;

[TestClass]
public class NotificationTest
{
    [TestMethod]
    public void TestNotificationDbus()
    {
        // This show a message on Linux Desktops.
        NotifyHelper.SendNotificationDbus("Test","A test message");
    }
}