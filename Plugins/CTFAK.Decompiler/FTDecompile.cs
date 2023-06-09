
using System.IO;

using CTFAK.FileReaders;


namespace CTFAK.Tools;

public class FTDecompile : IFusionTool
{
    public string Name => "Decompiler";
    public int[] Progress { get; }
    public void Execute(IFileReader reader)
    {
        var game = reader.GetGameData();
        var outPath = Path.Join("Dumps", game.Name ?? "Unknown game",Path.GetFileNameWithoutExtension(game.EditorFilename != null && string.IsNullOrEmpty(game.EditorFilename) ? game.Name : game.EditorFilename )+".mfa");
        
    }
}