using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.Core.CCN.Chunks;
using CTFAK.EXE;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.CCN.Chunks.Frame;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Shared.Banks;
using CTFAK.Shared.Banks.ImageBank;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN;

public class GameData
{
    public short runtimeVersion;
    public short runtimeSubversion;
    public int productVersion;
    public int productBuild;

    public string AboutText;
    public string Author = "";
    public BinaryFiles BinaryFiles = new BinaryFiles();
    public string Copyright;
    public string Doc;

    public string EditorFilename;
    public ExtData ExtData;
    public Extensions Extensions;

    public FontBank Fonts;
    public FrameHandles FrameHandles;

    public Dictionary<int, ObjectInfo> FrameItems = new();

    public List<Frame> Frames = new();
    public GlobalStrings GlobalStrings;
    public GlobalValues GlobalValues;

    public AppHeader Header;
    public ExtendedHeader ExtHeader;
    public ImageBank Images = new();

    public AppMenu Menu;
    public MusicBank Music;
    public ImageShapes ImageShapes;

    public string Name;
    public Bitmap Icon32x;
    public bool ExeOnly;

    public PackData PackData;
    public Shaders Shaders;
    public SoundBank Sounds;
    public string TargetFilename;
    public static event SaveHandler OnChunkLoaded;
    public static event SaveHandler OnFrameLoaded;

    public void Read(ByteReader reader)
    {
        var magic = reader.ReadAscii(4);

        // We don't really care about that stuff, because the reader will automatically figure out if the game uses unicode by checking some stuff in the AppName chunk
        if (magic == "PAMU")
            Settings.Unicode = true;
        else if (magic == "PAME")
            Settings.Unicode = false;
        else if (magic == "CRUF")
            Settings.gameType |= Settings.GameType.F3;
        else Logger.LogWarning("Couldn't found any known headers: " + magic); //Header not found

        if (CTFAKCore.Parameters.Contains("-android"))
            Settings.gameType |= Settings.GameType.ANDROID;
        
        if (CTFAKCore.Parameters.Contains("-f3"))
        {
            Logger.Log("Forcing F3 mode");
            Settings.gameType |= Settings.GameType.F3;
        }

        runtimeVersion = (short)reader.ReadUInt16();
        runtimeSubversion = (short)reader.ReadUInt16();
        productVersion = reader.ReadInt32();
        productBuild = reader.ReadInt32();
        Settings.Build = productBuild;

        Logger.Log("Fusion Build: " + productBuild);

        var chunkList = new ChunkList();
        chunkList.OnHandleChunk += (id, loader) =>
        {
            chunkList.HandleChunk(id, loader, this);
        }; // This doesn't work and I honestly don't care
        chunkList.OnChunkLoaded += (id, loader) =>
        {
            switch (id)
            {
                case 8739: //AppHeader
                    Header = loader as AppHeader;
                    break;
                case 8740: //AppName
                    Name = (loader as AppName)?.Value;
                    break;
                case 8741: //AppAuthor
                    Author = (loader as AppAuthor)?.Value;
                    break;
                case 8742: //AppMenu
                    Menu = loader as AppMenu;
                    break;
                case 8744: //Extensions
                    break;
                case 8745: //FrameItems
                    FrameItems = (loader as FrameItems)?.Items;
                    break;
                case 8746: //GlobalEvents
                    break;
                case 8747: //FrameHandler
                    FrameHandles = loader as FrameHandles;
                    break;
                case 8748: //ExtData
                    ExtData = loader as ExtData;
                    break;
                case 8749: //AdditionalExtension
                    break;
                case 8750: //AppEditorFilename
                    EditorFilename = (loader as EditorFilename)?.Value;
                    if (Settings.Build > 284)
                        Decryption.MakeKey(Name, Copyright, EditorFilename);
                    else
                        Decryption.MakeKey(EditorFilename, Name, Copyright);
                    break;
                case 8751: //AppTargetFilename
                    TargetFilename = (loader as TargetFilename)?.Value;
                    break;
                case 8752: //AppDoc
                    break;
                case 8753: //OtherExts
                    break;
                case 8754: //GlobalValues
                    GlobalValues = loader as GlobalValues;
                    break;
                case 8755: //GlobalStrings
                    GlobalStrings = loader as GlobalStrings;
                    break;
                case 8756: //Extensions2
                    Extensions = loader as Extensions;
                    break;
                case 8757: //AppIcon
                    var Icon = loader as AppIcon;
                    Icon32x = Icon.Icon;
                    break;
                case 8758: //DemoVersion
                    break;
                case 8759: //SecNum
                    break;
                case 8760: //BinaryFiles
                    BinaryFiles = loader as BinaryFiles;
                    break;
                case 8761: //AppMenuImages:
                    break;
                case 8762: //AboutText
                    AboutText = (loader as AboutText)?.Value;
                    break;
                case 8763: //Copyright
                    Copyright = (loader as Copyright)?.Value;
                    break;
                case 8764: //GlobalValueNames
                    break;
                case 8765: //GlobalStringNames
                    break;
                case 8766: //MvtTexts
                    break;
                case 8767: //FrameItems2
                    FrameItems = (loader as FrameItems2)?.Items;
                    break;
                case 8768: //ExeOnly
                    var exeOnly = loader as ExeOnly;
                    ExeOnly = exeOnly.exeOnly;
                    break;
                case 8771:
                    Shaders = loader as Shaders;
                    break;
                case 8773: //ExtendedHeader
                    ExtHeader = loader as ExtendedHeader;
                    break;
                case 8792: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 8793: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 13107: //Frame
                    Frames.Add(loader as Frame);
                    break;
                case 17664: //ImageShapes
                    ImageShapes = loader as ImageShapes;
                    break;
                case 26214: //ImageBank
                    Images = loader as ImageBank;
                    break;
                case 26215: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 26216: //SoundBank
                    if (CTFAKCore.Parameters.Contains("-nosounds")) break;
                    if (Settings.gameType == Settings.GameType.ANDROID)
                    {
                        Sounds = ApkFileReader.AndroidSoundBank;
                        var AndroidSounds = loader as AndroidSoundBank;
                        for (int i = 0; i < Sounds.Items.Count; i++)
                            Sounds.Items[i].Name = AndroidSounds.Items[Sounds.Items[i].Handle].Name;
                    }
                    else
                        Sounds = loader as SoundBank;
                    break;
                case 8790: //TwoFivePlusProperties
                    FrameItems = TwoFilePlusContainer.Instance.ObjectsContainer;
                    break;
            }
        };
        chunkList.Read(reader);
        // reading again if we encounter an F3 game that uses a separate chunk list for images and sounds
        // it's safe to just read again
        //chunkList.Read(reader); // turns out it's not
        if (CTFAKCore.Parameters.Contains("-debug"))
            Console.ReadLine();
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteAscii(Settings.Unicode ? "PAMU" : "PAME");
        writer.WriteInt32(3);
        writer.WriteInt32(770);
        writer.WriteInt32(0);
        writer.WriteInt32(Settings.Build);
        var chunkList = new ChunkList();
        

    }
}