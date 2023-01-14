using System;
using System.IO;
using EasyNetLog;

namespace CTFAK.Utils;

public static class Logger
{
    private static readonly EasyNetLogger logger;

    static Logger()
    {
        logger = new EasyNetLogger(
            log => $"<color=gray>[<color=purple>{DateTime.Now:HH:mm:ss.fff}</color>]</color> {log}", true,
            new[] { Path.Combine("Logs", $"{DateTime.Now:yy-MM-dd_HH_mm_ss}.log") }, Array.Empty<LogStream>());
    }


    public static event LoggerHandler OnLogged;


    public static void LogWarning(object msg)
    {
        logger.Log($"<color=yellow>{msg.ToString() ?? "null"}</color>");
    }


    public static void Log(string msg)
    {
        logger.Log(msg);
    }

    public static void Log(object msg)
    {
        Log(msg.ToString());
    }
}