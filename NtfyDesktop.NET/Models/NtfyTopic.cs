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
    public required string Id { get; init; }
    // Only a name created by users.
    public required string DisplayName { get; init; }
    public required Uri Uri { get; init; }
    public required string Token { get; init; }
}