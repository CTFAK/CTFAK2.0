using CTFAK.Memory;
using CTFAK.MMFParser.Common.Banks;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Events;

internal class Sample : ParameterCommon
{
    public int Flags;
    public int Handle;
    public string Name;

    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadInt16();
        Flags = reader.ReadUInt16();
        Name = reader.ReadUniversal();

        if (Settings.Android && Settings.Build < 289 &&
            !AndroidSoundBank.oldAndroidNames.ContainsKey(Handle))
            AndroidSoundBank.oldAndroidNames.Add(Handle, Name);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)Handle);
        writer.WriteUInt16((ushort)Flags);
        Name = Name.Replace(" ", "");
        writer.WriteUnicode(Name);
        writer.Skip(120);
        writer.WriteInt16(0);
    }

    public override string ToString()
    {
        return $"Sample '{Name}' handle: {Handle}";
    }
}