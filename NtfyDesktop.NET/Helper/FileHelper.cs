using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using NtfyDesktop.NET.Models;

namespace NtfyDesktop.NET.Helper;

public static class FileHelper
{
    public static readonly string AppDir = Environment.GetEnvironmentVariable("APPDIR")
                                           ?? Environment.CurrentDirectory;

    public static readonly string AppIcon = Path.Combine(AppDir, "Ntfy.png");

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NtfyDesktop.NET");

    /// <summary>
    /// Check whether config dir exists. Will automatically create if not exist.
    /// </summary>
    /// <returns>Whether the dir exist before the call.</returns>
    public static bool CheckDirectory()
    {
        if (!Directory.Exists(ConfigPath))
        {
            Directory.CreateDirectory(ConfigPath);
            return false;
        }
        else
        {
            return true;
        }
    }

    public static async Task SaveTopic(NtfyTopic topic)
    {
        CheckDirectory();
        await using StreamWriter writer = new(Path.Combine(ConfigPath, topic.Id + ".json"));
        await writer.WriteAsync(JsonSerializer.Serialize(topic));
    }

    public static async Task<List<NtfyTopic>> LoadTopics()
    {
        CheckDirectory();
        List<NtfyTopic> topics = [];
        foreach (var file in Directory.GetFiles(ConfigPath))
        {
            using StreamReader reader = new(file);
            var topic = JsonSerializer.Deserialize<NtfyTopic>(await reader.ReadToEndAsync());
            if (topic != null)
                topics.Add(topic);
        }

        return topics;
    }

    public static void DeleteTopic(NtfyTopic topic)
    {
        CheckDirectory();
        File.Delete(Path.Combine(ConfigPath, topic.Id + ".json"));
    }

    public static void SaveImage(Bitmap image, string fileName)
    {
        image.Save(fileName, new PngBitmapEncoderOptions()
        {
            CompressionLevel = CompressionLevel.NoCompression
        });
    }
}