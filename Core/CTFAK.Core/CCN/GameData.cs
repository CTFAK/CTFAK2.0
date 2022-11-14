using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CTFAK.EXE;
using CTFAK.MMFParser.EXE.Loaders;
using Ionic.Zlib;

namespace CTFAK.CCN
{
    public class GameData
    {
        public static event SaveHandler OnChunkLoaded;
        public static event SaveHandler OnFrameLoaded;
        
        private short runtimeVersion;
        private short runtimeSubversion;
        private int productVersion;
        private int productBuild;

        public string name;
        public string author="";
        public string copyright;
        public string aboutText;
        public string doc;

        public string editorFilename;
        public string targetFilename;

        //public ExeOnly Exe_Only;

        public AppMenu menu;
        //public AppIcon Icon;

        public AppHeader header;
        //public ExtentedHeader ExtHeader;

        public FontBank Fonts;
        public SoundBank Sounds;
        public MusicBank Music;
        public ImageBank Images=new ImageBank();

        //public GlobalValues GValues;
        //public GlobalStrings GStrings;

        //public Extensions Ext;

        public Dictionary<int,ObjectInfo> frameitems = new Dictionary<int, ObjectInfo>();

        public List<Frame> frames = new List<Frame>();
        public FrameHandles frameHandles;
        public Extensions extensions;

        public PackData packData;
        public Shaders shaders;
        public GlobalStrings globalStrings;
        public GlobalValues globalValues;
        public ExtData extData;
        public BinaryFiles binaryFiles;

        public void Read(ByteReader reader)
        {
            Console.WriteLine(reader.Tell());
            string magic = reader.ReadAscii(4);
            
            if (magic == "PAMU") Settings.Unicode = true;
            else if (magic == "PAME") Settings.Unicode = false;
            else if (magic == "CRUF") Settings.Unicode = true;
            else Logger.Log("Couldn't found any known headers: "+magic, true, ConsoleColor.Red);//Header not found
            
            
            runtimeVersion = (short)reader.ReadUInt16();
            runtimeSubversion = (short)reader.ReadUInt16();
            productVersion = reader.ReadInt32();
            productBuild = reader.ReadInt32();
            Settings.Build = productBuild;
            
            Logger.Log("Fusion Build: "+productBuild);
            
            var chunkList = new ChunkList();
            chunkList.OnHandleChunk += (id, loader) =>
            {
                chunkList.HandleChunk(id,loader,this);
            };
            chunkList.OnChunkLoaded += (id, loader) =>
            {
                switch (id)
                {
                    case 8739: //AppHeader
                        header = loader as AppHeader;
                        break;
                    case 8740: //AppName
                        name = (loader as AppName)?.value;
                        break;
                    case 8741: //AppAuthor
                        author = (loader as AppAuthor)?.value;
                        break;
                    case 8742: //AppMenu
                        menu = loader as AppMenu;
                        break;
                    case 8743: //ExtPath;
                        var extPath = loader as ExtPath;
                        break;
                    case 8744: //Extensions
                        break;
                    case 8745: //FrameItems
                        frameitems = (loader as FrameItems)?.Items;
                        break;
                    case 8746: //GlobalEvents
                        break;
                    case 8747: //FrameHandler
                        frameHandles = loader as FrameHandles;
                        break;
                    case 8748: //ExtData
                        extData = loader as ExtData;
                        break;
                    case 8749: //AdditionalExtension
                        break;
                    case 8750: //AppEditorFilename
                        editorFilename = (loader as EditorFilename)?.value;
                        if (Settings.Build > 284) 
                            Decryption.MakeKey(name, copyright, editorFilename);
                        else
                            Decryption.MakeKey(editorFilename, name, copyright);
                        break;
                    case 8751: //AppTargetFilename
                        targetFilename = (loader as TargetFilename)?.value;
                        break;
                    case 8752: //AppDoc
                        break;
                    case 8753: //OtherExts
                        break;
                    case 8754: //GlobalValues
                        globalValues = loader as GlobalValues;
                        break;
                    case 8755: //GlobalStrings
                        globalStrings=loader as GlobalStrings;
                        break;
                    case 8756: //Extensions2
                        extensions = loader as Extensions;
                        break;
                    case 8757: //AppIcon
                        break;
                    case 8758: //DemoVersion
                        break;
                    case 8759: //SecNum
                        break;
                    case 8760: //BinaryFiles
                        binaryFiles=loader as BinaryFiles;
                        break;
                    case 8761: //AppMenuImages:
                        break;
                    case 8762: //AboutText
                        aboutText = (loader as AboutText)?.value;
                        break;
                    case 8763: //Copyright
                        copyright = (loader as Copyright)?.value;
                        break;
                    case 8764: //GlobalValueNames
                        break;
                    case 8765: //GlobalStringNames
                        break;
                    case 8766: //MvtTexts
                        break;
                    case 8767: //FrameItems2
                        frameitems = (loader as FrameItems2)?.Items;
                        break;
                    case 8792: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 8793: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 13107: //Frame
                        frames.Add(loader as Frame);
                        break;
                    case 26214: //ImageBank
                        Images=loader as ImageBank;
                        break;
                    case 26215: //FontBank
                        Fonts = loader as FontBank;
                        break;
                    case 26216: //SoundBank
                        Sounds = loader as SoundBank;
                        break;
                    case 8790: //TwoFivePlusProperties
                        frameitems = TwoFilePlusContainer.instance.objectsContainer;
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
