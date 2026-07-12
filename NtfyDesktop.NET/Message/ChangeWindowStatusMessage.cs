namespace NtfyDesktop.NET.Message;

public class ChangeWindowStatusMessage
{
    public required WindowStatus Status { get; init; }
}

public enum WindowStatus
{
    Normal,
    Hidden
}