using CTFAK.Memory;
using CTFAK.MFA;

namespace CTFAK.CCN.Chunks.Objects
{
    public class SubApplication : ChunkLoader
    {
        public int odCx;                        // Size (ignored)
        public int odCy;
        public short odVersion;                    // 0
        public short odNStartFrame;
        public int odOptions;                    // Options
        public string odName;


        //Sub App stuff by -liz

        public override void Read(ByteReader reader)
        {
            odCx = reader.ReadInt32();
            odCy = reader.ReadInt32();
            odVersion = reader.ReadInt16();
            odNStartFrame = reader.ReadInt16();
            odOptions = reader.ReadInt32();
            //odName = reader.ReadUniversal();
        }

        public override void Write(ByteWriter Writer)
        {

        }
    }
}