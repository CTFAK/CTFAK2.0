using System.Diagnostics;
using System.Net;
using System.Reflection;
using CTFAK.FileReaders;
using CTFAK.Tools;
using CTFAK.Utils;
using SimpleCLI;
using SimpleCLI.Controls;

namespace CTFAK.GUI.CLI;

public static class MainInterface
{
    private static readonly string art =
        @" ____  _____  _____ ____  _  __   ____   _____ " + "\n" +
        @"/   _\/__ __\/    //  _ \/ |/ /  /_   \  \__  \" + "\n" +
        @"|  /    / \  |  __\| / \||   /    /   /    /  |" + "\n" +
        @"|  \__  | |  | |   | |-|||   \   /   /___ _\  |" + "\n" +
        @"\____/  \_/  \_/   \_/ \|\_|\_\  \____/\//____/";

    public static CliLabel dateErrorCliLabel;
    public static void AddHeader(CliWindow wnd)
    {
        wnd.Controls.Add(new CliLabel(art,ConsoleColor.DarkYellow));
        wnd.Controls.Add(new CliLabel("by 1987kostya and Yunivers",ConsoleColor.DarkMagenta));
        wnd.Controls.Add(new CliSeparator(1));
        if (dateErrorCliLabel != null)
        {
            wnd.Controls.Add(dateErrorCliLabel);
        }

    }
    public static IFileReader CurrentReader;
    public static void ExecuteTool(IFusionTool tool)
    {
        try
        {
            tool.Execute(CurrentReader);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Error while executing "+tool.Name);
            Logger.LogWarning(ex);
            Logger.LogWarning("Press any key to continue...");
            Console.ReadKey();
        }
    }

    public static void ValidateBuildTime()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attr = assembly.GetCustomAttributes().First(
                    a => a.GetType() == typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;
            var versionText = attr.InformationalVersion.Split("+")[1];
            var time = long.Parse(versionText);
            using (var wc = new WebClient())
            {
                var data = long.Parse(wc.DownloadString("https://kostyaslair.com/ctfak/latestbuildid"));
                if (data != time)
                {
                    dateErrorCliLabel =
                        new CliLabel(
                            $"Warning: You are not using the latest released build of CTFAK. Latest ID: {data}. Your ID: {time}\nSupport will not be provided. Please update\n",
                            ConsoleColor.Red);
                }
            }
        }
        catch
        {
            Logger.LogWarning("Error while accessing the version API. Running anyways...");
        }


    }
    public static void Start(string[] args)
    {
        CTFAKCore.Init();
        Directory.CreateDirectory("Plugins");
        ValidateBuildTime();
        var types = Assembly.GetAssembly(typeof(ExeFileReader)).GetTypes();

        var toolList = new List<IFusionTool>();
        foreach (var rawType in types)
            if (rawType.GetInterface(typeof(IFusionTool).FullName) != null)
                toolList.Add((IFusionTool)Activator.CreateInstance(rawType));
        foreach (var item in Directory.GetFiles("Plugins", "*.dll"))
        {
            var newAsm = Assembly.LoadFrom(Path.GetFullPath(item));
            foreach (var pluginType in newAsm.GetTypes())
                if (pluginType.GetInterface(typeof(IFusionTool).FullName) != null)
                    toolList.Add((IFusionTool)Activator.CreateInstance(pluginType));
        }
        
        if (args.Length > 1)
        {
            var sw = new StreamWriter(new FileStream("debug.log",FileMode.Create));
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            List<string> pluginNames = new List<string>();
            for (int i = 2; i < args.Length; i++)
            {
                pluginNames.Add(args[i]);
            }
            var initialPath = args[1];
            if (File.Exists(initialPath))
            {
                CurrentReader = new AutoFileReader();
                CurrentReader.LoadGame(initialPath);
                foreach (var name in pluginNames)
                {
                    var plugin = toolList.FirstOrDefault(a => a.GetType().Name == name);
                    if (plugin != null)
                    {
                        ExecuteTool(plugin);
                    }
                }
                sw.WriteLine($"=====GAME: {CurrentReader.GetGameData().Name}======");
                foreach (var err in Logger.errors)
                {
                    sw.WriteLine(err);
                }
            }
            else if (Directory.Exists(initialPath))
            {
                var games = Directory.GetFiles(initialPath);
                foreach (var path in games)
                {
                    Logger.errors.Clear();
                    CurrentReader = new AutoFileReader();
                    CurrentReader.LoadGame(path);
                    foreach (var name in pluginNames)
                    {
                        var plugin = toolList.FirstOrDefault(a => a.GetType().Name == name);
                        if (plugin != null)
                        {
                            ExecuteTool(plugin);
                        }
                    }
                    sw.WriteLine($"=====GAME: {CurrentReader.GetGameData().Name}======");
                    foreach (var err in Logger.errors)
                    {
                        sw.WriteLine(err);
                    }
                    
                }
            }
            stopWatch.Stop();
            sw.WriteLine($"Total time: {stopWatch.Elapsed.TotalSeconds}");
            sw.Flush();
            sw.Close();
        }
        var mainCliWindow = new CliWindow();
        var inspectorCliWindow = new CliWindow();
        var fileOptionsCliWindow = new CliWindow();
        var pluginsCliWindow = new CliWindow();
        AddHeader(mainCliWindow);
        AddHeader(fileOptionsCliWindow);
        AddHeader(inspectorCliWindow);
        AddHeader(pluginsCliWindow);

        
        
        
        

        var pluginText = new CliLabel("Select a plugin");
        var pluginStopwatch = new Stopwatch();
        pluginsCliWindow.Controls.Add(pluginText);
        pluginsCliWindow.Controls.Add(new CliSeparator(1));
        var isAndroid = new CliCheckbox("Use Android");
        var traceChunks = new CliCheckbox("Trace Chunks");
        var noicons = new CliCheckbox("No Icons");
        var noimgs = new CliCheckbox("No Images");

        foreach (var tool in toolList)
        {
            pluginsCliWindow.Controls.Add(new CliButton($"{tool.Name}",() =>
            {
                pluginStopwatch.Start();
                ExecuteTool(tool);
                pluginStopwatch.Stop();
                pluginText.Text = $"Execution of {tool.Name} finished in {pluginStopwatch.Elapsed.TotalSeconds} seconds";
            }));
        }
        pluginsCliWindow.Controls.Add(new CliButton("Back",(() => {CliWindow.Show(inspectorCliWindow);})));
        
        mainCliWindow.Controls.Add(new HorizontalLayout());
        
        mainCliWindow.Controls.Add(new CliButton("Load game",(() =>
        {
            string path = "";
            while (!File.Exists(path))
            {
                Console.Write("Enter the path to the game file: ");
                path = Console.ReadLine().Trim('"');
                if(!File.Exists(path))
                    Console.WriteLine("File doesn't exist");
            }
            
            fileOptionsCliWindow.Controls.Add(new CliLabel("Select file reader: "));
            fileOptionsCliWindow.Controls.Add(new CliSeparator(1));
            var readerStopwatch = new Stopwatch();
            List<IFileReader> readers = new List<IFileReader>();
            foreach (var rawType in types)
                if (rawType.GetInterface(typeof(IFileReader).FullName) != null)
                {
                   readers.Add((IFileReader)Activator.CreateInstance(rawType));
                   
                }
            readers = readers.OrderBy(a=>a.Priority).ToList();
            foreach (var reader in readers)
            {
                
                fileOptionsCliWindow.Controls.Add(new CliButton($"Read as {reader.Name}",((key) =>
                {
                    CTFAKCore.CurrentReader = reader;
                    if (key == ConsoleKey.D)
                        CTFAKCore.Parameters += "-debug";
                    if (isAndroid.Activated)
                        CTFAKCore.Parameters += "-android ";
                    if (traceChunks.Activated)
                        CTFAKCore.Parameters += "-trace_chunks ";
                    if (noicons.Activated)
                        CTFAKCore.Parameters += "-noicons ";
                    if (noimgs.Activated)
                        CTFAKCore.Parameters += "-noimg ";
                    
                    
                    CurrentReader = reader;
                    readerStopwatch.Start();
                    CurrentReader.LoadGame(path);
                    readerStopwatch.Stop();
                        
                    // Filling inspector with some data
                    var game = reader.GetGameData();
                    inspectorCliWindow.Controls.Add(new CliLabel($"Reading finished in {readerStopwatch.Elapsed.TotalSeconds}"));
                    inspectorCliWindow.Controls.Add(new CliSeparator(1));
                        
                    // Basic inspector info
                    inspectorCliWindow.Controls.Add(new CliLabel($"Game name: {game.Name}"));
                    inspectorCliWindow.Controls.Add(new CliLabel($"Game Author: {game.Author}"));
                    inspectorCliWindow.Controls.Add(new CliLabel($"Fusion Build: {Settings.Build}"));
                    inspectorCliWindow.Controls.Add(new CliLabel($"Number of Frames: {game.Frames.Count}"));
                    inspectorCliWindow.Controls.Add(new CliLabel($"Number of Images: {game.Images.Items.Count}"));
                        
                    inspectorCliWindow.Controls.Add(new CliSeparator(1));

                    inspectorCliWindow.Controls.Add(new HorizontalLayout());
                        
                    // Inspector CliButtons
                    inspectorCliWindow.Controls.Add(new CliButton("Plugins",()=>{CliWindow.Show(pluginsCliWindow);}));
                        
                    inspectorCliWindow.Controls.Add(new CliButton("Pack data",()=>{}));
                    inspectorCliWindow.Controls.Add(new CliButton("Frame data",()=>{}));
                    inspectorCliWindow.Controls.Add(new CliButton("Exit",()=>{Environment.Exit(0);}));
                        
                        
                        
                    CliWindow.Show(inspectorCliWindow);
                })));
                
            }
            fileOptionsCliWindow.Controls.Add(isAndroid);
            fileOptionsCliWindow.Controls.Add(traceChunks);
            fileOptionsCliWindow.Controls.Add(noicons);
            fileOptionsCliWindow.Controls.Add(noimgs);
            CliWindow.Show(fileOptionsCliWindow);
                    
        })));
        mainCliWindow.Controls.Add(new CliSeparator(1));

        mainCliWindow.Controls.Add(new CliButton("Load chunk data",(() => {})));
        mainCliWindow.Start();
    }
}