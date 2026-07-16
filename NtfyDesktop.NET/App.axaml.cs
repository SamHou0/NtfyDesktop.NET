using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using log4net;
using NtfyDesktop.NET.Helper;
using NtfyDesktop.NET.ViewModels;
using NtfyDesktop.NET.Views;

namespace NtfyDesktop.NET;

public partial class App : Application
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(App));
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            bool isFirstRun = !FileHelper.CheckDirectory();
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(isFirstRun),
            };
            if (isFirstRun)
            {
                desktop.MainWindow = _mainWindow;
            }
            else
            {
                NotifyHelper.SendNotificationDbus("NtfyDesktop is running background!");
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void Close_OnClick(object? sender, EventArgs e)
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow? mainWindow = null;
                mainWindow ??= desktop.MainWindow as MainWindow;
                mainWindow ??= _mainWindow;
                MainWindowViewModel vm = mainWindow?.DataContext as MainWindowViewModel ??
                                         throw new NullReferenceException();
                await vm.CancelOperation();
                mainWindow!.Close();
                desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to disconnect window", ex);
            Environment.Exit(1);
        }
        // finally
        // {
        //     Environment.Exit(0);
        // }
    }

    private void Show_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow ??= _mainWindow;
            desktop.MainWindow!.Show();
        }
    }
}