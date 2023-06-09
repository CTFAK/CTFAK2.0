using CTFAK.Memory;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFASubApplication : ObjectLoader
{
    public string FileName;
    public int Flaggyflag;
    public int FrameNum;
    public int Height;
    public int Width;

    public override void Read(ByteReader reader)
    {
        base.Read(reader);
        reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        base.Write(writer);
        writer.AutoWriteUnicode(FileName);
        writer.WriteInt32(Width);
        writer.WriteInt32(Height);
        writer.WriteInt32(Flaggyflag);
        writer.WriteInt32(FrameNum);
        //Writer.WriteInt32(-1);
    }
}