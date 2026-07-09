using System;

namespace NtfyDesktop.NET.Models;

public class NtfyTopic
{
    // Uri already contains topic name.
    public required Uri Uri { get; set; }
    // Only a name created by users.
    public required string DisplayName { get; set; }
}