# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build / Test / Run

```bash
dotnet build                          # Build (Debug)
dotnet build -c Release               # Build (Release)
dotnet test                           # Run all MSTest unit tests
dotnet run --project NtfyDesktop.NET  # Launch the desktop app
```

Tests are in `NtfyDesktop.NET.Test/` using **MSTest** v4.3. Run a single test:

```bash
dotnet test --filter "FullyQualifiedName~NtfyWebsocketTest"
```

## Architecture

This is a cross-platform desktop client for [ntfy.sh](https://ntfy.sh) — a pub-sub notification service. It connects to ntfy topics via WebSocket and delivers incoming messages as Linux desktop notifications.

- **Framework:** .NET 10.0, Avalonia UI 12.1 (cross-platform desktop GUI)
- **Pattern:** MVVM (Model-View-ViewModel) with `CommunityToolkit.Mvvm` 8.4.2 source generators
- **Theme:** Material Design (Dark base, LightGreen primary) via `Material.Avalonia`

### Project structure

| Project | Purpose |
|---|---|
| `NtfyDesktop.NET/` | Main desktop application (`WinExe`) |
| `NtfyDesktop.NET.Test/` | MSTest integration tests (real WebSocket connections) |

The main project's source folders follow MVVM layering:

| Folder | Role |
|---|---|
| `Views/` | Avalonia XAML windows + code-behind. `MainWindow` is the only window — three-column layout: topic list on the left, editor panel on the right, action bar at the bottom. |
| `ViewModels/` | Observable VMs using `[ObservableProperty]` and `[RelayCommand]` source generators. `MainWindowViewModel` owns the topic collection. `TopicViewModel` manages a per-topic WebSocket lifecycle. |
| `Models/` | Plain data objects. `NtfyTopic` (persisted config), `NtfyMessage` (JSON-deserialized incoming message). |
| `Service/` | `NtfyWsService` — wraps `ClientWebSocket` with Bearer-token auth and a receive loop. |
| `Helper/` | Static utilities: `FileHelper` (JSON persistence), `NotifyHelper` (calls `notify-send`). |
| `Converter/` | XAML `IValueConverter` for first-run UI messaging. |
| `Message/` | `WeakReferenceMessenger` message types for cross-component signaling (e.g., show/hide window). |

### Key behaviors

1. **First-run detection:** `FileHelper.CheckDirectory()` returns `false` if `~/.config/NtfyDesktop.NET/` doesn't exist yet. On first run, the main window opens for configuration. On subsequent runs, the app starts minimized to the system tray and shows a background notification.

2. **Persistent config:** Topic configurations are stored as individual JSON files (`{id}.json`) in `~/.config/NtfyDesktop.NET/`. `FileHelper.SaveTopic` / `LoadTopics` / `DeleteTopic` manage these.

3. **WebSocket lifecycle:** Each `TopicViewModel` owns one `NtfyWsService` wrapping a `ClientWebSocket`. Connection errors trigger an automatic retry loop (5-second delay, indefinite retries until connected) managed through `CancellationTokenSource`. The service exposes `OnMessageReceived` (string event) and `OnConnectionError` (Func<Task> event). Graceful shutdown nulls the error handler before cancelling the CTS, so cancellation doesn't trigger a reconnect.

4. **Notification delivery:** `NotifyHelper.SendNotificationDbus` shells out to `notify-send` with the app icon. This is **Linux-only** — there is no Windows/macOS notification backend.

5. **View-ViewModel resolution:** `ViewLocator` implements `IDataTemplate` and maps `FooViewModel` → `FooView` by string replacement and `Activator.CreateInstance`. Compiled bindings are on by default (`AvaloniaUseCompiledBindingsByDefault`).

6. **Window close behavior:** `MainWindow.Window_OnClosing` intercepts non-programmatic close requests (user clicking X) and hides to tray instead of exiting. Full exit happens through the tray icon's "Exit" menu item, which calls `App.Close_OnClick` → `vm.CancelOperation()` to disconnect all WebSockets gracefully.

7. **Tests are integration tests:** They connect to a real ntfy WebSocket endpoint and require `NTFYDESKTOP_UNITTEST_TOKEN` environment variable for authentication. There are no mocked/offline tests.
