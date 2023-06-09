using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Banks;

//this might fuck up normal games, ill rewrite it in future, dw
//[ChunkLoader(26216, "SoundBank")]
public class AndroidSoundBank : ChunkLoader
{
    public static Dictionary<int, string> oldAndroidNames = new();
    public AndroidSoundItem[] Items = new AndroidSoundItem[0];

    public int NumOfHandles;
    public int NumOfItems;
    public static event SaveHandler OnSoundLoaded;

    public override void Read(ByteReader reader)
    {
        NumOfHandles = reader.ReadInt16();
        NumOfItems = reader.ReadInt16();

        Items = new AndroidSoundItem[NumOfHandles];
        for (var i = 0; i < NumOfItems; i++)
        {
            var item = new AndroidSoundItem();
            item.Read(reader);
            OnSoundLoaded?.Invoke(i, NumOfItems);
            Items[item.Handle] = item;
        }
    }

    public override void Write(ByteWriter writer)
    {
        var i = 0;
        foreach (var item in Items)
            i++;

        writer.WriteInt32(i);
        foreach (var item in Items)
            item.Write(writer);
    }
}

public class AndroidSoundItem : SoundBase
{
    public byte[] Data;
    public int Flags;
    public int Frequency;
    public int Handle;
    public int Length;
    public string Name = "";
    public int Size;

    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadInt16();
        Flags = reader.ReadInt16();
        Length = reader.ReadInt32();
        Frequency = reader.ReadInt32();

        if (Settings.Build >= 289 && (Flags & 0x100) != 0)
            Name = reader.ReadWideString(reader.ReadInt16());
        else
            Name = AndroidSoundBank.oldAndroidNames[Handle];
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32((uint)Handle);
        writer.WriteInt32(0);
        writer.WriteUInt32(0);
        writer.WriteInt32(Data.Length + Name.Length * 2);
        writer.WriteUInt32((uint)Flags);
        writer.WriteInt32(0);
        writer.WriteInt32(Name.Length);
        writer.WriteUnicode(Name);

        writer.WriteBytes(Data);
    }
}