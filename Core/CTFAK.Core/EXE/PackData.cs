using System.Collections.Generic;
using System.Diagnostics;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.EXE;

public class PackData
{
    public uint FormatVersion;
    public List<PackFile> Items = new();

    public void Read(ByteReader reader)
    {
        Logger.Log("Reading PackData");
        var start = reader.Tell();
        var header = reader.ReadBytes(8); //PackData header. I can probably validate that, but I don't think I need to

        var headerSize = reader.ReadUInt32();
        Debug.Assert(headerSize == 32);
        var dataSize = reader.ReadUInt32();

        reader.Seek((int)(start + dataSize - 32));
        var uheader = reader.ReadAscii(4);
        if (uheader == "PAMU")
        {
            Logger.Log("Found PAMU header");
            Settings.gameType |= Settings.GameType.NORMAL;
            Settings.Unicode = true;
        }
        else if (uheader == "PAME")
        {
            Logger.Log("Found PAME header");
            if (!Settings.Old)
                Settings.gameType |= Settings.GameType.MMF2;
            Settings.Unicode = false;
        }

        reader.Seek(start + 16);

        FormatVersion = reader.ReadUInt32();
        var check = reader.ReadInt32();
        //Removing this seemed to not break anything, adding it breaks things for me.
        //Debug.Assert(check == 0);
        check = reader.ReadInt32();
        Debug.Assert(check == 0);

        var count = reader.ReadUInt32();

        var offset = reader.Tell();
        for (var i = 0; i < count; i++)
        {
            if (!reader.HasMemory(2)) break;
            var value = reader.ReadUInt16();
            if (!reader.HasMemory(value)) break;
            reader.ReadBytes(value);
            reader.Skip(value);
            if (!reader.HasMemory(value)) break;
        }

        var newHeader = reader.ReadAscii(4);
        var hasBingo = newHeader != "PAME" && newHeader != "PAMU";

        reader.Seek(offset);
        for (var i = 0; i < count; i++)
        {
            var item = new PackFile();
            item.HasBingo = hasBingo;
            item.Read(reader);
            Items.Add(item);
        }
    }
}

public class PackFile
{
    private int _bingo;
    public byte[] Data;
    public bool HasBingo;
    public string PackFilename = "ERROR";

    public void Read(ByteReader exeReader)
    {
        var len = exeReader.ReadUInt16();
        PackFilename = exeReader.ReadWideString(len);
        _bingo = exeReader.ReadInt32();
        Data = exeReader.ReadBytes(exeReader.ReadInt32());
        Logger.Log($"New packfile data: Name - {PackFilename}; Data size - {Data.Length}");
        try
        {
            //File.WriteAllBytes($"ExtDump\\{PackFilename}", ZlibStream.UncompressBuffer(Data));
        }
        catch
        {
            //File.WriteAllBytes($"ExtDump\\{PackFilename}", Data);
        }
        //Dump();
    }
}