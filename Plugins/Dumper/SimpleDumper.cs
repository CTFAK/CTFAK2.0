using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTFAK.FileReaders;
using CTFAK.Tools;
using CTFAK.Utils;

namespace Dumper;

public class AutoDumper : IFusionTool
{
    public int[] Progress = { };
    int[] IFusionTool.Progress => Progress;
    public string Name => "Dump Everything";

    public void Execute(IFileReader reader)
    {
        Logger.Log("Dumping images...");
        new ImageDumper().Execute(reader);
        Logger.Log("Image dumping done");

        Logger.Log("Dumping sounds...");
        new SoundDumper().Execute(reader);
        Logger.Log("Sound dumping done");

        Logger.Log("Dumping packed data...");
        new PackedDumper().Execute(reader);
        Logger.Log("packed data dumping done");
    }
}

public class ImageDumper : IFusionTool
{
    public int[] Progress = { };
    int[] IFusionTool.Progress => Progress;
    public string Name => "Image Dumper";

    public void Execute(IFileReader reader)
    {
        var images = reader.GetGameData().Images.Items;
        var outPath = reader.GetGameData().Name ?? "Unknown Game";
        var rgx = new Regex("[^a-zA-Z0-9 -]");
        outPath = rgx.Replace(outPath, "").Trim(' ');
        Directory.CreateDirectory($"Dumps\\{outPath}\\Images");
        var tasks = new Task[images.Count];
        var i = 0;
        foreach (var image in images.Values)
        {
            var newTask = new Task(() =>
            {
                var bmp = image.bitmap;
                bmp.Save($"Dumps\\{outPath}\\Images\\{image.Handle}.png");
            });
            tasks[i] = newTask;
            newTask.Start();
            i++;
            Progress = new int[2] { i, images.Count };
        }

        foreach (var item in tasks) item.Wait();
    }
}

public class SoundDumper : IFusionTool
{
    public int[] Progress = { };
    int[] IFusionTool.Progress => Progress;
    public string Name => "Sound Dumper";

    public void Execute(IFileReader reader)
    {
        var sounds = reader.GetGameData().Sounds.Items;
        var outPath = reader.GetGameData().Name ?? "Unknown Game";
        var rgx = new Regex("[^a-zA-Z0-9 -]");
        outPath = rgx.Replace(outPath, "").Trim(' ');
        Directory.CreateDirectory($"Dumps\\{outPath}\\Sounds");
        var soundint = 0;
        foreach (var snd in sounds)
        {
            File.WriteAllBytes($"Dumps\\{outPath}\\Sounds\\{Utils.ClearName(snd.Name)}{getExtension(snd.Data)}",
                snd.Data);
            soundint++;
            Progress = new int[2] { soundint, sounds.Count };
        }
    }

    public static string getExtension(byte[] data)
    {
        if (data[0] == 0xff || data[0] == 0x49) return ".mp3";

        return ".wav";
    }
}

public class PackedDumper : IFusionTool
{
    public int[] Progress = { };
    int[] IFusionTool.Progress => Progress;
    public string Name => "Packed Data Dumper";

    public void Execute(IFileReader reader)
    {
        var binarydata = reader.GetGameData().BinaryFiles.Files;
        var packdata = reader.GetGameData().PackData.Items;
        var outPath = reader.GetGameData().Name ?? "Unknown Game";
        var rgx = new Regex("[^a-zA-Z0-9 -]");
        outPath = rgx.Replace(outPath, "").Trim(' ');
        if (binarydata.Count == 0 && packdata.Count == 0)
        {
            Logger.Log("No Packed Data found.");
            return;
        }

        var packedint = 0;
        if (binarydata.Count > 0)
        {
            Directory.CreateDirectory($"Dumps\\{outPath}\\Packed Data\\Binary Data");
            foreach (var pack in binarydata)
            {
                File.WriteAllBytes(
                    $"Dumps\\{outPath}\\Packed Data\\Binary Data\\{Path.GetFileNameWithoutExtension(pack.Name + ".exe")}",
                    pack.Data);
                packedint++;
                Progress = new int[2] { packedint, binarydata.Count + packdata.Count };
            }
        }

        if (packdata.Count > 0)
            foreach (var pack in packdata)
            {
                var dir = $"Dumps\\{outPath}\\Packed Data\\Pack Data\\";
                if (Path.GetExtension(pack.PackFilename) == ".mfx")
                    dir += "Extensions\\";
                else if (Path.GetExtension(pack.PackFilename) == ".dll")
                    dir += "Libraries\\";
                else if (Path.GetExtension(pack.PackFilename) == ".ift" ||
                         Path.GetExtension(pack.PackFilename) == ".sft")
                    dir += "Filters\\";
                else if (Path.GetExtension(pack.PackFilename) == ".mvx")
                    dir += "Movements\\";
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(dir + Path.GetFileNameWithoutExtension(pack.PackFilename + ".exe"), pack.Data);
                packedint++;
                Progress = new int[2] { packedint, binarydata.Count + packdata.Count };
            }
    }
}