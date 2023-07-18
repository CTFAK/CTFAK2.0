using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.MFA;

namespace CTFAK.Core.CCN.Chunks.Objects
{
    public class SubApplication : ChunkLoader
    {
        public int odCx;
        public int odCy;
        public short odVersion;
        public short odNStartFrame;
        public int odOptions;
        public string odName;

        public override void Read(ByteReader reader)
        {
            odCx = reader.ReadInt32();
            odCy = reader.ReadInt32();
            odVersion = reader.ReadInt16();
            odNStartFrame = reader.ReadInt16();
            odOptions = reader.ReadInt32();
            //odName = reader.ReadYuniversal();
        }

        public override void Write(ByteWriter Writer)
        {

        }
    }
}