using System.Diagnostics;
using System.Net;
using System.Reflection;
using CTFAK;
using CTFAK.FileReaders;
using CTFAK.Tools;
using CTFAK.Utils;
using SimpleCLI;
using SimpleCLI.Controls;

public class Program
{
    private static readonly string art =
        @" ____  _____  _____ ____  _  __   ____    ____ " + "\n" +
        @"/   _\/__ __\/    //  _ \/ |/ /  /_   \  /  _ \" + "\n" +
        @"|  /    / \  |  __\| / \||   /    /   /  | / \|" + "\n" +
        @"|  \__  | |  | |   | |-|||   \   /   /___| \_/|" + "\n" +
        @"\____/  \_/  \_/   \_/ \|\_|\_\  \____/\/\____/";

    public static Label dateErrorLabel;
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
                    dateErrorLabel =
                        new Label(
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
    public static void AddHeader(Window wnd)
    {
        wnd.Controls.Add(new Label(art,ConsoleColor.DarkYellow));
        wnd.Controls.Add(new Label("by 1987kostya and Yunivers",ConsoleColor.DarkMagenta));
        wnd.Controls.Add(new Separator(1));
        if (dateErrorLabel != null)
        {
            wnd.Controls.Add(dateErrorLabel);
        }

    }

    public static IFileReader currentReader;

    public static void ExecuteTool(IFusionTool tool)
    {
        try
        {
            tool.Execute(currentReader);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Error while executing "+tool.Name);
            Logger.LogWarning(ex);
            Logger.LogWarning("Press any key to continue...");
            Console.ReadKey();
        }
    }
    public static void Main(string[] args)
    {
        CTFAKCore.Init();
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
        
        if (args.Length > 0)
        {
            var sw = new StreamWriter(new FileStream("debug.log",FileMode.Create));
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            List<string> pluginNames = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                pluginNames.Add(args[i]);
            }
            var initialPath = args[0];
            if (File.Exists(initialPath))
            {
                currentReader = new AutoFileReader();
                currentReader.LoadGame(initialPath);
                foreach (var name in pluginNames)
                {
                    var plugin = toolList.FirstOrDefault(a => a.GetType().Name == name);
                    if (plugin != null)
                    {
                        ExecuteTool(plugin);
                    }
                }
                sw.WriteLine($"=====GAME: {currentReader.GetGameData().Name}======");
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
                    currentReader = new AutoFileReader();
                    currentReader.LoadGame(path);
                    foreach (var name in pluginNames)
                    {
                        var plugin = toolList.FirstOrDefault(a => a.GetType().Name == name);
                        if (plugin != null)
                        {
                            ExecuteTool(plugin);
                        }
                    }
                    sw.WriteLine($"=====GAME: {currentReader.GetGameData().Name}======");
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
        var mainWindow = new Window();
        var inspectorWindow = new Window();
        var fileOptionsWindow = new Window();
        var pluginsWindow = new Window();
        AddHeader(mainWindow);
        AddHeader(fileOptionsWindow);
        AddHeader(inspectorWindow);
        AddHeader(pluginsWindow);

        
        
        
        

        var pluginText = new Label("Select a plugin");
        var pluginStopwatch = new Stopwatch();
        pluginsWindow.Controls.Add(pluginText);
        pluginsWindow.Controls.Add(new Separator(1));
        var isAndroid = new Checkbox("Use Android");
        var traceChunks = new Checkbox("Trace Chunks");
        var noicons = new Checkbox("No Icons");
        var noimgs = new Checkbox("No Images");

        foreach (var tool in toolList)
        {
            pluginsWindow.Controls.Add(new Button($"{tool.Name}",() =>
            {
                pluginStopwatch.Start();
                ExecuteTool(tool);
                pluginStopwatch.Stop();
                pluginText.Text = $"Execution of {tool.Name} finished in {pluginStopwatch.Elapsed.TotalSeconds} seconds";
            }));
        }
        pluginsWindow.Controls.Add(new Button("Back",(() => {Window.Show(inspectorWindow);})));
        
        mainWindow.Controls.Add(new HorizontalLayout());
        
        mainWindow.Controls.Add(new Button("Load game",(() =>
        {
            string path = "";
            while (!File.Exists(path))
            {
                Console.Write("Enter the path to the game file: ");
                path = Console.ReadLine().Trim('"');
                if(!File.Exists(path))
                    Console.WriteLine("File doesn't exist");
            }
            
            fileOptionsWindow.Controls.Add(new Label("Select file reader: "));
            fileOptionsWindow.Controls.Add(new Separator(1));
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
                
                fileOptionsWindow.Controls.Add(new Button($"Read as {reader.Name}",((key) =>
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
                    
                    
                    currentReader = reader;
                    readerStopwatch.Start();
                    currentReader.LoadGame(path);
                    readerStopwatch.Stop();
                        
                    // Filling inspector with some data
                    var game = reader.GetGameData();
                    inspectorWindow.Controls.Add(new Label($"Reading finished in {readerStopwatch.Elapsed.TotalSeconds}"));
                    inspectorWindow.Controls.Add(new Separator(1));
                        
                    // Basic inspector info
                    inspectorWindow.Controls.Add(new Label($"Game name: {game.Name}"));
                    inspectorWindow.Controls.Add(new Label($"Game Author: {game.Author}"));
                    inspectorWindow.Controls.Add(new Label($"Fusion Build: {Settings.Build}"));
                    inspectorWindow.Controls.Add(new Label($"Number of Frames: {game.Frames.Count}"));
                    inspectorWindow.Controls.Add(new Label($"Number of Images: {game.Images.Items.Count}"));
                        
                    inspectorWindow.Controls.Add(new Separator(1));

                    inspectorWindow.Controls.Add(new HorizontalLayout());
                        
                    // Inspector buttons
                    inspectorWindow.Controls.Add(new Button("Plugins",()=>{Window.Show(pluginsWindow);}));
                        
                    inspectorWindow.Controls.Add(new Button("Pack data",()=>{}));
                    inspectorWindow.Controls.Add(new Button("Frame data",()=>{}));
                    inspectorWindow.Controls.Add(new Button("Exit",()=>{Environment.Exit(0);}));
                        
                        
                        
                    Window.Show(inspectorWindow);
                })));
                
            }
            fileOptionsWindow.Controls.Add(isAndroid);
            fileOptionsWindow.Controls.Add(traceChunks);
            fileOptionsWindow.Controls.Add(noicons);
            fileOptionsWindow.Controls.Add(noimgs);
            Window.Show(fileOptionsWindow);
                    
        })));
        mainWindow.Controls.Add(new Separator(1));

        mainWindow.Controls.Add(new Button("Load chunk data",(() => {})));
        mainWindow.Start();
    }
    
}