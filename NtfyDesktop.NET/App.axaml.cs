using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NtfyDesktop.NET.ViewModels;
using NtfyDesktop.NET.Views;

namespace NtfyDesktop.NET;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private async void Close_OnClick(object? sender, EventArgs e)
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow window)
            {
                MainWindowViewModel vm = window.DataContext as MainWindowViewModel ??
                                         throw new NullReferenceException();
                await vm.CancelOperation();
                window.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Error] Failed to disconnect window: " + ex);
            Environment.Exit(1);
        }
        finally
        {
            Environment.Exit(0);
        }
    }

    private void Show_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is MainWindow window)
        {
            desktop.MainWindow.Show();
        }
    }
}