using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NtfyDesktop.NET.Helper;
using NtfyDesktop.NET.Models;
using NtfyDesktop.NET.Service;

namespace NtfyDesktop.NET.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private NtfyWsService? _service;
    [ObservableProperty] public partial string? UriString { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; } = false;
    [ObservableProperty] public partial string? AuthToken { get; set; }
    [ObservableProperty] public partial string? DisplayMessage { get; set; }

    [RelayCommand]
    public async Task ApplySettings()
    {
        try
        {
            _service = new NtfyWsService(
                new Uri(UriString ??
                        throw new ArgumentNullException(nameof(UriString))),
                AuthToken ??
                throw new ArgumentNullException(nameof(AuthToken)));
            _service.OnMessageReceived += (async (message) => await OnMessageReceive(message));
            await _service.ConnectAsync(_cancellationTokenSource.Token);
            IsConnected = true;
            _ = _service.StartReceivingAsync(_cancellationTokenSource.Token);
            DisplayMessage = "Connected";
        }
        catch (Exception ex)
        {
            DisplayMessage = ex.Message;
            IsConnected = false;
        }
    }

    [RelayCommand]
    public async Task Close(Window window)
    {
        await _cancellationTokenSource.CancelAsync();
        window.Close();
    }

    private static async Task OnMessageReceive(string message)
    {
        try
        {
            var ntfyMessage = JsonSerializer.Deserialize<NtfyMessage>(message)
                              ?? throw new ArgumentNullException(nameof(message));
            if (ntfyMessage.Event == "message")
            {
                NotifyHelper.SendNotificationDbus(
                    ntfyMessage.Title ?? "NtfyDesktop.NET",
                    ntfyMessage.Message ?? throw new ArgumentNullException(nameof(ntfyMessage.Message)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to process message: " + ex.Message);
            Console.WriteLine(ex);
        }
    }
}