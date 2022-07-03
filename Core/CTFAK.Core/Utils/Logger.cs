using System;
using System.Diagnostics;
using System.IO;

namespace CTFAK.Utils
{
    public static class Logger
    {
        static StreamWriter _writer;
        static Logger()
        {
            File.Delete("Latest.log");
            _writer = new StreamWriter("Latest.log", false);
            _writer.AutoFlush = true;
        }

        public static void Log(object text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White,
            bool logToConsole = true)
        {
            Log(text.ToString(), logToScreen, color, logToConsole);

        }
        public static void LogWarning(object text)
        {
            Log(text.ToString(), true, ConsoleColor.Yellow, true);

        }
        public static void Log(string text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White, bool logToConsole = true)
        {
            var actualText = $"[{DateTime.Now.ToString("HH:mm:ss:ff")}] {text}";
            if (logToScreen)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(actualText);
                Console.ForegroundColor = ConsoleColor.White;
            }


            _writer.WriteLine(actualText);
            _writer.Flush();

            //if (logToConsole) MainConsole.Message(text);



        }
    }
}
