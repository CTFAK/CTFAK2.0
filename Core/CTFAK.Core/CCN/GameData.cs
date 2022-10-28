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
            string magic = reader.ReadAscii(4); //Reading header
            //Checking for header
            if (magic == "PAMU") Settings.Unicode = true;//PAMU
            else if (magic == "PAME") Settings.Unicode = false;//PAME
            else if (magic == "CRUF") Settings.Unicode = true;
            else Logger.Log("Couldn't found any known headers: "+magic, true, ConsoleColor.Red);//Header not found
            Logger.Log("Game Header: "+magic);
            runtimeVersion = (short)reader.ReadUInt16();
            runtimeSubversion = (short)reader.ReadUInt16();
            productVersion = reader.ReadInt32();
            productBuild = reader.ReadInt32();
            Settings.Build = productBuild;
            Logger.Log("Fusion Build: "+productBuild);
            string gameExeName = Path.GetFileName(Core.path);
            if (Core.parameters.Contains("-trace_chunks"))
                Directory.CreateDirectory($"CHUNK_TRACE\\{gameExeName}");
            var chunkList = new ChunkList();
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
                        {
                            Decryption.MakeKey(name, copyright, editorFilename);
                            Logger.Log($"Generating a decryption key: {name}, {copyright}, {editorFilename}");
                        }
                        else
                        {
                            Decryption.MakeKey(editorFilename, name, copyright);
                            Logger.Log($"Generating a decryption key: {editorFilename}, {name}, {copyright}");
                        }
                        
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
                    
                    case 8789:
                        frameitems = TwoFilePlusContainer.instance.objectsContainer;
                        break;
                    
                        
                    
                }
            };
            chunkList.Read(reader);
            chunkList.Read(reader);//ONLY FOR F3 ONLY FOR F3 ONLY FOR F3



            /*switch (newChunk.Id)
            {
                case 4386:
                    //TODO: CHUNK_PREVIEW
                    break;
                case 8739:
                    header = new AppHeader();
                    header.Read(chunkReader);
                    break;
                case 8740:
                    var appname = new AppName();
                    appname.Read(chunkReader);
                    name = appname.value;
                    break;
                case 8741:
                    var appauthor = new AppAuthor();
                    appauthor.Read(chunkReader);
                    author = appauthor.value;
                    break;
                case 8742:
                    menu = new AppMenu();
                    menu.Read(chunkReader);
                    break;
                case 8743:
                    var extPath = new ExtPath();
                    extPath.Read(chunkReader);
                    break;
                case 8744:
                    //TODO: CHUNK_EXTENSIONS
                    break;
                case 8745:
                    var count = chunkReader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var newObjInfo = new ObjectInfo();
                        newObjInfo.Read(chunkReader);
                        frameitems.Add(newObjInfo.handle, newObjInfo);
                        
                    }
                    
                    break;
                case 8746:
                    //TODO: CHUNK_GLOBALEVENT
                    break;
                case 8747:
                    frameHandles = new FrameHandles();
                    frameHandles.Read(chunkReader);
                    break;
                case 8748:
                    extData = new ExtData();
                    extData.Read(chunkReader);
                    //TODO: CHUNK_EXTDATA
                    break;
                case 8749:
                //TODO: CHUNK_ADDITIONAL_EXTENSION
                case 8750:
                    var editorFile = new EditorFilename();
                    editorFile.Read(chunkReader);
                    editorFilename = editorFile.value;
                    if (Settings.Build > 284) Decryption.MakeKey(name, copyright, editorFilename);
                    else Decryption.MakeKey(editorFilename, name, copyright);
                    break;
                case 8751:
                    var trgtFile = new TargetFilename();
                    trgtFile.Read(chunkReader);
                    targetFilename = trgtFile.value;
                    break;
                case 8752:
                    //TODO: CHUNK_APPDOC
                    break;
                case 8753:
                    //TODO: CHUNK_OTHEREXTS
                    break;
                case 8754:
                    globalValues = new GlobalValues();
                    globalValues.Read(chunkReader);
                    break;
                case 8755:
                    
                    //File.WriteAllBytes("anus2000.bin",chunkReader.ReadBytes());
                    //globalStrings = new GlobalStrings(chunkReader);
                    //globalStrings.Read();
                    break;
                case 8756:
                    extensions = new Extensions();
                    extensions.Read(chunkReader);
                    break;

                case 8759:

                    break;
                case 8760:
                    binaryFiles = new BinaryFiles();
                    binaryFiles.Read(chunkReader);
                    break;
                case 8763:
                    var copyrightChunk = new Copyright();
                    copyrightChunk.Read(chunkReader);
                    copyright = copyrightChunk.value;
                    break;
                case 8770:

                    break;
                case 8771:
                    shaders = new Shaders();
                    shaders.Read(chunkReader);
                    break;
                
                case 8787: //2.5+ object headers:
                    while (true)
                    {
                        
                        
                        frameitems.Add(newObject.handle,newObject);
                    }
                    break;
                case 8788: //2.5+ object names:
                    

                    break;
                case 8790: //2.5+ object properties
                    
                    break;
                case 8793:


                    break;
                case 13107:
                    var frame = new Frame();
                    frame.Read(chunkReader);
                    OnFrameLoaded?.Invoke(frames.Count,header.NumberOfFrames);
                    frames.Add(frame);
                    /*if (frame.name == "Battle")
                    {
                        
                        int index = 0;
                        int max = frame.events.Items.Count;
                        for (int i = max; i > max-0; i--)
                        {
                            frame.events.Items.Remove(frame.events.Items[i]);
                        }
                        foreach (var evGrp in frame.events.Items)
                        {
                            Console.WriteLine($"{index}");
                            foreach (var cond in evGrp.Conditions)
                            {
                                Console.WriteLine(cond);
                            }
                            foreach (var cond in evGrp.Actions)
                            {
                                Console.WriteLine(cond);
                            }
                            Console.WriteLine("--------------------------");
                            index++;
                        }
                        Console.ReadKey();
                    }
                    
                    break;

                case 26214:
                    
                    Images = new ImageBank();
                    Images.Read(chunkReader);
                    break;
                case 26215:
                    Fonts = new FontBank();
                    Fonts.Compressed = true;
                    Fonts.Read(chunkReader);
                    break;
                case 26216:
                    Sounds = new SoundBank();
                    Sounds.Read(chunkReader);
                    break;
                case 21217:
                    Music = new MusicBank();
                    Music.Read(chunkReader);
                    break;

            }*/




        }
        
    }
}
