using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Tools;
using CTFAK.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTFAK
{
    public class Program
    {
        public static IFileReader gameParser;
        public static string parameters;
        public static string path;

        static void Main(string[] args)
        {


            if (File.Exists(Path.GetTempPath() + "application.ccn")) File.Delete(Path.GetTempPath() + "application.ccn");


            /*var mfa = new MFAData();
            mfa.Read(new ByteReader("D:\\test.mfa", FileMode.Open));
            mfa.Write(new ByteWriter(new FileStream("ass.mfa", FileMode.Create)));
            Console.ReadKey();
            return;*/

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
            path = string.Empty;
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
            parameters = loadParams;
            
            var types = Assembly.GetExecutingAssembly().GetTypes();
            List<IFileReader> availableReaders = new List<IFileReader>();

            foreach (var rawType in types)
            {
                if (rawType.GetInterface(typeof(IFileReader).FullName) != null)
                    availableReaders.Add((IFileReader)Activator.CreateInstance(rawType));
            }
            foreach (var item in Directory.GetFiles("Plugins", "*.dll"))
            {
                var newAsm = Assembly.LoadFrom(Path.GetFullPath(item));
                foreach (var pluginType in newAsm.GetTypes())
                {
                    if (pluginType.GetInterface(typeof(IFileReader).FullName) != null)
                        availableReaders.Add((IFileReader)Activator.CreateInstance(pluginType));
                }
            }

            if (Path.GetExtension(path)==".exe")
            {
                gameParser = new ExeFileReader();
            }
            else if (Path.GetExtension(path) == ".apk")
            {
                if (File.Exists(Path.GetTempPath() + "application.ccn"))
                    File.Delete(Path.GetTempPath() + "application.ccn");
                path = new ApkFileReader().ExtractCCN(path);
                gameParser = new EXE.CCNFileReader();
            }
            else
            {
                SELECT_READER:
                Console.Clear();
                ASCIIArt.DrawArt();
                ASCIIArt.SetStatus("Selecting tool");
                Console.WriteLine($"{availableReaders.Count} tool(s) available\n\nSelect tool: ");
                Console.WriteLine("0. Exit CTFAK");
                for (int i = 0; i < availableReaders.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableReaders[i].Name}");
                }
                var key1 = Console.ReadLine();
                var readerSelect = int.Parse(key1);
                if (readerSelect == 0) Environment.Exit(0);
                gameParser = availableReaders[readerSelect - 1];
            }
            
            


                
            


            var readStopwatch = new Stopwatch();
            readStopwatch.Start();
            Console.Clear();
            ASCIIArt.DrawArt();
            ASCIIArt.SetStatus("Reading game");
            Console.WriteLine($"Reading game with \"{gameParser.Name}\"");
            gameParser.PatchMethods();
            
            
            gameParser.LoadGame(path);
            readStopwatch.Stop();
            if (Settings.twofiveplus)
            {
                Logger.LogWarning("This game uses 2.5+ and so it can't be decompiled");
                Console.ReadKey();
            }
            
            Console.Clear();
            ASCIIArt.DrawArt();
            Console.WriteLine($"Reading finished in {readStopwatch.Elapsed.TotalSeconds} seconds");
            
            


            Settings.gameType = Settings.GameType.NORMAL;
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
            Console.WriteLine($"Game Information:");
            Console.WriteLine($"Game Name: "+gameParser.getGameData().name);
            Console.WriteLine($"Author: "+gameParser.getGameData().author);
            Console.WriteLine($"FusionBuild: "+Settings.Build);
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
}
