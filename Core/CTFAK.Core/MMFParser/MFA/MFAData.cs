using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.Common.Banks;
using CTFAK.Utils;

namespace CTFAK.MMFParser.MFA;

public class MFAData
{
    public const string FontBankId = "ATNF";
    public const string ImageBankId = "AGMI";
    public const string MusicBankId = "ASUM";
    public const string SoundBankId = "APMS";
    public string Aboutbox;

    public string Author;

    public BinaryFiles BinaryFiles = new();
    public Color BorderColor;
    public string BuildPath = "";
    public int BuildType;
    public int BuildVersion;
    public MFAChunkList Chunks = new();
    public string CommandLine;
    public string Company;
    public MFAControls Controls = new MFAControls();
    public string Copyright;
    public List<Tuple<string, int>> CustomQuals = new();
    public string Description;

    public BitDict DisplayFlags = new(new[]
    {
        "MaximizedOnBoot",
        "ResizeDisplay",
        "FullscreenAtStart",
        "AllowFullscreen",
        "Heading",
        "HeadingWhenMaximized",
        "MenuBar",
        "MenuOnBoot",
        "NoMinimize",
        "NoMaximize",
        "NoThickFrame",
        "NoCenter",
        "DisableClose",
        "HiddenAtStart",
        "MDI",
        "Unknown1",
        "Unknown2",
        "Unknown3",
        "Unknown4",
        "Unknown5",
        "Unknown6",
        "Unknown7",
        "Unknown8",
        "Unknown9",
        "Unknown10",
        "Unknown11",
        "Unknown12",
        "Unknown13",
        "Unknown14",
        "Unknown15",
        "Unknown16",
        "Unknown17",
        "Unknown18",
        "Unknown19",
        "Unknown20"
    });

    public List<Tuple<int, string, string, int, string>> Extensions = new();

    public FontBank Fonts = new();
    public int FrameRate;
    public List<MFAFrame> Frames = new();
    public byte[] GlobalEvents = new byte[0];
    public MFAValueList GlobalStrings = new MFAValueList();

    public MFAValueList GlobalValues= new MFAValueList();

    public BitDict GraphicFlags = new(new[]
    {
        "MultiSamples",
        "MachineIndependentSpeed",
        "SamplesOverFrames",
        "PlaySamplesWhenUnfocused",
        "IgnoreInputOnScreensaver",
        "DirectX",
        "VRAM",
        "VisualThemes",
        "VSync",
        "RunWhenMinimized",
        "RunWhenResizing",
        "EnableDebuggerShortcuts",
        "NoDebugger",
        "NoSubappSharing",
        "Direct3D9",
        "Direct3D8",
        "Unknown1",
        "Unknown2",
        "Unknown3",
        "IncludePreloaderFlash",
        "DontGenerateHTMLFlash",
        "Unknown4",
        "DisableIME",
        "ReduceCPUUsage",
        "Unknown5",
        "UseHighPerformanceGPU",
        "Profiling",
        "DontProfileAtStart",
        "Direct3D11",
        "PremultipliedAlpha",
        "DontOptimizeEvents",
        "RecordSlowLoops"
    });

    public int GraphicMode;

    public string HelpFile;
    public int IcoCount;
    public List<int> IconImages = new List<int>();
    public AGMIBank Icons = new();
    public AGMIBank Images = new();
    public int InitialLifes;
    public int InitialScore;
    public int LangId = 8192;
    public AppMenu Menu;
    public Dictionary<int, int> MenuImages = new Dictionary<int, int>();
    public uint MenuSize;
    public short MfaSubversion;

    public short MfaVersion;
    public MusicBank Music = new();

    public string Name;
    public string Path;
    public int Product;
    public int QualCount;
    public SoundBank Sounds = new();

    public byte[] Stamp = new byte[0];
    public string _unknownString; //Found in original mfa build 283 after help file
    public string _unknownString2; //Found in original mfa build 283 after build path
    public string Version;
    public int _windowMenuIndex;

    public int WindowX;
    public int WindowY;

    public void Write(ByteWriter writer)
    {
        writer.WriteAscii("MFU2");
        writer.WriteInt16(MfaVersion);
        writer.WriteInt16(MfaSubversion);
        writer.WriteInt32(Product);
        writer.WriteInt32(BuildVersion);
        writer.WriteInt32(LangId);
        writer.AutoWriteUnicode(Name);
        writer.AutoWriteUnicode(Description);
        writer.AutoWriteUnicode(Path);
        writer.WriteUInt32((uint)Stamp.Length);
        writer.WriteBytes(Stamp);

        writer.WriteAscii(FontBankId);
        Fonts.Write(writer);

        writer.WriteAscii(SoundBankId);
        Sounds.Write(writer);

        writer.WriteAscii(MusicBankId);
        // music.Write();

        writer.WriteInt32(0); //someone is using musics lol?
        //TODO: Do music

        writer.WriteAscii(ImageBankId);
        Icons.Write(writer);

        writer.WriteAscii(ImageBankId);
        Images.Write(writer);

        writer.AutoWriteUnicode(Name);
        writer.AutoWriteUnicode(Author);
        writer.AutoWriteUnicode(Description);
        writer.AutoWriteUnicode(Copyright);
        writer.AutoWriteUnicode(Company);
        writer.AutoWriteUnicode(Version);
        writer.WriteInt32(WindowX);
        writer.WriteInt32(WindowY);
        writer.WriteColor(BorderColor);
        writer.WriteUInt32(DisplayFlags.Flag);
        writer.WriteUInt32(GraphicFlags.Flag);
        writer.AutoWriteUnicode(HelpFile);
        writer.AutoWriteUnicode(_unknownString);
        writer.WriteUInt32((uint)InitialScore);
        writer.WriteUInt32((uint)InitialLifes);
        writer.WriteInt32(FrameRate);
        writer.WriteInt32(BuildType);
        writer.AutoWriteUnicode(BuildPath ?? "");
        writer.AutoWriteUnicode(_unknownString2);
        writer.AutoWriteUnicode(CommandLine);
        writer.AutoWriteUnicode(Aboutbox);
        writer.WriteInt32(0);

        BinaryFiles.Write(writer);

        Controls.Write(writer);

        if (Menu != null)
            using (var menuWriter = new ByteWriter(new MemoryStream()))
            {
                Menu.Write(menuWriter);

                writer.WriteUInt32((uint)menuWriter.BaseStream.Position);
                writer.WriteWriter(menuWriter);
            }
        else
            writer.WriteInt32(0);

        writer.WriteInt32(_windowMenuIndex);
        writer.WriteInt32(MenuImages.Count);
        foreach (var valuePair in MenuImages)
        {
            writer.WriteInt32(valuePair.Key);
            writer.WriteInt32(valuePair.Value);
        }

        GlobalValues.Write(writer);
        GlobalStrings.Write(writer);
        writer.WriteInt32(GlobalEvents.Length);
        writer.WriteBytes(GlobalEvents);
        writer.WriteInt32(GraphicMode);
        writer.WriteUInt32((uint)IconImages.Count);
        foreach (var iconImage in IconImages) writer.WriteInt32(iconImage);
        writer.WriteInt32(CustomQuals.Count);
        foreach (var customQual in CustomQuals)
        {
            writer.AutoWriteUnicode(customQual.Item1);
            writer.WriteInt32(customQual.Item2);
        }

        writer.WriteInt32(Extensions.Count);
        foreach (var extension in Extensions)
        {
            writer.WriteInt32(extension.Item1);
            writer.AutoWriteUnicode(extension.Item3);
            writer.AutoWriteUnicode(extension.Item2);
            writer.WriteInt32(extension.Item4);
            writer.WriteInt16((short)(extension.Item5.Length - 1));
            writer.Skip(1);
            writer.WriteInt8(0x80);
            //Writer.WriteInt8(0x01);
            writer.Skip(2);
            writer.WriteUnicode(extension.Item5);
        }

        //Writer.Skip(-2);
        writer.WriteInt32(Frames.Count); //frame

        var startPos = writer.Tell() + 4 * Frames.Count + 4;
        //Console.WriteLine(startPos);
        var newWriter = new ByteWriter(new MemoryStream());
        foreach (var frame in Frames)
        {
            writer.WriteUInt32((uint)(startPos + newWriter.Tell()));
            frame.Write(newWriter);
        }

        writer.WriteUInt32((uint)(startPos + newWriter.Tell()));

        writer.WriteWriter(newWriter);
        Chunks.Write(writer);
        writer.Flush();
        writer.Close();
        writer.Dispose();
        Console.WriteLine("Writing done");
    }

    public void Read(ByteReader reader)
    {
        Settings.isMFA = true;
        reader.ReadAscii(4);
        MfaVersion = reader.ReadInt16();
        MfaSubversion = reader.ReadInt16();
        Product = reader.ReadInt32();
        BuildVersion = reader.ReadInt32();
        //reader.ReadInt32();//unknown
        //Settings.Build = BuildVersion;
        LangId = reader.ReadInt32();
        Name = reader.AutoReadUnicode();
        Description = reader.AutoReadUnicode();
        Path = reader.AutoReadUnicode();
        var stampLen = reader.ReadInt32();
        Stamp = reader.ReadBytes(stampLen);

        if (reader.ReadAscii(4) != FontBankId) throw new Exception("Invalid Font Bank");
        Fonts.Compressed = false;
        Fonts.Read(reader);

        if (reader.ReadAscii(4) != SoundBankId) throw new Exception("Invalid Sound Bank");
        Sounds.IsCompressed = false;
        Sounds.Read(reader);

        if (reader.ReadAscii(4) != MusicBankId) throw new Exception("Invalid Music Bank");
        Music.Read(reader);

        if (reader.ReadAscii(4) != "AGMI") throw new Exception("Invalid Icon Bank: ");
        Icons.Read(reader);

        if (reader.ReadAscii(4) != "AGMI") throw new Exception("Invalid Image Bank");
        Images.Read(reader);
        var nam = reader.AutoReadUnicode();
        Debug.Assert(Name == nam);

        Author = reader.AutoReadUnicode();
        var desc = reader.AutoReadUnicode();
        Debug.Assert(Description == desc);
        //Helper.CheckPattern(, Description);
        Copyright = reader.AutoReadUnicode();
        Company = reader.AutoReadUnicode();
        Version = reader.AutoReadUnicode();
        WindowX = reader.ReadInt32();
        WindowY = reader.ReadInt32();
        BorderColor = reader.ReadColor();
        DisplayFlags.Flag = reader.ReadUInt32();
        GraphicFlags.Flag = reader.ReadUInt32();
        HelpFile = reader.AutoReadUnicode();
        _unknownString = reader.AutoReadUnicode();

        InitialScore = reader.ReadInt32();
        InitialLifes = reader.ReadInt32();
        FrameRate = reader.ReadInt32();
        BuildType = reader.ReadInt32();
        BuildPath = reader.AutoReadUnicode();
        _unknownString2 = reader.AutoReadUnicode();
        CommandLine = reader.AutoReadUnicode();
        Aboutbox = reader.AutoReadUnicode();
        reader.ReadUInt32();

        BinaryFiles = new BinaryFiles();
        BinaryFiles.Read(reader);

        Controls = new MFAControls();
        Controls.Read(reader);

        MenuSize = reader.ReadUInt32();
        var currentPosition = reader.Tell();
        try
        {
            Menu = new AppMenu();
            Menu.Read(reader);
        }
        catch
        {
        }

        reader.Seek(MenuSize + currentPosition);

        _windowMenuIndex = reader.ReadInt32();
        MenuImages = new Dictionary<int, int>();
        var miCount = reader.ReadInt32();
        for (var i = 0; i < miCount; i++)
        {
            var id = reader.ReadInt32();
            MenuImages[id] = reader.ReadInt32();
        }

        GlobalValues = new MFAValueList();
        GlobalValues.Read(reader);
        GlobalStrings = new MFAValueList();
        GlobalStrings.Read(reader);
        GlobalEvents = reader.ReadBytes(reader.ReadInt32());
        GraphicMode = reader.ReadInt32();

        IcoCount = reader.ReadInt32();
        IconImages = new List<int>();
        for (var i = 0; i < IcoCount; i++) IconImages.Add(reader.ReadInt32());

        QualCount = reader.ReadInt32();
        CustomQuals = new List<Tuple<string, int>>();
        for (var i = 0; i < QualCount; i++) //qualifiers
        {
            var name = reader.ReadAscii(reader.ReadInt32());
            var handle = reader.ReadInt32();
            CustomQuals.Add(new Tuple<string, int>(name, handle));
        }

        var extCount = reader.ReadInt32();
        Extensions = new List<Tuple<int, string, string, int, string>>();
        for (var i = 0; i < extCount; i++) //extensions
        {
            var handle = reader.ReadInt32();
            var name = reader.AutoReadUnicode();
            var filename = reader.AutoReadUnicode();
            var magic = reader.ReadInt32();
            var subType = reader.ReadWideString(reader.ReadInt32());
            var tuple = new Tuple<int, string, string, int, string>(handle, filename, name, magic, subType);
            Extensions.Add(tuple);
        }

        if (reader.PeekInt32() > 900) reader.ReadInt16();
        //
        var frameOffsets = new List<int>();
        var offCount = reader.ReadInt32();
        for (var i = 0; i < offCount; i++) frameOffsets.Add(reader.ReadInt32());

        var nextOffset = reader.ReadInt32();
        Frames = new List<MFAFrame>();
        foreach (var item in frameOffsets)
        {
            reader.Seek(item);
            var testframe = new MFAFrame();
            testframe.Read(reader);
            Frames.Add(testframe);
        }

        reader.Seek(nextOffset);
        Chunks = new MFAChunkList();
        Chunks.Read(reader);
        reader.Dispose();
    }
}

public static class MFAUtils
{
    public static string AutoReadUnicode(this ByteReader reader)
    {
        var len = reader.ReadInt16();
        var check = reader.ReadInt16();
        Debug.Assert(check == -32768);
        return reader.ReadWideString(len);
    }

    public static void AutoWriteUnicode(this ByteWriter writer, string value)
    {
        if (value == null) value = "";
        writer.WriteInt16((short)value.Length);
        writer.Skip(1);
        writer.WriteInt8(0x80);
        writer.WriteUnicode(value);
    }
}