using System.Text.Json.Serialization;

namespace NtfyDesktop.NET.Models;

public class NtfyMessage
{
    [JsonPropertyName("id")] public required string Id { get; set; }
    [JsonPropertyName("time")] public required int TimeStamp { get; set; }
    [JsonPropertyName("expires")] public int Expires { get; set; }
    [JsonPropertyName("event")] public required string Event { get; set; }
    [JsonPropertyName("topic")] public required string Topic { get; set; }
    [JsonPropertyName("sequence_id")] public string? SequenceId { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("tags")] public string[]? Tags { get; set; }
    [JsonPropertyName("priority")] public int Priority { get; set; }
    [JsonPropertyName("click")] public string? Click { get; set; }
    
}