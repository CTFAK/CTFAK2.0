using CTFAK.FileReaders;
using CTFAK.MMFParser.CCN;
using Joveler.Compression.ZLib;

namespace CTFAK;

public delegate void SaveHandler(int index, int all);

public delegate void LoggerHandler(string output);

public delegate void SimpleMessage<T>(T data);

public delegate T2 SimpleMessage<T, T2>(T data);

public class CTFAKCore
{
    public static IFileReader CurrentReader;
    public static string Parameters = "";

    public static void Init()
    {
        ChunkList.Init();
        ZLibInit.GlobalInit("x64\\zlibwapi.dll");
        //var libraryFile = System.IO.Path.Combine("x64", "CTFAK-Native.dll");
        //NativeLib.LoadLibrary(libraryFile);
    }
}