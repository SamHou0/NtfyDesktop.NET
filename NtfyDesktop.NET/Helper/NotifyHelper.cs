using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NtfyDesktop.NET.Helper;

public static class NotifyHelper
{
    public static void SendNotificationDbus(string message, string title = "Ntfy Desktop")
    {
        string[] args =
        [
            title,
            message
        ];
        // Generate arg string
        string argString = "\"" +
                           string.Join("\" \"", args)
                           + "\"";
        Process.Start("notify-send", argString);
    }
}