using System;
using System.Drawing;
using CTFAK.Memory;

namespace CTFAK.CCN.Chunks.Frame;

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
        Module = reader.ReadAscii(4);
        Name = reader.ReadAscii(4);
        Duration = reader.ReadInt32();
        Flags = reader.ReadInt32();
        Color = reader.ReadColor();
        var nameOffset = reader.ReadInt32();
        var parameterOffset = reader.ReadInt32();
        var parameterSize = reader.ReadInt32();
        reader.Seek(currentPos + nameOffset);
        ModuleFile = reader.ReadUniversal();
        reader.Seek(currentPos + parameterOffset);
        ParameterData = reader.ReadBytes(parameterSize);
    }

    public override void Write(ByteWriter Writer)
    {
        throw new NotImplementedException();
    }
}