using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;

namespace CTFAK.Core.MFA.MFAObjectLoaders
{
    public class MFASubApplication : ObjectLoader
    {
        public string fileName;
        public int width;
        public int height;
        public int flaggyflag;
        public int frameNum;
        public override void Read(ByteReader reader)
        {
            base.Read(reader);
            reader.ReadInt32();
        }


        public override void Write(ByteWriter Writer)
        {
            base.Write(Writer);
            Writer.AutoWriteUnicode(fileName);
            Writer.WriteInt32(width);
            Writer.WriteInt32(height);
            Writer.WriteInt32(flaggyflag);
            Writer.WriteInt32(frameNum);
            //Writer.WriteInt32(-1);
        }
    }
}