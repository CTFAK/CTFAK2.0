using CTFAK.Core.Utils;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Threading.Tasks;

namespace CTFAK.Core.CCN.Chunks.Banks.ImageBank
{
    public class NormalImage : FusionImage
    {
        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt32();
            if (Settings.Build >= 284)
                Handle--;

            var decompressedSize = reader.ReadInt32();
            var compSize = reader.ReadInt32();
            var compressedBuffer = reader.ReadBytes(compSize);
            var task = new Task(() =>
            {
                var decompressedReader = new ByteReader(Decompressor.DecompressBlock(compressedBuffer));
                Checksum = decompressedReader.ReadInt32();
                references = decompressedReader.ReadInt32();
                var dataSize = decompressedReader.ReadInt32();
                Width = decompressedReader.ReadInt16();
                Height = decompressedReader.ReadInt16();
                GraphicMode = decompressedReader.ReadByte();
                Flags.flag = decompressedReader.ReadByte();
                decompressedReader.ReadInt16();
                HotspotX = decompressedReader.ReadInt16();
                HotspotY = decompressedReader.ReadInt16();
                ActionX = decompressedReader.ReadInt16();
                ActionY = decompressedReader.ReadInt16();
                Transparent = decompressedReader.ReadColor();
                if (Flags["LZX"])
                {
                    var decompSize = decompressedReader.ReadInt32();
                    imageData = Decompressor.DecompressBlock(decompressedReader,
                        (int)(decompressedReader.Size() - decompressedReader.Tell()));
                }
                else
                {
                    imageData = decompressedReader.ReadBytes(dataSize);
                }
            });

            task.RunSynchronously();
            ImageBank.imageReadingTasks.Add(task);
        }
    }
}
