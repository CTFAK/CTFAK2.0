using System.Drawing;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks.Frame;

public class Transition : ChunkLoader
{
    public Color Color;
    public int Duration;
    public int Flags;
    public string Module;
    public string ModuleFile;
    public string Name;
    public byte[] ParameterData;

    public override void Read(ByteReader reader)
    {
        var currentPos = reader.Tell();
        Module = reader.ReadAscii(4); // 0
        Name = reader.ReadAscii(4); // 4
        Duration = reader.ReadInt32(); // 8
        Flags = reader.ReadInt32(); //12
        Color = reader.ReadColor(); //16
        var nameOffset = reader.ReadInt32(); // 20
        var parameterOffset = reader.ReadInt32(); // 24
        var parameterSize = reader.ReadInt32(); // 28
        reader.Seek(currentPos + nameOffset);
        ModuleFile = reader.ReadUniversal();
        reader.Seek(currentPos + parameterOffset);
        ParameterData = reader.ReadBytes(parameterSize);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteAscii(Module);
        writer.WriteAscii(Name);
        writer.WriteInt32(Duration);
        writer.WriteInt32(Flags);
        writer.WriteColor(Color);
        var offsets = writer.Tell();
        writer.WriteInt32(0);
        writer.WriteInt32(0);
        writer.WriteInt32(ParameterData.Length);
        var namePos = writer.Tell();
        writer.WriteUnicode(ModuleFile);
        var dataPos = writer.Tell();
        writer.WriteBytes(ParameterData);
        var end = writer.Tell();
        writer.Seek(offsets);
        writer.WriteInt32((int)namePos);
        writer.WriteInt32((int)dataPos);
        writer.Seek(end);
    }
}