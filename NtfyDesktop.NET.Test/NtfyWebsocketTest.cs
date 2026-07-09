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
        await _service.ConnectAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task TestMessageReceived()
    {
        CancellationTokenSource cts = new();
        string message;
        _service.OnMessageReceived += s =>
        {
            // Should output open event, and quick cancel.
            Console.WriteLine(s);
            cts.Cancel();
        };
        await _service.ConnectAsync(cts.Token);
        await _service.StartReceivingAsync(cts.Token);
    }
}