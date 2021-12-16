using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Tools;
using CTFAK.Utils;
using Joveler.Compression.ZLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK
{
    class Program
    {
        public static IFileReader gameParser;
        static void Main(string[] args)
        {
            string arch = null;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }
            string libPath = Path.Combine(arch, "zlibwapi-fast.dll");

            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

            ZLibInit.GlobalInit(libPath);


            Directory.CreateDirectory("Plugins");
            Directory.CreateDirectory("Dumps");





            ASCIIArt.DrawArt();
            ASK_FOR_PATH:
            string path = string.Empty;
            if (args.Length == 0)
            {
                Console.Write("Game path: ");
                path = Console.ReadLine();

            }
            else path = args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine("ERROR: File not found");
                goto ASK_FOR_PATH;
            }
            var readStopwatch = new Stopwatch();
            readStopwatch.Start();
            Console.Clear();
            ASCIIArt.DrawArt();
            Console.WriteLine("Reading game with default method");
            var reader = new ByteReader(path, System.IO.FileMode.Open);
            gameParser = new ExeFileReader();
            gameParser.LoadGame(reader);
            readStopwatch.Stop();
            Console.Clear();
            ASCIIArt.DrawArt();
            Console.WriteLine($"Reading finished in {readStopwatch.Elapsed.TotalSeconds} seconds");

            
            var types = Assembly.GetExecutingAssembly().GetTypes();
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
            selectedTool.Execute(gameParser);
            executeStopwatch.Stop();
            Console.Clear();
            
            ASCIIArt.DrawArt();
            Console.WriteLine($"Execution of {selectedTool.Name} finished in {executeStopwatch.Elapsed.TotalSeconds} seconds");
            goto SELECT_TOOL;

            Console.ReadKey();

        }
    }
}
