using CTFAK.FileReaders;
using CTFAK.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CTFAK.Utils;
using CTFAK.Memory;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

namespace Dumper
{
    public class AutoDumper : IFusionTool
    {
        public int[] Progress = new int[] { };
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
            Logger.Log("Packed Data dumping done");
        }
    }
    public class ImageDumper : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Image Dumper";

        public void Execute(IFileReader reader)
        {
            var images = reader.getGameData().Images.Items;
            var outPath = reader.getGameData().name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            Directory.CreateDirectory($"Dumps\\{outPath}\\Images");
            Task[] tasks = new Task[images.Count];
            int i = 0;
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
            foreach (var item in tasks)
            {
                item.Wait();
            }
        }
    }

    public class SoundDumper : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Sound Dumper";
        public static string[] MODSignatures = { "2CHN","M.K.","6CHN","8CHN","10CH","12CH","14CH","16CH",
                                                 "18CH","20CH","22CH","24CH","26CH","28CH","30CH","32CH",
                                                 "M!K!", "FLT4", "FLT4", "OCTA" };
        public static string getExtension(byte[] data)
        {
            if (data.Length < 0x4) return ".bin"; // < 0x4 bytes? Not an audio format!

            // Common formats
            if (data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F') // WAVE PCM
                return ".wav"; // Only one of supported formats has the RIFF chunk. No need to check the "WAVE" flag.
            if (data[0] == 'O' && data[1] == 'g' && data[2] == 'g' && data[3] == 'S') // OGG VORBIS
                return ".ogg";
            if (data[0] == 'F' && data[1] == 'O' && data[2] == 'R' && data[3] == 'M') // AIFF 
                return ".aiff";
            if (data[0] == 'I' && data[1] == 'D' && data[2] == '3') // MP3
                return ".mp3";

            if (data.Length < 0x2C) return ".bin"; // Probably not an audio file...

            // Module formats (.MOD, .XM, .S3M etc)
            if (data[0] == 'I' && data[1] == 'M' && data[2] == 'P' && data[3] == 'M') // Impulse tracker module files (.it)
                return ".it";
            var str = System.Text.Encoding.Default.GetString(data, 0, 17);
            if (str == "Extended Module: ") // EXTENDED MODULE (FastTracker II)
                return ".xm";
            str = System.Text.Encoding.Default.GetString(data, 0x438, 4);
            foreach (var s in MODSignatures) // AMIGA MOD FILES (ProTracker/NoiseTracker/FastTracker II/etc...)
            {
                if (s == str) return ".mod";
            }

            if (data[0x2C] == 'S' && data[0x2D] == 'C' && data[0x2E] == 'R' && data[0x2F] == 'M') // ScreamTracker III module file. (.s3m)
                return ".s3m";

            // Because of Clickteam stole the MOD replayer from open-source OpenMPT library, there's more file formats that can be supported by modflt.sft.
            // 
            return ".wav";
        }

        public void Execute(IFileReader reader)
        {
            var sounds = reader.getGameData().Sounds.Items;
            var outPath = reader.getGameData().name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            Directory.CreateDirectory($"Dumps\\{outPath}\\Sounds");
            int soundint = 0;
            foreach (var snd in sounds)
            {
                File.WriteAllBytes($"Dumps\\{outPath}\\Sounds\\{Utils.ClearName(snd.Name)}{getExtension(snd.Data)}", snd.Data);
                soundint++;
                Progress = new int[2] { soundint, sounds.Count };
            }
        }
    }
    public class PackedDumper : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Packed Data Dumper";

        public void Execute(IFileReader reader)
        {
            var binarydata = reader.getGameData().binaryFiles.files;
            var packdata = reader.getGameData().packData.Items;
            var outPath = reader.getGameData().name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            if (binarydata.Count == 0 && packdata.Count == 0)
            {
                Logger.Log("No Packed Data found.");
                return;
            }

            int packedint = 0;
            if (binarydata.Count > 0)
            {
                Directory.CreateDirectory($"Dumps\\{outPath}\\Packed Data\\Binary Data");
                foreach (var pack in binarydata)
                {
                    File.WriteAllBytes($"Dumps\\{outPath}\\Packed Data\\Binary Data\\{Path.GetFileNameWithoutExtension(pack.name + ".exe")}", pack.data);
                    packedint++;
                    Progress = new int[2] { packedint, binarydata.Count + packdata.Count };
                }
            }
            if (packdata.Count > 0)
            {
                foreach (var pack in packdata)
                {
                    string dir = $"Dumps\\{outPath}\\Packed Data\\Pack Data\\";
                    if (Path.GetExtension(pack.PackFilename) == ".mfx")
                        dir += "Extensions\\";
                    else if (Path.GetExtension(pack.PackFilename) == ".dll")
                        dir += "Libraries\\";
                    else if (Path.GetExtension(pack.PackFilename) == ".ift" || Path.GetExtension(pack.PackFilename) == ".sft")
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
    }
}
