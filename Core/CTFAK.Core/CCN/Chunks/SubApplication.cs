using CTFAK.Memory;

namespace CTFAK.CCN.Chunks.Objects;

public class SubApplication : ChunkLoader
{
    public int odCx; // Size (ignored)
    public int odCy;
    public string odName;
    public short odNStartFrame;
    public int odOptions; // Options
    public short odVersion; // 0


    //Sub App stuff by -liz
    //This is stolen from XNA exporter and is absolutely useless

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