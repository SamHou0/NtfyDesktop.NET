using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NtfyDesktop.NET.Helper;

public static class NotifyHelper
{
    public static void SendNotificationDbus(string message, string title = "Ntfy Desktop")
    {
        // TODO: Handle bad string format.
        Process.Start("notify-send",
            string.Concat(
                "--app-name=NtfyDesktop.NET ",
                "--icon=\"",
                FileHelper.AppIcon,
                "\" \"",
                title, "\" \"",
                message, "\""
            )
        );
    }
}