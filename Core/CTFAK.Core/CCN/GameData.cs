using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using CTFAK.EXE;
using CTFAK.MMFParser.EXE.Loaders;

namespace CTFAK.CCN
{
    public class GameData
    {
        public static event SaveHandler OnChunkLoaded;
        public static event SaveHandler OnFrameLoaded;
        
        private short _runtimeVersion;
        private short _runtimeSubversion;
        private int _productVersion;
        private int _productBuild;

        public string Name;
        public string Author="";
        public string Copyright;
        public string AboutText;
        public string Doc;

        public string EditorFilename;
        public string TargetFilename;

        public AppMenu Menu;

        public AppHeader Header;
        
        public FontBank Fonts;
        public SoundBank Sounds;
        public MusicBank Music;
        public ImageBank Images = new ImageBank();

        public Dictionary<int,ObjectInfo> FrameItems = new Dictionary<int, ObjectInfo>();

        public List<Frame> Frames = new List<Frame>();
        public FrameHandles FrameHandles;
        public Extensions Extensions;

        public PackData PackData;
        public Shaders Shaders;
        public GlobalStrings GlobalStrings;
        public GlobalValues GlobalValues;
        public ExtData ExtData;
        public BinaryFiles BinaryFiles;

        public void Read(ByteReader reader)
        {
            Console.WriteLine(reader.Tell());
            string magic = reader.ReadAscii(4);

            if (magic == "PAMU") Settings.Unicode = true;
            else if (magic == "PAME") Settings.Unicode = false;
            else if (magic == "CRUF") Settings.Unicode = true;
            else Logger.Log("Couldn't found any known headers: " + magic, true, ConsoleColor.Red); //Header not found


            _runtimeVersion = (short)reader.ReadUInt16();
            _runtimeSubversion = (short)reader.ReadUInt16();
            _productVersion = reader.ReadInt32();
            _productBuild = reader.ReadInt32();
            Settings.Build = _productBuild;

            Logger.Log("Fusion Build: " + _productBuild);

            var chunkList = new ChunkList();
            chunkList.OnHandleChunk += (id, loader) => { chunkList.HandleChunk(id, loader, this); };
            chunkList.OnChunkLoaded += (id, loader) =>
            {
                switch (id)
                {
                    case 8739: //AppHeader
                        Header = loader as AppHeader;
                        break;
                    case 8740: //AppName
                        Name = (loader as AppName)?.value;
                        break;
                    case 8741: //AppAuthor
                        Author = (loader as AppAuthor)?.value;
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
                        EditorFilename = (loader as EditorFilename)?.value;
                        if (Settings.Build > 284)
                            Decryption.MakeKey(Name, Copyright, EditorFilename);
                        else
                            Decryption.MakeKey(EditorFilename, Name, Copyright);
                        break;
                    case 8751: //AppTargetFilename
                        TargetFilename = (loader as TargetFilename)?.value;
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
                        AboutText = (loader as AboutText)?.value;
                        break;
                    case 8763: //Copyright
                        Copyright = (loader as Copyright)?.value;
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
                    case 8792: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 8793: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 13107: //Frame
                        Frames.Add(loader as Frame);
                        break;
                    case 26214: //ImageBank
                        Images = loader as ImageBank;
                        break;
                    case 26215: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 26216: //SoundBank
                        Sounds = loader as SoundBank;
                        break;
                    case 8790: //TwoFivePlusProperties
                        FrameItems = TwoFilePlusContainer.instance.objectsContainer;
                        break;
                }
            };
            chunkList.Read(reader);
            // reading again if we encounter an F3 game that uses a separate chunk list for images and sounds
            // it's safe to just read again
            chunkList.Read(reader);

        }
    }
}
