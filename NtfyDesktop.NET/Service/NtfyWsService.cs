using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NtfyDesktop.NET.Models;

namespace NtfyDesktop.NET.Service;

public partial class NtfyWsService : IDisposable
{
    private readonly ClientWebSocket _socket = new();
    public event Action<string>? OnMessageReceived;
    public event Func<Task>? OnConnectionError;
    private readonly Uri _uri;
    private readonly string _token;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public WebSocketState State => _socket.State;

    public NtfyWsService(Uri uri, string token)
    {
        _socket.Options.SetRequestHeader("Authorization", "Bearer " + token);
        _socket.Options.SetRequestHeader("Cache", "no");
        _socket.Options.SetRequestHeader("User-Agent", "NtfyDesktop.NET/0.0.1");
        _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);
        _socket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(5);
        _uri = uri;
        _token = token;
    }

    public void Dispose()
    {
        _socket.Dispose();
        OnConnectionError = null;
        OnMessageReceived = null;
        Console.WriteLine($"[INFO] Disposing {nameof(NtfyWsService)}");
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Connect websocket. Note this is a validation func, so will throw the exception out
    /// </summary>
    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine($"[INFO] Trying to connect to {_uri}");
            await _socket.ConnectAsync(_uri, _cancellationTokenSource.Token);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[ERROR] {nameof(ConnectAsync)} exception: {exception}");
            throw;
        }
    }

    /// <summary>
    /// Start receiving messages. Will call the Action when error occurs so no
    /// need to catch
    /// </summary>
    public async Task StartReceivingAsync()
    {
        Console.WriteLine($"[INFO] Start Receiving message from {_uri}");
        try
        {
            while (_socket.State == WebSocketState.Open)
            {
                Console.WriteLine($"[INFO] Receiving message from {_uri}");
                WebSocketReceiveResult result;
                var ms = new MemoryStream();
                do
                {
                    byte[] buffer = new byte[2048];
                    result = await _socket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                string message = Encoding.UTF8.GetString(ms.ToArray());
                Console.WriteLine($"[Debug] {nameof(StartReceivingAsync)} received: {message}");
                OnMessageReceived?.Invoke(message);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[INFO] {nameof(StartReceivingAsync)} cancelled");
            // Necessary. Network error may cause cancel.
            // User Cancellation show first remove error handler. 
            OnConnectionError?.Invoke();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[ERROR] {nameof(StartReceivingAsync)} exception: {exception}");
            OnConnectionError?.Invoke();
        }
    }

    /// <summary>
    /// Cancel all operations in this service.
    /// </summary>
    public async Task CancelReceivingAsync()
    {
        Console.WriteLine($"[INFO] Cancel Receiving from {_uri}");
        OnConnectionError = null;
        OnMessageReceived = null;
        await _cancellationTokenSource.CancelAsync();
        if (_socket.State == WebSocketState.Open)
        {
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User requested", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Closing websocket: {ex.Message}");
            }
        }
        _socket.Dispose();
    }
}