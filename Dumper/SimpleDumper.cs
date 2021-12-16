using CTFAK.FileReaders;
using CTFAK.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            foreach (var pair in images)
            {
                var image = pair.Value;

                var bmp = image.bitmap;
                bmp.Save($"Dumps\\{outPath}\\Images\\{image.Handle}.png");
            }
        }
    }
    public class SoundDumper : IFusionTool
    {
        public string Name => "Sound Dumper";

        public void Execute(IFileReader reader)
        {
            var sounds = reader.getGameData().Sounds.Items;
            var outPath = Path.GetFileNameWithoutExtension(reader.getGameData().targetFilename) ?? "dummyGame";
            Directory.CreateDirectory($"Dumps\\{outPath}\\Sounds");
            foreach (var snd in sounds)
            {

                    File.WriteAllBytes($"Dumps\\{outPath}\\Sounds\\{Utils.ClearName(snd.Name)}.wav", snd.Data);

            }
        }
    }
}
