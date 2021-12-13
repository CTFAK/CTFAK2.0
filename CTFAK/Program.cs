using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Utils;
using Joveler.Compression.ZLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            string libPath = Path.Combine(arch, "zlibwapi.dll");

            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

            ZLibInit.GlobalInit(libPath);

            var reader = new ByteReader(@"D:\fnaf\FiveNightsAtFreddys2.exe",System.IO.FileMode.Open);
            //var reader = new ByteReader(@"D:\fnaf\sl.exe", System.IO.FileMode.Open);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            gameParser = new ExeFileReader();
            gameParser.LoadGame(reader);
            stopwatch.Stop();
            Logger.Log($"Finished in {stopwatch.Elapsed.Seconds} seconds", true, ConsoleColor.Green);
            Console.ReadKey();

        }
    }
}
