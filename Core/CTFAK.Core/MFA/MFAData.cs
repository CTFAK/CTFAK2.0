using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.Memory;
using CTFAK.MMFParser.MFA.Loaders;
using CTFAK.Utils;

namespace CTFAK.MFA;

public class MFAData
{
    public static readonly string FontBankId = "ATNF";
    public static readonly string ImageBankId = "AGMI";
    public static readonly string MusicBankId = "ASUM";
    public static readonly string SoundBankId = "APMS";
    public string Aboutbox;

    public string Author;

    public BinaryFiles binaryFiles = new();
    public Color BorderColor;
    public string BuildPath = "";
    public int BuildType;
    public int BuildVersion;
    public MFAChunkList Chunks;
    public string CommandLine;
    public string Company;
    public MFAControls Controls;
    public string Copyright;
    public List<Tuple<string, int>> CustomQuals;
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
        "MDI"
    });

    public List<Tuple<int, string, string, int, string>> Extensions;

    public FontBank Fonts = new();
    public int FrameRate;
    public List<MFAFrame> Frames;
    public byte[] GlobalEvents;
    public MFAValueList GlobalStrings;

    public MFAValueList GlobalValues;

    public BitDict GraphicFlags = new(new[]
    {
        "MultiSamples",
        "SpeedIndependent",
        "SoundsOverFrames",
        "PlaySamplesWhenUnfocused",
        "IgnoreInputOnScreensaver",
        "DirectX",
        "VRAM",
        "EnableVisualThemes",
        "VSync",
        "RunWhenMinimized",
        "RunWhenResizing",
        "EnableDebuggerShortcuts",
        "NoDebugger",
        "NoSubappSharing"
    });

    public int GraphicMode;

    public string HelpFile;
    public int IcoCount;
    public List<int> IconImages;
    public AGMIBank Icons = new();
    public AGMIBank Images = new();
    public int InitialLifes;
    public int InitialScore;
    public int LangId = 32;
    public AppMenu Menu;
    public Dictionary<int, int> menuImages;
    public uint MenuSize;

    public int MfaBuild;
    public MusicBank Music = new();

    public string Name;
    public string Path;
    public int Product;
    public int QualCount;
    public SoundBank Sounds = new();

    public byte[] Stamp;
    public string unknown_string; //Found in original mfa build 283 after help file
    public string unknown_string_2; //Found in original mfa build 283 after build path
    public string Version;
    private int windowMenuIndex;

    public int WindowX;
    public int WindowY;

    public void Write(ByteWriter Writer)
    {
        Writer.WriteAscii("MFU2");
        Writer.WriteInt32(MfaBuild);
        Writer.WriteInt32(Product);
        Writer.WriteInt32(BuildVersion);
        Writer.WriteInt32(LangId);
        Writer.AutoWriteUnicode(Name);
        Writer.AutoWriteUnicode(Description);
        Writer.AutoWriteUnicode(Path);
        Writer.WriteUInt32((uint)Stamp.Length);
        Writer.WriteBytes(Stamp);

        Writer.WriteAscii(FontBankId);
        Fonts.Write(Writer);

        Writer.WriteAscii(SoundBankId);
        Sounds.Write(Writer);

        Writer.WriteAscii(MusicBankId);
        // music.Write();

        Writer.WriteInt32(0); //someone is using musics lol?
        //TODO: Do music

        Writer.WriteAscii(ImageBankId);
        Icons.Write(Writer);

        Writer.WriteAscii(ImageBankId);
        Images.Write(Writer);

        Writer.AutoWriteUnicode(Name);
        Writer.AutoWriteUnicode(Author);
        Writer.AutoWriteUnicode(Description);
        Writer.AutoWriteUnicode(Copyright);
        Writer.AutoWriteUnicode(Company);
        Writer.AutoWriteUnicode(Version);
        Writer.WriteInt32(WindowX);
        Writer.WriteInt32(WindowY);
        Writer.WriteColor(Color.FromArgb(0, 255, 255, 255));
        Writer.WriteInt32((int)DisplayFlags.flag);
        Writer.WriteInt32((int)GraphicFlags.flag);
        Writer.AutoWriteUnicode(HelpFile);
        Writer.AutoWriteUnicode(unknown_string);
        Writer.WriteUInt32((uint)InitialScore);
        Writer.WriteUInt32((uint)InitialLifes);
        Writer.WriteInt32(FrameRate);
        Writer.WriteInt32(BuildType);
        Writer.AutoWriteUnicode(BuildPath ?? "");
        Writer.AutoWriteUnicode(unknown_string_2);
        Writer.AutoWriteUnicode(CommandLine);
        Writer.AutoWriteUnicode(Aboutbox);
        Writer.WriteInt32(0);

        binaryFiles.Write(Writer);

        Controls.Write(Writer);

        if (Menu != null)
            using (var menuWriter = new ByteWriter(new MemoryStream()))
            {
                Menu.Write(menuWriter);

                Writer.WriteUInt32((uint)menuWriter.BaseStream.Position);
                Writer.WriteWriter(menuWriter);
            }
        else
            Writer.WriteInt32(0);

        Writer.WriteInt32(windowMenuIndex);
        Writer.WriteInt32(menuImages.Count);
        foreach (var valuePair in menuImages)
        {
            Writer.WriteInt32(valuePair.Key);
            Writer.WriteInt32(valuePair.Value);
        }

        GlobalValues.Write(Writer);
        GlobalStrings.Write(Writer);
        Writer.WriteInt32(GlobalEvents.Length);
        Writer.WriteBytes(GlobalEvents);
        Writer.WriteInt32(GraphicMode);
        Writer.WriteUInt32((uint)IconImages.Count);
        foreach (var iconImage in IconImages) Writer.WriteInt32(iconImage);
        Writer.WriteInt32(CustomQuals.Count);
        foreach (var customQual in CustomQuals)
        {
            Writer.AutoWriteUnicode(customQual.Item1);
            Writer.WriteInt32(customQual.Item2);
        }

        Writer.WriteInt32(Extensions.Count);
        foreach (var extension in Extensions)
        {
            Writer.WriteInt32(extension.Item1);
            Writer.AutoWriteUnicode(extension.Item3);
            Writer.AutoWriteUnicode(extension.Item2);
            Writer.WriteInt32(extension.Item4);
            Writer.WriteInt16((short)(extension.Item5.Length - 1));
            Writer.Skip(1);
            Writer.WriteInt8(0x80);
            //Writer.WriteInt8(0x01);
            Writer.Skip(2);
            Writer.WriteUnicode(extension.Item5);
        }

        //Writer.Skip(-2);
        Writer.WriteInt32(Frames.Count); //frame

        var startPos = Writer.Tell() + 4 * Frames.Count + 4;
        //Console.WriteLine(startPos);
        var newWriter = new ByteWriter(new MemoryStream());
        foreach (var frame in Frames)
        {
            Writer.WriteUInt32((uint)(startPos + newWriter.Tell()));
            frame.Write(newWriter);
        }

        Writer.WriteUInt32((uint)(startPos + newWriter.Tell()));

        Writer.WriteWriter(newWriter);
        Chunks.Write(Writer);
        Writer.Flush();
        Writer.Close();
        Console.WriteLine("Writing done");
    }

    public void Read(ByteReader reader)
    {
        Settings.isMFA = true;
        reader.ReadAscii(4);
        MfaBuild = reader.ReadInt32();
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
        DisplayFlags.flag = reader.ReadUInt32();
        GraphicFlags.flag = reader.ReadUInt32();
        HelpFile = reader.AutoReadUnicode();
        unknown_string = reader.AutoReadUnicode();

        InitialScore = reader.ReadInt32();
        InitialLifes = reader.ReadInt32();
        FrameRate = reader.ReadInt32();
        BuildType = reader.ReadInt32();
        BuildPath = reader.AutoReadUnicode();
        unknown_string_2 = reader.AutoReadUnicode();
        CommandLine = reader.AutoReadUnicode();
        Aboutbox = reader.AutoReadUnicode();
        reader.ReadUInt32();

        binaryFiles = new BinaryFiles();
        binaryFiles.Read(reader);

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

        windowMenuIndex = reader.ReadInt32();
        menuImages = new Dictionary<int, int>();
        var miCount = reader.ReadInt32();
        for (var i = 0; i < miCount; i++)
        {
            var id = reader.ReadInt32();
            menuImages[id] = reader.ReadInt32();
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