using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Banks;

[ChunkLoader(26216, "SoundBank")]
public class SoundBank : ChunkLoader
{
    public bool IsCompressed = true;
    public List<SoundItem> Items = new();

    public int NumOfItems;
    public int References = 0;
    public static event SaveHandler OnSoundLoaded;

    public override void Read(ByteReader reader)
    {
        //if (!Settings.DoMFA) reader.Seek(0);//Reset the reader to avoid bugs when dumping more than once
        Items = new List<SoundItem>();
        NumOfItems = reader.ReadInt32();
        //if (!Settings.DumpSounds) return;

        for (var i = 0; i < NumOfItems; i++)
        {
            if (Settings.Android) continue;
            if (Settings.Old) continue;

            var item = new SoundItem();

            item.IsCompressed = IsCompressed;
            item.Read(reader);
            OnSoundLoaded?.Invoke(i, NumOfItems);

            Items.Add(item);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Items.Count);
        foreach (var item in Items) item.Write(writer);
    }
}

public class SoundBase : ChunkLoader
{
    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Read(ByteReader reader)
    {
    }
}

public class SoundItem : SoundBase
{
    public int Checksum;
    public byte[] Data;
    public uint Flags;
    public uint Handle;
    public bool IsCompressed;
    public string Name;
    public uint References;
    public int Size;

    //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public override void Read(ByteReader reader)
    {
        base.Read(reader);

        var start = reader.Tell();

        Handle = reader.ReadUInt32();
        Checksum = reader.ReadInt32();

        References = reader.ReadUInt32();
        var decompressedSize = reader.ReadInt32();
        Flags = reader.ReadUInt32();
        var res = reader.ReadInt32();
        var nameLenght = reader.ReadInt32();

        ByteReader soundData;
        if (IsCompressed && Flags != 33)
        {
            Size = reader.ReadInt32();
            soundData = new ByteReader(Decompressor.DecompressBlock(reader, Size));
        }
        else
        {
            soundData = new ByteReader(reader.ReadBytes(decompressedSize));
        }

        Name = soundData.ReadWideString(nameLenght).Replace(" ", "");
        if (Flags == 33) soundData.Seek(0);
        Data = soundData.ReadBytes((int)soundData.Size());
        soundData.Close();
        soundData.Dispose();
    }

    public void AndroidRead(ByteReader soundData, string itemName)
    {
        Handle = uint.Parse(Path.GetFileNameWithoutExtension(itemName).TrimStart('s'));
        Size = (int)soundData.Size();
        Name = Path.GetFileNameWithoutExtension(itemName);
        Data = soundData.ReadBytes((int)soundData.Size());
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32(Handle);
        writer.WriteInt32(Checksum);
        writer.WriteUInt32(References);
        writer.WriteInt32(Data.Length + Name.Length * 2);
        writer.WriteUInt32(Flags);
        writer.WriteInt32(0);
        writer.WriteInt32(Name.Length);
        writer.WriteUnicode(Name);
        // writer.BaseStream.Position -= 4;
        writer.WriteBytes(Data);
    }
}

public class OldSound : SoundBase
{
    private ushort _bitsPerSample;
    private ushort _blockAlign;
    private uint _byteRate;
    private ushort _channelCount;
    private ushort _checksum;
    private byte[] _data;
    private uint _flags;
    private ushort _format;
    private uint _handle;
    private string _name;
    private uint _references;
    private uint _sampleRate;
    private uint _size;

    public override void Read(ByteReader reader)
    {
        _handle = reader.ReadUInt32();
        var start = reader.Tell();
        var newData = new ByteReader(Decompressor.DecompressOld(reader));
        _checksum = newData.ReadUInt16();
        _references = newData.ReadUInt32();
        _size = newData.ReadUInt32();
        _flags = newData.ReadUInt32();
        var reserved = newData.ReadUInt32();
        var nameLen = newData.ReadInt32();
        _name = newData.ReadAscii(nameLen);

        _format = newData.ReadUInt16();
        _channelCount = newData.ReadUInt16();
        _sampleRate = newData.ReadUInt32();
        _byteRate = newData.ReadUInt32();
        _blockAlign = newData.ReadUInt16();
        _bitsPerSample = newData.ReadUInt16();
        newData.ReadUInt16();
        var chunkSize = newData.ReadInt32();
        Debug.Assert(newData.Size() - newData.Tell() == chunkSize);
        _data = newData.ReadBytes(chunkSize);
    }

    public void CopyDataToSound(ref SoundItem result)
    {
        result.Handle = _handle;
        result.Checksum = _checksum;
        result.References = _references;
        result.Data = GetWav();
        result.Name = _name;
        result.Flags = _flags;
    }

    public byte[] GetWav()
    {
        var writer = new ByteWriter(new MemoryStream());
        writer.WriteAscii("RIFF");
        writer.WriteInt32(_data.Length - 44);
        writer.WriteAscii("WAVEfmt ");
        writer.WriteUInt32(16);
        writer.WriteUInt16(_format);
        writer.WriteUInt16(_channelCount);
        writer.WriteUInt32(_sampleRate);
        writer.WriteUInt32(_byteRate);
        writer.WriteUInt16(_blockAlign);
        writer.WriteUInt16(_bitsPerSample);
        writer.WriteAscii("data");
        writer.WriteUInt32((uint)_data.Length);
        writer.WriteBytes(_data);

        return writer.GetBuffer();
    }
}