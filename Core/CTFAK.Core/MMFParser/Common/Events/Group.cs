using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Events;

public class Group : ParameterCommon
{
    private const string GroupWords = "mqojhm:qskjhdsmkjsmkdjhq\u0063clkcdhdlkjhd";
    public ushort Flags;
    public ushort Id;
    public string Name;
    public long Offset;
    public int Password;
    public byte[] Unk1;
    public byte[] Unk2;

    private static short WrapSingleChar(short value)
    {
        value = (short)(value & 0xFF);
        if (value > 127) value -= 256;

        return value;
    }

    public static int GenerateChecksum(string name, string pass)
    {
        var v4 = 0x3939;
        foreach (var c in name) v4 += Convert.ToInt16(c) ^ 0x7FFF;

        var v5 = 0;
        foreach (var c in pass)
        {
            v4 += WrapSingleChar((short)(Convert.ToInt16(GroupWords[v5]) + (Convert.ToInt16(c) ^ 0xC3))) ^ 0xF3;
            v5++;
            if (v5 > GroupWords.Length)
                v5 = 0;
        }

        return v4;
    }

    public override void Read(ByteReader reader)
    {
        Offset = reader.Tell() - 24;
        Flags = reader.ReadUInt16();
        Id = reader.ReadUInt16();
        Name = reader.ReadWideString();
        if (Settings.Build >= 293) Name = "Group " + Id;
        Unk1 = reader.ReadBytes(190 - Name.Length * 2);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt16(Flags);
        writer.WriteUInt16(Id);
        writer.WriteUnicode(Name, true);
        writer.WriteBytes(Unk1);

        Password = GenerateChecksum(Name, "");
        writer.WriteInt32(Password);
        writer.WriteInt16(0);
    }

    public override string ToString()
    {
        return $"Group: {Name}";
    }
}