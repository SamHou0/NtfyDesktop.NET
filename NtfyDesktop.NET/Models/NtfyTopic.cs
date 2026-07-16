using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NtfyDesktop.NET.Service;

namespace NtfyDesktop.NET.Models;

public class NtfyTopic
{
    /// <summary>
    /// Randomly generated item Guid
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Only a name created by users.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The server uri
    /// </summary>
    public required string BaseUri { get; init; }

    /// <summary>
    /// The name of the topic, in the uri string
    /// </summary>
    public required string TopicName { get; init; }

    public required string Token { get; init; }
    public Uri ToUri() => new($"wss://{BaseUri}/{TopicName}/ws");
}