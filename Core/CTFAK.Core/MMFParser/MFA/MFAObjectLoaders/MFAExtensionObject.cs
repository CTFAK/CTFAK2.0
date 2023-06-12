using System.Diagnostics;
using CTFAK.Memory;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFAExtensionObject : MFAAnimationObject
{
    public byte[] ExtensionData;
    public int ExtensionId;
    public string ExtensionName;
    public int ExtensionPrivate;
    public int ExtensionType;
    public int ExtensionVersion;
    public string Filename;
    public uint Magic;
    public string SubType;

    public override void Read(ByteReader reader)
    {
        base.Read(reader);
        ExtensionType = reader.ReadInt32();
        if (ExtensionType == -1)
        {
            ExtensionName = reader.AutoReadUnicode();
            Filename = reader.AutoReadUnicode();
            Magic = reader.ReadUInt32();
            SubType = reader.AutoReadUnicode();
        }

        var newReader = new ByteReader(reader.ReadBytes((int)reader.ReadUInt32()));
        var dataSize = newReader.ReadInt32() - 20;
        reader.ReadInt32();
        //Trace.Assert(reader.ReadInt32()==-1,"Extension magic is not equal to -1");
        
        ExtensionVersion = newReader.ReadInt32();
        ExtensionId = newReader.ReadInt32();
        ExtensionPrivate = newReader.ReadInt32();
        ExtensionData = newReader.ReadBytes(dataSize);
    }

    public override void Write(ByteWriter writer)
    {
        _isExt = true;
        base.Write(writer);
        if (ExtensionType == -1)
        {
            writer.AutoWriteUnicode(ExtensionName);
            writer.AutoWriteUnicode(Filename);
            writer.WriteUInt32(Magic);
            writer.AutoWriteUnicode(SubType);
        }

        if (ExtensionData == null) ExtensionData = new byte[] { 0 };
        writer.WriteInt32(ExtensionData.Length + 20);
        writer.WriteInt32(ExtensionData.Length + 20);
        writer.WriteInt32(-1);
        writer.WriteInt32(ExtensionVersion);
        writer.WriteInt32(ExtensionId);
        writer.WriteInt32(ExtensionPrivate);
        writer.WriteBytes(ExtensionData);
    }
}