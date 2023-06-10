using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.FileReaders;

public class ExeFileReader : IFileReader
{
    public GameData Game;
    public Dictionary<int, Bitmap> Icons = new();
    public int Priority => 5;
    public virtual string Name => "EXE";

    private ByteReader _reader;

    public virtual bool LoadGame(string gamePath)
    {
        CTFAKCore.CurrentReader = this;
        Settings.gameType = Settings.GameType.NORMAL;
        LoadIcons(gamePath);

        _reader = new ByteReader(gamePath, FileMode.Open);
        ReadPEHeader(_reader);
        LoadCcn(_reader);
        return true;
    }

    public GameData GetGameData()
    {
        return Game;
    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return Icons;
    }

    public void Close()
    {
        _reader.Close();
        _reader.Dispose();
    }

    public ByteReader GetFileReader()
    {
        return _reader;
    }

    public void LoadIcons(string gamePath)
    {
        var icoExt = new IconExtractor(gamePath);
        var icos = icoExt.GetAllIcons();
        foreach (var icon in icos) Icons.Add(icon.Width, icon.ToBitmap());

        if (!Icons.ContainsKey(16)) Icons.Add(16, Icons[32].ResizeImage(new Size(16, 16)));
        if (!Icons.ContainsKey(48)) Icons.Add(48, Icons[32].ResizeImage(new Size(48, 48)));
        if (!Icons.ContainsKey(128)) Icons.Add(128, Icons[32].ResizeImage(new Size(128, 128)));
        if (!Icons.ContainsKey(256)) Icons.Add(256, Icons[32].ResizeImage(new Size(256, 256)));
    }

    public void LoadCcn(ByteReader reader)
    {
        PackData packData = null;
        if (Settings.Old)
        {
            Settings.Unicode = false;
            if (reader.PeekInt32() != 1162690896) //PAME magic
                while (true)
                {
                    if (reader.Tell() >= reader.Size()) break;
                    var id = reader.ReadInt16();
                    var flag = reader.ReadInt16();
                    var size = reader.ReadInt32();
                    reader.ReadBytes(size);
                    // MMF1.5, MMF1 and most likely CNC store extensions in a separate chunk list. TODO: Figure this out
                    //var newChunk = new Chunk(reader);
                    //var chunkData = newChunk.Read();
                    if (id == 32639) break;
                }
        }
        else
        {
            packData = new PackData();
            packData.Read(reader);
        }

        Game = new GameData();
        Game.Read(reader);
        if (!Settings.Old) Game.PackData = packData;
    }

    public int ReadPEHeader(ByteReader reader)
    {
        var entryPoint = CalculateEntryPoint(reader);
        reader.Seek(0);
        var exeHeader = reader.ReadBytes(entryPoint);

        var firstShort = reader.PeekUInt16();

        if (firstShort != 0x7777) Settings.gameType |= Settings.GameType.MMF15;
        if (Settings.Old) Logger.Log($"1.5 game detected. First short: {firstShort.ToString("X")}");
        return (int)reader.Tell();
    }


    public int CalculateEntryPoint(ByteReader exeReader)
    {
        var sig = exeReader.ReadAscii(2);
        if (sig != "MZ") Logger.LogWarning("Invalid executable signature");

        exeReader.Seek(60);

        var hdrOffset = exeReader.ReadUInt16();

        exeReader.Seek(hdrOffset);
        var peHdr = exeReader.ReadAscii(2);
        exeReader.Skip(4);

        var numOfSections = exeReader.ReadUInt16();

        exeReader.Skip(16);
        var optionalHeader = 28 + 68;
        var dataDir = 16 * 8;
        exeReader.Skip(optionalHeader + dataDir);

        var possition = 0;
        for (var i = 0; i < numOfSections; i++)
        {
            var entry = exeReader.Tell();
            var sectionName = exeReader.ReadAscii();

            if (sectionName == ".extra")
            {
                exeReader.Seek(entry + 20);
                possition = (int)exeReader.ReadUInt32(); //Pointer to raw data
                break;
            }

            if (i >= numOfSections - 1)
            {
                exeReader.Seek(entry + 16);
                var size = exeReader.ReadUInt32();
                var address = exeReader.ReadUInt32(); //Pointer to raw data

                possition = (int)(address + size);
                break;
            }

            exeReader.Seek(entry + 40);
        }

        exeReader.Seek(possition);
        return (int)exeReader.Tell();
    }
}

public class UnpackedExeFileReader : ExeFileReader
{
    public override string Name => "EXE (Unpacked)";

    public override bool LoadGame(string gamePath)
    {
        CTFAKCore.CurrentReader = this;
        Settings.gameType = Settings.GameType.NORMAL;
        LoadIcons(gamePath);

        var reader = new ByteReader(gamePath.Replace(".exe", ".dat"), FileMode.Open);
        LoadCcn(reader);
        return true;
    }
}