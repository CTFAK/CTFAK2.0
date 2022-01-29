using CTFAK.FileReaders;
using CTFAK.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dumper
{
    public class ImageDumper : IFusionTool
    {
        public string Name => "Image Dumper";

        public void Execute(IFileReader reader)
        {
            var images = reader.getGameData().images.Items;
            var outPath = Path.GetFileNameWithoutExtension(reader.getGameData().targetFilename) ?? "dummyGame";
            Directory.CreateDirectory($"Dumps\\{outPath}\\Images");
            Task[] tasks = new Task[images.Count];
            int i = 0;
            foreach (var image in images.Values)
            {
                
                tasks[i]= Task.Run(() =>
                {
                    var bmp = image.bitmap;
                    bmp.Save($"Dumps\\{outPath}\\Images\\{image.Handle}.png");
                });
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
            var outPath = Path.GetFileNameWithoutExtension(reader.getGameData().targetFilename) ?? "dummyGame";
            Directory.CreateDirectory($"Dumps\\{outPath}\\Sounds");
            foreach (var snd in sounds)
            {

                    File.WriteAllBytes($"Dumps\\{outPath}\\Sounds\\{Utils.ClearName(snd.Name)}{getExtension(snd.Data)}", snd.Data);

            }
        }
    }
}
