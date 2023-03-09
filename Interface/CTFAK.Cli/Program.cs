using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using CTFAK;
using CTFAK.FileReaders;
using CTFAK.MMFParser.CCN;
using CTFAK.Tools;
using CTFAK.Utils;

public class Program
{
    public static IFileReader gameParser;

    public static void Main(string[] args)
    {
        CTFAKCore.Init();
        /*Settings.Build = 294;
        Settings.gameType = Settings.GameType.NORMAL;
        var newMFA = new MFAData();
        newMFA.Read(new ByteReader(new FileStream(args[0],FileMode.Open)));
        newMFA.Write(new ByteWriter(new FileStream(args[0]+".retard.mfa",FileMode.Create)));*/
        ASCIIArt.SetStatus("Idle");
        Directory.CreateDirectory("Plugins");
        Directory.CreateDirectory("Dumps");
        ASCIIArt.DrawArt2();
        ASCIIArt.DrawArt();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("by 1987kostya");
        Console.ResetColor();
        Thread.Sleep(700);
        //Console.Clear();
        ASCIIArt.DrawArt();

        ASK_FOR_PATH:
        ASCIIArt.SetStatus("Waiting for file");
        var path = string.Empty;
        if (args.Length == 0)
        {
            Console.Write("Game path: ");
            path = Console.ReadLine().Trim('"');
        }
        else
        {
            path = args[0];
        }

        if (!File.Exists(path))
        {
            Console.WriteLine("ERROR: File not found");
            goto ASK_FOR_PATH;
        }

        Console.Write("Parameters: ");
        var loadParams = Console.ReadLine();
        CTFAKCore.Parameters = loadParams;

        var types = Assembly.GetAssembly(typeof(ExeFileReader)).GetTypes();

        var availableReaders = new List<IFileReader>();

        if (Path.GetExtension(path) == ".exe")
        {
            gameParser = new ExeFileReader();
        }
        else if (Path.GetExtension(path) == ".apk")
        {
            /*if (File.Exists(Path.GetTempPath() + "application.ccn"))
                File.Delete(Path.GetTempPath() + "application.ccn");
            path = ApkFileReader.ExtractCcn(path);
            gameParser = new CCNFileReader();*/
        }
        else
        {
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
            for (var i = 0; i < availableReaders.Count; i++) Console.WriteLine($"{i + 1}. {availableReaders[i].Name}");
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

        gameParser.LoadGame(path);
        readStopwatch.Stop();

        //Console.Clear();
        ASCIIArt.DrawArt();
        Console.WriteLine($"Reading finished in {readStopwatch.Elapsed.TotalSeconds} seconds");

        var availableTools = new List<IFusionTool>();
#if RELEASE
        availableTools.Add(new Dumper.ImageDumper());
        availableTools.Add(new CTFAK.Tools.FTDecompile());
#endif
        foreach (var rawType in types)
            if (rawType.GetInterface(typeof(IFusionTool).FullName) != null)
                availableTools.Add((IFusionTool)Activator.CreateInstance(rawType));
        foreach (var item in Directory.GetFiles("Plugins", "*.dll"))
        {
            var newAsm = Assembly.LoadFrom(Path.GetFullPath(item));
            foreach (var pluginType in newAsm.GetTypes())
                if (pluginType.GetInterface(typeof(IFusionTool).FullName) != null)
                    availableTools.Add((IFusionTool)Activator.CreateInstance(pluginType));
        }

        SELECT_TOOL:
        Console.WriteLine("");
        Console.WriteLine("Game Information:");
        Console.WriteLine("Game Name: " + gameParser.GetGameData().Name);
        Console.WriteLine("Author: " + gameParser.GetGameData().Author);
        Console.WriteLine("Number of frames: " + gameParser.GetGameData().Frames.Count);
        Console.WriteLine("Fusion Build: " + Settings.Build);
        Console.WriteLine("");
        ASCIIArt.SetStatus("Selecting tool");
        Console.WriteLine($"{availableTools.Count} tool(s) available\n\nSelect tool or specify a command ");
        Console.WriteLine("0. Exit CTFAK");
        for (var i = 0; i < availableTools.Count; i++) Console.WriteLine($"{i + 1}. {availableTools[i].Name}");
        Console.WriteLine();
        Console.Write(">> ");
        var commandInput = Console.ReadLine();
        if (int.TryParse(commandInput, out var toolSelect))
        {
            if (toolSelect == 0) Environment.Exit(0);
            var selectedTool = availableTools[toolSelect - 1];
            Console.WriteLine($"Selected tool: {selectedTool.Name}. Executing");
            var executeStopwatch = new Stopwatch();
            executeStopwatch.Start();
            ASCIIArt.SetStatus($"Executing {selectedTool.Name}");
            try
            {
                selectedTool.Execute(gameParser);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Console.ReadKey();
            }

            executeStopwatch.Stop();
            //Console.Clear();

            ASCIIArt.DrawArt();
            Console.WriteLine(
                $"Execution of {selectedTool.Name} finished in {executeStopwatch.Elapsed.TotalSeconds} seconds");
        }
        else
        {
            if (commandInput == "chunkList")
            {
                Console.WriteLine($"Loaded chunk loaders: {ChunkList.KnownLoaders.Count}");
                foreach (var loader in ChunkList.KnownLoaders)
                {
                    var actualLoader = loader.Value;
                    Console.WriteLine($"Loader \"{actualLoader.ChunkName}\" for ID {actualLoader.ChunkId}");
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Command not found");
            }


            Console.Clear();

            ASCIIArt.DrawArt();
        }

        goto SELECT_TOOL;

        Console.ReadKey();
    }
}