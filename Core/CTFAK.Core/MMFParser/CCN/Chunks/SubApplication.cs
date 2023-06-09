using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks;

public class SubApplication : ChunkLoader
{
    public int OdCx; // Size (ignored)
    public int OdCy;
    public string OdName;
    public short OdNStartFrame;
    public int OdOptions; // Options
    public short OdVersion; // 0


    //Sub App stuff by -liz
    //This is stolen from XNA exporter and is absolutely useless

    //Revisiting this a month later. I agree with myself from the past.
    //This is garbage and I should seriously rewrite this
    //TODO: Rewrite sub-applications

    public override void Read(ByteReader reader)
    {
        OdCx = reader.ReadInt32();
        OdCy = reader.ReadInt32();
        OdVersion = reader.ReadInt16();
        OdNStartFrame = reader.ReadInt16();
        OdOptions = reader.ReadInt32();
        //odName = reader.ReadUniversal();
    }

    public override void Write(ByteWriter writer)
    {
    }
}