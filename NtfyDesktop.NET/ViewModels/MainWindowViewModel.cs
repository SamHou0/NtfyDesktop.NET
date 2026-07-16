using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NtfyDesktop.NET.Helper;
using NtfyDesktop.NET.Message;
using NtfyDesktop.NET.Models;
using NtfyDesktop.NET.Service;

namespace NtfyDesktop.NET.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] public partial ObservableCollection<TopicViewModel> Topics { get; set; } = [];
    [ObservableProperty] public partial TopicViewModel? SelectedTopic { get; set; }
    [ObservableProperty] public partial bool IsItemsEditing { get; set; } = false;
    [ObservableProperty] public partial bool IsFirstRun { get; set; } = false;

    public MainWindowViewModel(bool isFirstRun)
    {
        IsFirstRun = isFirstRun;
        if (!IsFirstRun)
        {
            _ = ReadTopics();
        }
        else
        {
            // TODO: Take a ui tour.
        }
    }

    [RelayCommand]
    public void AddTopic()
    {
        TopicViewModel vm = new(RemoveTopic)

        {
            DisplayName = "New Topic",
        };
        if (SelectedTopic != null)
        {
            vm.BaseUri = SelectedTopic.BaseUri;
            vm.Token = SelectedTopic.Token;
        }
        Topics.Add(vm);
    }

    /// <summary>
    /// Handles the topic removal.
    /// </summary>
    /// <param name="topic">The topic that requested the removal</param>
    private void RemoveTopic(TopicViewModel topic)
    {
        Topics.Remove(topic);
    }

    private async Task ReadTopics()
    {
        List<NtfyTopic> topics = await FileHelper.LoadTopics();
        foreach (var topic in topics)
        {
            Topics.Add(new TopicViewModel(topic, RemoveTopic));
        }
    }

    public async Task CancelOperation()
    {
        NotifyHelper.SendNotificationDbus("Please wait while disconnecting from server...");
        foreach (var topic in Topics)
        {
            await topic.CancelAllOperations();
        }
    }
}