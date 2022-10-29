using System;
using System.Diagnostics;
using System.IO;

namespace CTFAK.Utils
{
    public static class Logger
    {
        static StreamWriter _writer;
        public static event LoggerHandler OnLogged;
        static Logger()
        {
            File.Delete("Latest.log");
            _writer = new StreamWriter("Latest.log", false);
            _writer.AutoFlush = true;
        }

        public static void Log(object text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White)
        {
            Log(text.ToString(), logToScreen, color);
        }
        public static void LogWarning(object text)
        {
            Log(text.ToString(), true, ConsoleColor.Yellow);
        }
        public static void Log(string text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White)
        {
            var actualText = $"[{DateTime.Now.ToString("HH:mm:ss:ff")}] {text}";
            if (logToScreen)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(actualText);
                Console.ForegroundColor = ConsoleColor.White;
                OnLogged?.Invoke(actualText);
            }

            _writer.WriteLine(actualText);
            _writer.Flush();

            //if (logToConsole) MainConsole.Message(text);
        }
    }
}
