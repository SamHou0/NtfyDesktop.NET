using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using log4net;
using NtfyDesktop.NET.Helper;
using NtfyDesktop.NET.Models;
using NtfyDesktop.NET.Service;

namespace NtfyDesktop.NET.ViewModels;

public partial class TopicViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TopicViewModel));
    private NtfyTopic? _topicNow;
    private NtfyWsService? _topicNowService;

    private CancellationTokenSource _retryCts = new();

    // delete self and any config 
    public event Action<TopicViewModel>? DeleteRequested;

    /// <summary>
    /// Server uri w/o topic name
    /// </summary>
    [ObservableProperty]
    public partial string? BaseUri { get; set; }

    /// <summary>
    /// The topic real topic name in ntfy systems, bind to ui.
    /// </summary>
    [ObservableProperty]
    public partial string? TopicName { get; set; }

    [ObservableProperty]
    // Only a name created by users.
    public partial string DisplayName { get; set; } = "Untitled";

    // Generated Guid
    [ObservableProperty] public partial string Id { get; set; } = Guid.NewGuid().ToString();

    // The auth token
    [ObservableProperty] public partial string? Token { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = "Not initialized";
    [ObservableProperty] public partial bool IsTestingConnection { get; set; } = false;
    [ObservableProperty] public partial bool IsConnected { get; set; } = false;

    /// <summary>
    /// Default model. Generate fresh model.
    /// </summary>
    public TopicViewModel(Action<TopicViewModel> deleteRequested)
    {
        this.DeleteRequested = deleteRequested;
    }

    /// <summary>
    /// Create model and load topic. Best for reading from disk.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="deleteRequested"></param>
    public TopicViewModel(NtfyTopic topic, Action<TopicViewModel> deleteRequested)
    {
        _topicNow = topic;
        this.BaseUri = topic.BaseUri;
        this.TopicName = topic.TopicName;
        this.DisplayName = topic.DisplayName;
        this.Token = topic.Token;
        this.Id = topic.Id;
        this.DeleteRequested = deleteRequested;
        _ = SaveAndConnect();
    }

    [RelayCommand]
    public async Task DeleteSelf()
    {
        // Process files.
        try
        {
            if (_topicNow != null)
                FileHelper.DeleteTopic(_topicNow);
            else // When user save, but test failed.
            {
                FileHelper.DeleteTopic(Id);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to remove and delete topic file", ex);
        }

        await CancelAllOperations();
        // Notify parent
        DeleteRequested?.Invoke(this);
    }

    [RelayCommand]
    public async Task ApplyEdit()
    {
        if (string.IsNullOrEmpty(BaseUri) || string.IsNullOrEmpty(TopicName) || string.IsNullOrEmpty(DisplayName) ||
            string.IsNullOrEmpty(Token))
        {
            StatusMessage = "Fill in the textbox first.";
            return;
        }

        // First cancel retry and old service!
        await CancelAllOperations();

        IsTestingConnection = true;
        _topicNow = new()
        {
            Id = Id,
            DisplayName = DisplayName,
            BaseUri = BaseUri,
            TopicName = TopicName,
            Token = new(Token),
        };
        await SaveAndConnect();
        IsTestingConnection = false;
    }

    /// <summary>
    /// Save topic and connect to remote ws.
    /// </summary>
    private async Task SaveAndConnect()
    {
        if (_topicNow == null) return;
        StatusMessage = "Connecting...";
        IsConnected = false;

        try
        {
            await ResetWsService();
            await FileHelper.SaveTopic(_topicNow!);
            await _topicNowService!.ConnectAsync();
            _ = _topicNowService!.StartReceivingAsync();
            StatusMessage = "Connected and Receiving...!";
            IsConnected = true;
        }
        catch (Exception ex)
        {
            StatusMessage = "[ERROR] Failed to save & start connection: " + ex.Message;
            StatusMessage += Environment.NewLine +
                             "Why not double check your configuration to ensure network and auth is working?";
            _topicNow = null;
        }
    }

    /// <summary>
    /// Cancel operations and regenerate token, then recreate service.
    /// Uses _topicNow.
    /// </summary>
    private async Task ResetWsService()
    {
        await CancelWsService();

        if (_topicNow == null)
            throw new InvalidOperationException("No topic has been set.");
        _topicNowService = new NtfyWsService(_topicNow.ToUri(), _topicNow.Token);
        _topicNowService.OnMessageReceived += OnMessageReceived;
        _topicNowService.OnConnectionError += NtfyWsServiceOnConnectionError;
    }

    /// <summary>
    /// Cancel old service, and remove service reference.
    /// No exception will occur, call this before exit.
    /// </summary>
    private async Task CancelWsService()
    {
        if (_topicNowService != null)
        {
            try
            {
                Log.Info($"{_topicNow?.DisplayName} disconnecting...");
                await _topicNowService.CancelReceivingAsync()!;
                _topicNowService.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Error canceling websocket", ex);
            }

            _topicNowService = null;
        }
    }

    /// <summary>
    /// Cancel all operations. Call this before exit.
    /// </summary>
    public async Task CancelAllOperations()
    {
        _topicNow = null;
        await _retryCts.CancelAsync();
        await CancelWsService();
    }

    private async Task NtfyWsServiceOnConnectionError()
    {
        Log.Info($"{_topicNow?.DisplayName} Connection Failed. Wait 5 seconds and reconnect...");
        IsConnected = false;
        uint retryTimes = 0;
        do
        {
            StatusMessage = $"[Retry {retryTimes}] Connection error. Reconnecting in 5 seconds...";
            try
            {
                await Task.Delay(5000, _retryCts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Info($"{_topicNow?.DisplayName} Reconnecting Cancelled...");
                return;
            }

            try
            {
                await ResetWsService();
                StatusMessage = $"Reconnecting...";
                await _topicNowService!.ConnectAsync();
                Log.Info($"{_topicNow?.DisplayName} Reconnect Success!");
                _ = _topicNowService.StartReceivingAsync();
            }
            catch
            {
                Log.Error($"Still cannot connect. Wait for next retry {retryTimes + 1}.");
            }
            finally
            {
                retryTimes++;
            }
        } while (_topicNowService!.State != WebSocketState.Open);

        StatusMessage = "Connected and Receiving...!";
        IsConnected = true;
    }

    private static void OnMessageReceived(string message)
    {
        try
        {
            var ntfyMessage = JsonSerializer.Deserialize<NtfyMessage>(message)
                              ?? throw new ArgumentNullException(nameof(message));
            if (ntfyMessage.Event == "message")
            {
                NotifyHelper.SendNotificationDbus(
                    ntfyMessage.Message ?? throw new ArgumentNullException(nameof(ntfyMessage.Message)),
                    ntfyMessage.Title ?? ntfyMessage.Topic);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to process message", ex);
        }
    }
}