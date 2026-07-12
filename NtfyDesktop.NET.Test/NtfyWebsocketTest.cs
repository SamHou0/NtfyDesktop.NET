namespace NtfyDesktop.NET.Test;
using NtfyDesktop.NET.Service;
[TestClass]
public sealed class NtfyWebsocketTest
{
    private NtfyWsService _service = new(new("wss://notify.samhou.moe/error/ws")
        , Environment.GetEnvironmentVariable("NTFYDESKTOP_UNITTEST_TOKEN"));
    [TestMethod]
    public void TestInitialize()
    {
        Assert.IsNotNull(_service);
    }

    [TestMethod]
    public async Task TestConnect()
    {
        await _service.ConnectAsync();
    }

    [TestMethod]
    public async Task TestMessageReceived()
    {
        _service.OnMessageReceived += async s =>
        {
            // Should output open event, and quick cancel.
            Console.WriteLine(s);
            await _service.CancelReceivingAsync();
        };
        await _service.ConnectAsync();
        await _service.StartReceivingAsync();
    }
}