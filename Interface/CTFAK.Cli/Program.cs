using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public static string builddate = "7/17/23";
    public static bool didToolArg = false;

    public static void Main(string[] args)
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var pathToExe = processModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            Directory.SetCurrentDirectory(pathToContentRoot);
        }
        CTFAK.CTFAKCore.Init();
        ASCIIArt.SetStatus("Idle");
        Directory.CreateDirectory("Plugins");
        Directory.CreateDirectory("Dumps");
        ASCIIArt.DrawArt2();
        ASCIIArt.DrawArt();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("by 1987kostya and Yunivers");
        Console.ResetColor();
        Thread.Sleep(700);
        Console.Clear();

        ASCIIArt.DrawArt();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("by 1987kostya and Yunivers");
        Console.WriteLine($"Running {builddate} build.\n");

    ASK_FOR_PATH:
        ASCIIArt.SetStatus("Waiting for file");
        Console.ResetColor();
        string path = string.Empty;
        CTFAKCore.parameters = string.Empty;
        if (args.Length != 1)
        {
            if (!args.Contains("-path"))
            {
                Console.Write("Game path: ");
                path = Console.ReadLine().Trim('"');
            }
            else
            {
                bool foundpath = false;
                foreach (string s in args)
                    if (foundpath)
                    {
                        path = s;
                        break;
                    }
                    else if (s == "-path")
                        foundpath = true;
            }
        }
        else path = args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine("ERROR: File not found");
            goto ASK_FOR_PATH;
        }
        if (args.Length != 1)
        {
            if (!args.Contains("-path"))
            {
                Console.Write("Parameters: ");
                CTFAKCore.parameters = Console.ReadLine();
            }
            else
            {
                bool foundparams = false;
                foreach (string s in args)
                    if (foundparams)
                    {
                        CTFAKCore.parameters = s;
                        break;
                    }
                    else if (s == "-parameters")
                        foundparams = true;
            }
        }

        var types = Assembly.GetAssembly(typeof(ExeFileReader)).GetTypes();

        List<IFileReader> availableReaders = new List<IFileReader>();


        if (!args.Contains("-forcetype"))
        {
            if (Path.GetExtension(path) == ".exe")
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
        }
        else
        {
            bool foundtype = false;
            foreach (string s in args)
                if (foundtype)
                {
                    switch (s.ToLower())
                    {
                        case "exe":
                            gameParser = new ExeFileReader();
                            break;
                        case "ccn":
                            gameParser = new CCNFileReader();
                            break;
                        case "apk":
                            if (File.Exists(Path.GetTempPath() + "application.ccn"))
                                File.Delete(Path.GetTempPath() + "application.ccn");
                            path = ApkFileReader.ExtractCCN(path);
                            gameParser = new CCNFileReader();
                            break;
                        case "mfa":
                            gameParser = new MFAFileReader();
                            break;
                    }
                    break;
                }
                else if (s == "-forcetype")
                    foundtype = true;
        }
            
        var readStopwatch = new Stopwatch();
        readStopwatch.Start();
        //Console.Clear();
        ASCIIArt.DrawArt();
        ASCIIArt.SetStatus("Reading game");
        Console.WriteLine($"Reading game with \"{gameParser.Name}\"");
        gameParser.PatchMethods();
        gameParser.LoadGame(path);
        IFileReader game = gameParser.Copy();
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
        if (args.Contains("-closeonfinish") && !args.Contains("-tool"))
            Environment.Exit(0);
    SELECT_TOOL:
        Console.WriteLine("");
        Console.WriteLine($"Game Information:");
        Console.WriteLine($"Game Name: "+gameParser.getGameData().name);
        Console.WriteLine($"Author: "+gameParser.getGameData().author);
        Console.WriteLine($"Number of frames: "+gameParser.getGameData().frames.Count);
        Console.WriteLine($"Fusion Build: "+ gameParser.getGameData().productBuild);
        Console.WriteLine("");
        ASCIIArt.SetStatus("Selecting tool");
        if (!args.Contains("-tool") && !didToolArg || didToolArg)
        {
            Console.WriteLine($"{availableTools.Count} tool(s) available\n\nSelect tool: ");
            Console.WriteLine("0. Exit CTFAK");
            for (int i = 0; i < availableTools.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {availableTools[i].Name}");
            }
            var key = Console.ReadLine();
            var toolSelect = int.Parse(key);
            if (toolSelect == 0) Environment.Exit(0);
            IFusionTool selectedTool = availableTools[toolSelect - 1];
            Console.WriteLine($"Selected tool: {selectedTool.Name}. Executing");
            var executeStopwatch = new Stopwatch();
            executeStopwatch.Start();
            ASCIIArt.SetStatus($"Executing {selectedTool.Name}");
            try
            {
                Settings.Build = gameParser.getGameData().productBuild;
                selectedTool.Execute(game);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Console.ReadKey();
            }
            executeStopwatch.Stop();
            Console.Clear();
            ASCIIArt.DrawArt();
            Console.WriteLine($"Execution of {selectedTool.Name} finished in {executeStopwatch.Elapsed.TotalSeconds} seconds");
        }
        else
        {
            bool foundarg = false;
            foreach (var s in args)
            {
                if (foundarg)
                {
                    int tool = -1;
                    for (int i = 0; i < availableTools.Count; i++)
                        if (availableTools[i].Name == s)
                            tool = i;

                    if (tool == -1)
                        break;

                    IFusionTool selectedTool = availableTools[tool];
                    Console.WriteLine($"Selected tool: {selectedTool.Name}. Executing");
                    var executeStopwatch = new Stopwatch();
                    executeStopwatch.Start();
                    ASCIIArt.SetStatus($"Executing {selectedTool.Name}");
                    try
                    {
                        Settings.Build = gameParser.getGameData().productBuild;
                        selectedTool.Execute(game);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        Console.ReadKey();
                    }
                    executeStopwatch.Stop();
                    Console.Clear();
                    ASCIIArt.DrawArt();
                    Console.WriteLine($"Execution of {selectedTool.Name} finished in {executeStopwatch.Elapsed.TotalSeconds} seconds");
                }
                else if (s == "-tool")
                    foundarg = true;
            }
            didToolArg = true;
        }
        if (args.Contains("-closeonfinish"))
            Environment.Exit(0);
        goto SELECT_TOOL;
    }
}