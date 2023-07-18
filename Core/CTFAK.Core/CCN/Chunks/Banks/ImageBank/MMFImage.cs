using CTFAK.Memory;
using System;

namespace CTFAK.Core.CCN.Chunks.Banks.ImageBank
{
    public class MMFImage : FusionImage 
    {
        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt32();

            var decompressedReader = new ByteReader(Decompressor.DecompressOld(reader));

            Checksum = decompressedReader.ReadInt16();
            references = decompressedReader.ReadInt32();

            var dataSize = decompressedReader.ReadInt32();
            Width = decompressedReader.ReadInt16();
            Height = decompressedReader.ReadInt16();
            GraphicMode = decompressedReader.ReadByte();
            Flags.flag = decompressedReader.ReadByte();

            HotspotX = decompressedReader.ReadInt16();
            HotspotY = decompressedReader.ReadInt16();
            ActionX = decompressedReader.ReadInt16();
            ActionY = decompressedReader.ReadInt16();
            Console.WriteLine($"{Handle} - {GraphicMode}");
        }
    }
}
