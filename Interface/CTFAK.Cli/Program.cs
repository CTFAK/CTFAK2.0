using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using CTFAK;
using CTFAK.EXE;
using CTFAK.FileReaders;
using CTFAK.Tools;
using CTFAK.Utils;

public class Program
{
    public static IFileReader gameParser;

    public static void Main(string[] args)
    {
        CTFAK.CTFAKCore.Init();
        ASCIIArt.SetStatus("Idle");
            Directory.CreateDirectory("Plugins");
            Directory.CreateDirectory("Dumps");
            ASCIIArt.DrawArt2();
            ASCIIArt.DrawArt();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("by 1987kostya");
            Console.ResetColor();
            Thread.Sleep(700);
            Console.Clear();
            




            ASCIIArt.DrawArt();

            ASK_FOR_PATH:
            ASCIIArt.SetStatus("Waiting for file");
            string path = string.Empty;
            if (args.Length == 0)
            {
                Console.Write("Game path: ");
                path = Console.ReadLine().Trim('"');
            }
            else path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("ERROR: File not found");
                goto ASK_FOR_PATH;
            }
            Console.Write("Parameters: ");
            var loadParams = Console.ReadLine();
            CTFAK.CTFAKCore.parameters = loadParams;

            var types = Assembly.GetAssembly(typeof(ExeFileReader)).GetTypes();

            List<IFileReader> availableReaders = new List<IFileReader>();



            if (Path.GetExtension(path)==".exe")
                gameParser = new ExeFileReader();
            else if (Path.GetExtension(path) == ".apk")
            {
                if (File.Exists(Path.GetTempPath() + "application.ccn"))
                    File.Delete(Path.GetTempPath() + "application.ccn");
                path = ApkFileReader.ExtractCCN(path);
                gameParser = new CCNFileReader();
            }
            else if (Path.GetExtension(path) == ".mfa")
                gameParser = new MFAFileReader();
            else
            {
                SELECT_READER:
                foreach (var rawType in types)
                    if (rawType.GetInterface(typeof(IFileReader).FullName) != null)
                        availableReaders.Add((IFileReader)Activator.CreateInstance(rawType));
                foreach (var item in Directory.GetFiles("Plugins", "*.dll"))
                {
                    var newAsm = Assembly.LoadFrom(Path.GetFullPath(item));
                    foreach (var pluginType in newAsm.GetTypes())
                        if (pluginType.GetInterface(typeof(IFileReader).FullName) != null)
                            availableReaders.Add((IFileReader)Activator.CreateInstance(pluginType));
                }
                //Console.Clear();
                ASCIIArt.DrawArt();
                ASCIIArt.SetStatus("Selecting tool");
                Console.WriteLine($"{availableReaders.Count} readers(s) available\n\nSelect reader: ");
                Console.WriteLine("0. Exit CTFAK");
                for (int i = 0; i < availableReaders.Count; i++)
                    Console.WriteLine($"{i + 1}. {availableReaders[i].Name}");
                var key1 = Console.ReadLine();
                var readerSelect = int.Parse(key1);
                if (readerSelect == 0) Environment.Exit(0);
                gameParser = availableReaders[readerSelect - 1];
            }
            
            var readStopwatch = new Stopwatch();
            readStopwatch.Start();
            //Console.Clear();
            ASCIIArt.DrawArt();
            ASCIIArt.SetStatus("Reading game");
            Console.WriteLine($"Reading game with \"{gameParser.Name}\"");
            gameParser.PatchMethods();
            

            gameParser.LoadGame(path);
            readStopwatch.Stop();

            //Console.Clear();
            ASCIIArt.DrawArt();
            Console.WriteLine($"Reading finished in {readStopwatch.Elapsed.TotalSeconds} seconds");
            
            List<IFusionTool> availableTools = new List<IFusionTool>();
            foreach (var rawType in types)
            {
                if (rawType.GetInterface(typeof(IFusionTool).FullName) != null)
                availableTools.Add((IFusionTool)Activator.CreateInstance(rawType));
            }
            foreach (var item in Directory.GetFiles("Plugins","*.dll"))
            {
                var newAsm = Assembly.LoadFrom(Path.GetFullPath(item));
                foreach (var pluginType in newAsm.GetTypes())
                {
                    if (pluginType.GetInterface(typeof(IFusionTool).FullName) != null)
                        availableTools.Add((IFusionTool)Activator.CreateInstance(pluginType));
                }
            }
        SELECT_TOOL:
            Console.WriteLine("");
            Console.WriteLine($"Game Information:");
            Console.WriteLine($"Game Name: "+gameParser.getGameData().name);
            Console.WriteLine($"Author: "+gameParser.getGameData().author);
            Console.WriteLine($"Number of frames: "+gameParser.getGameData().frames.Count);
            Console.WriteLine($"Fusion Build: "+Settings.Build);
            Console.WriteLine("");
            ASCIIArt.SetStatus("Selecting tool");
            Console.WriteLine($"{availableTools.Count} tool(s) available\n\nSelect tool: ");
            Console.WriteLine("0. Exit CTFAK");
            for (int i = 0; i < availableTools.Count; i++)
            {
                Console.WriteLine($"{i+1}. {availableTools[i].Name}");
            }
            var key = Console.ReadLine();
            var toolSelect = int.Parse(key);
            if (toolSelect == 0) Environment.Exit(0);
            IFusionTool selectedTool = availableTools[toolSelect-1];
            Console.WriteLine($"Selected tool: {selectedTool.Name}. Executing");
            var executeStopwatch = new Stopwatch();
            executeStopwatch.Start();
            ASCIIArt.SetStatus($"Executing {selectedTool.Name}");
            try
            {
                selectedTool.Execute(gameParser);
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
                Console.ReadKey();
            }
            executeStopwatch.Stop();
            Console.Clear();
            
            ASCIIArt.DrawArt();
            Console.WriteLine($"Execution of {selectedTool.Name} finished in {executeStopwatch.Elapsed.TotalSeconds} seconds");
            goto SELECT_TOOL;

            Console.ReadKey();
    }
}