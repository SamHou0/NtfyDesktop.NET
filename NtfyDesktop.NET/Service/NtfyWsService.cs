using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NtfyDesktop.NET.Service;

public class NtfyWsService
{
    private ClientWebSocket _socket = new();
    public event Action<string>? OnMessageReceived;
    private readonly Uri _uri;

    public NtfyWsService(Uri uri, string token)
    {
        _socket.Options.SetRequestHeader("Authorization", "Bearer " + token);
        _socket.Options.SetRequestHeader("Cache", "no");
        _socket.Options.SetRequestHeader("User-Agent", "NtfyDesktop.NET/0.0.1");
        _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
        _socket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(30);
        _uri = uri;
    }

    public async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            await _socket.ConnectAsync(_uri, token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[INFO] {nameof(ConnectAsync)} cancelled");
        }
    }

    /// <summary>
    /// Start receiving messages.
    /// </summary>
    /// <param name="token"></param>
    public async Task StartReceivingAsync(CancellationToken token)
    {
        try
        {
            while (_socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                var ms = new MemoryStream();
                do
                {
                    byte[] buffer = new byte[2048];
                    result = await _socket.ReceiveAsync(buffer, token);
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                string message = Encoding.UTF8.GetString(ms.ToArray());
                OnMessageReceived?.Invoke(message);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[INFO] {nameof(StartReceivingAsync)} cancelled");
        }
    }
}