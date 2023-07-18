using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.Core.CCN.Chunks.Banks.ImageBank
{
    public class AndroidImage : FusionImage
    {
        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt16();
            if (Settings.BuildType == 284)
                Handle--;
            GraphicMode = (byte)reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            HotspotX = reader.ReadInt16();
            HotspotY = reader.ReadInt16();
            ActionX = reader.ReadInt16();
            ActionY = reader.ReadInt16();
            Flags.flag = 16;
            var dataSize = reader.ReadInt32();

            if (reader.PeekByte() == 255)
                imageData = reader.ReadBytes(dataSize);
            else
                imageData = Decompressor.DecompressBlock(reader, dataSize);
        }
    }
}
