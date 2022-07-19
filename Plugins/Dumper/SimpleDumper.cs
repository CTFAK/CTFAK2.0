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

namespace Dumper
{
    public class AutoDumper : IFusionTool
    {
        public string Name => "Dump Everything";
        public void Execute(IFileReader reader)
        {
            Logger.Log("Dumping images...");
            new ImageDumper().Execute(reader);
            Logger.Log("Image dumping done");
            Logger.Log("Dumping sounds...");
            new SoundDumper().Execute(reader);
            Logger.Log("Sound dumping done");
        }
    }
    public class ImageDumper : IFusionTool
    {
        public string Name => "Image Dumper";

        public void Execute(IFileReader reader)
        {
            var images = reader.getGameData().Images.Items;
            var outPath = reader.getGameData().name ?? "Unknown Game";
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
            }
            foreach (var item in tasks)
            {
                item.Wait();
            }
        }
    }
    public class SoundDumper : IFusionTool
    {
        public string Name => "Sound Dumper";
        public static string getExtension(byte[] data)
        {
            if (data[0] == 0xff||data[0]==0x49) return ".mp3";
            
            return ".wav";
        }

        public void Execute(IFileReader reader)
        {
            var sounds = reader.getGameData().Sounds.Items;
            var outPath = reader.getGameData().name ?? "Unknown Game";
            Directory.CreateDirectory($"Dumps\\{outPath}\\Sounds");
            foreach (var snd in sounds)
            {
                File.WriteAllBytes($"Dumps\\{outPath}\\Sounds\\{Utils.ClearName(snd.Name)}{getExtension(snd.Data)}", snd.Data);
            }
        }
    }
}
