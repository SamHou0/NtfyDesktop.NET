using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using NtfyDesktop.NET.Helper;
using NtfyDesktop.NET.Message;

namespace NtfyDesktop.NET.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        WeakReferenceMessenger.Default.Register<ChangeWindowStatusMessage>(this,
            WindowStatusChange);
        InitializeComponent();
        ExtractIcon();
    }

    private void WindowStatusChange(object recipient, ChangeWindowStatusMessage message)
    {
        switch (message.Status)
        {
            case WindowStatus.Hidden:
                Hide();
                break;
            case WindowStatus.Normal:
                Show();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void ExtractIcon()
    {
        try
        {
            var image = new Bitmap(AssetLoader.Open(new Uri("avares://NtfyDesktop.NET/Assets/Ntfy.png")));
            FileHelper.SaveImage(image, FileHelper.AppIcon);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Warning] Failed to extract icon Ntfy.png. " + ex.Message);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic)
        {
            e.Cancel = true;
            NotifyHelper.SendNotificationDbus(
                "NtfyDesktop.NET is hiding into background! Your message receiving continues...");
            Hide();
        }
    }
}