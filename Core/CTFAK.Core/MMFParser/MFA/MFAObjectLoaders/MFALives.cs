using System.Collections.Generic;
using CTFAK.Memory;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFALives : ObjectLoader
{
    public int DisplayType;
    public int Flags;
    public int Font;
    public int Height;
    public List<int> Images = new();
    public uint Player;
    public int Width;

    public override void Read(ByteReader reader)
    {
        base.Read(reader);
        Player = reader.ReadUInt32();
        Images = new List<int>();
        var imgCount = reader.ReadInt32();
        for (var i = 0; i < imgCount; i++) Images.Add(reader.ReadInt32());

        DisplayType = reader.ReadInt32();
        Flags = reader.ReadInt32();
        Font = reader.ReadInt32();
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        base.Write(writer);
        writer.WriteInt32((int)Player);
        if (!CTFAKCore.Parameters.Contains("-noimgs"))
        {
            writer.WriteInt32(Images.Count);
            foreach (var i in Images) writer.WriteInt32(i);
        }
        else
        {
            writer.WriteInt32(0);
        }

        writer.WriteInt32(DisplayType);
        writer.WriteInt32(Flags);
        writer.WriteInt32(Font);
        writer.WriteInt32(Width);
        writer.WriteInt32(Height);
    }
}