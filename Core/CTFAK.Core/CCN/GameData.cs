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
using CTFAK.FileReaders;
using CTFAK.Core.CCN.Chunks;
using System.Reflection.Metadata.Ecma335;
using CTFAK.Core.CCN.Chunks.Banks.ImageBank;
using CTFAK.Core.CCN.Chunks.Banks.SoundBank;

namespace CTFAK.CCN
{
    public class GameData
    {
        public static event CTFAKCore.SaveHandler OnChunkLoaded;
        public static event CTFAKCore.SaveHandler OnFrameLoaded;
        public static string builddate = "7/17/23";

        public short runtimeVersion;
        public short runtimeSubversion;
        public int productVersion;
        public int productBuild;

        public string name;
        public string author="";
        public string copyright;
        public string aboutText;
        public string doc;

        public string editorFilename;
        public string targetFilename;

        public bool Exe_Only;

        public AppMenu menu;
        public Bitmap Icon;

        public AppHeader header;
        public ExtendedHeader ExtHeader;

        public ChunkOffsets chunkOffsets;

        public FontBank Fonts;
        public SoundBank Sounds;
        public MusicBank Music;
        public ImageBank Images = new ImageBank();

        public Dictionary<int,ObjectInfo> frameitems = new Dictionary<int, ObjectInfo>();

        public List<Frame> frames = new List<Frame>();
        public FrameHandles frameHandles;
        public Extensions extensions;

        public PackData packData = new PackData();
        public Shaders shaders = new Shaders();
        public GlobalStrings globalStrings;
        public GlobalValues globalValues;
        public ExtData extData;
        public BinaryFiles binaryFiles = new();
        public TrueTypeFonts ttfs;

        public void Read(ByteReader reader)
        {
            Logger.Log($"Running {builddate} build.", false);
            string magic = reader.ReadAscii(4); //Reading header
            //Checking for header
            if (magic == "PAMU") Settings.Unicode = true;//PAMU
            else if (magic == "PAME") Settings.Unicode = false;//PAME
            else if (magic == "CRUF") Settings.gameType = Settings.GameType.F3;
            else Logger.Log("Couldn't found any known headers: "+magic, true, ConsoleColor.Red);//Header not found
            if (CTFAKCore.parameters.Contains("-f1.5"))
                Settings.gameType = Settings.GameType.MMF15;
            if (CTFAKCore.parameters.Contains("-android"))
                Settings.gameType = Settings.GameType.ANDROID;
            if (CTFAKCore.parameters.Contains("-f3"))
                Settings.gameType = Settings.GameType.F3;
            Logger.Log("Game Header: " + magic);
            runtimeVersion = (short)reader.ReadUInt16();
            runtimeSubversion = (short)reader.ReadUInt16();
            productVersion = reader.ReadInt32();
            productBuild = reader.ReadInt32();
            Settings.Build = productBuild;
            Logger.Log("Fusion Build: " + productBuild);
            string gameExeName = Path.GetFileName(CTFAKCore.path);
            if (CTFAKCore.parameters.Contains("-trace_chunks"))
                Directory.CreateDirectory($"CHUNK_TRACE\\{gameExeName}");
            int chunkIndex = 0;
            int frameIndex = 0;
            List<Task> readingTasks = new List<Task>();
            while(true)
            {
                OnChunkLoaded?.Invoke(chunkIndex,0);
                if (reader.Tell() >= reader.Size()) break;
                var newChunk = new Chunk();
                var chunkData = newChunk.Read(reader);
                if (newChunk.Id == 32494 && Settings.F3) Settings.Fusion3Seed = true;
                if (CTFAKCore.parameters.Contains("-onlyimages"))
                {
                    if (newChunk.Id != 26214 && // Image Bank
                        newChunk.Id != 13107 && // Frame
                        newChunk.Id != 8745  && // Frame Items
                        newChunk.Id != 8767  && // Frame Items 2
                        newChunk.Id != 8787  && // Frame Items 2.5+
                        newChunk.Id != 8788  && // Frame Item Names 2.5+
                        newChunk.Id != 8790  && // Frame Item Properties 2.5+
                        newChunk.Id != 8763  && // Copyright
                        newChunk.Id != 8750  && // Editor Filename
                        newChunk.Id != 8740)    // App Name 
                        continue;
                }
                //if (newChunk.Id == 32639) break;
                if (newChunk.Id == 8787 && !Settings.F3) Settings.gameType = Settings.GameType.TWOFIVEPLUS;
                var chunkReader = new ByteReader(chunkData);
                chunkIndex++;
                string chunkName = "";
                if (CTFAKCore.parameters.Contains("-trace_chunks"))
                {
                    if (!ChunkList.ChunkNames.TryGetValue(newChunk.Id, out chunkName))
                    {
                        chunkName = $"UNKNOWN-{newChunk.Id}";
                    }

                    Logger.Log(
                        $"Encountered chunk: {chunkName}, chunk flag: {newChunk.Flag}, exe size: {newChunk.Size}, decompressed size: {chunkData.Length}");
                    File.WriteAllBytes($"CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin",chunkData);
                    Logger.Log($"Raw chunk data written to CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin");
                }
                string log = $"Reading Chunk {newChunk.Id}";
                if (ChunkList.ChunkNames.TryGetValue(newChunk.Id, out chunkName))
                    log += $" ({chunkName})";
                if (CTFAKCore.parameters.Contains("-chunk_info"))
                    log += $" (Size: {newChunk.Size}) (Offset: {reader.Tell() - newChunk.Size})";
                Logger.Log(log);
                switch (newChunk.Id)
                {
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
                    case 8767:
                    case 8745:
                        var count = chunkReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            var newObjInfo = new ObjectInfo();
                            newObjInfo.Read(chunkReader);
                            //Logger.Log("New Frame Item: " + newObjInfo.handle);
                            frameitems.Add(newObjInfo.handle, newObjInfo);
                        }
                        break;
                    case 8747:
                        frameHandles = new FrameHandles();
                        frameHandles.Read(chunkReader);
                        break;
                    case 8748:
                        extData = new ExtData();
                        extData.Read(chunkReader);
                        break;
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
                    case 8754:
                        globalValues = new GlobalValues();
                        globalValues.Read(chunkReader);
                        break;
                    case 8755:
                        globalStrings = new GlobalStrings();
                        globalStrings.Read(chunkReader);
                        break;
                    case 8756:
                        extensions = new Extensions();
                        extensions.Read(chunkReader);
                        break;
                    case 8757:
                        Icon = AppIcon.ReadIcon(chunkReader);
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
                    case 8768:
                        Exe_Only = ExeOnly.ReadBool(chunkReader);
                        break;
                    case 8771:
                        shaders = new Shaders();
                        shaders.Read(chunkReader);
                        break;
                    case 8773:
                        ExtHeader = new ExtendedHeader();
                        ExtHeader.Read(chunkReader);
                        break;
                    //case 8774: No clue, App Code Page
                    //case 8775: No clue, Frame Offset
                    //case 8776: No clue, Ad Mob ID
                    //case 8779: No clue, Android Menu
                    case 8787: //2.5+ object headers:
                        while (true)
                        {
                            if (chunkReader.Tell() >= chunkReader.Size()) break;
                            var newObject = new ObjectInfo();
                            newObject.handle = chunkReader.ReadInt16();
                            newObject.ObjectType = chunkReader.ReadInt16();
                            newObject.Flags = chunkReader.ReadInt16();
                            chunkReader.Skip(2);
                            newObject.InkEffect = chunkReader.ReadByte();
                            if(newObject.InkEffect!=1)
                            {
                                chunkReader.Skip(3);
                                var r = chunkReader.ReadByte();
                                var g = chunkReader.ReadByte();
                                var b = chunkReader.ReadByte();
                                newObject.rgbCoeff = Color.FromArgb(0, b, g, r);
                                newObject.blend = chunkReader.ReadByte();
                            }
                            else
                            {
                                var flag = chunkReader.ReadByte();
                                chunkReader.Skip(2);
                                newObject.InkEffectValue = chunkReader.ReadByte();
                                chunkReader.Skip(3);
                            }

                            //Logger.Log("New Frame Item: " + newObject.handle);
                            frameitems.Add(newObject.handle,newObject);
                        }
                        break;
                    case 8788: //2.5+ object names:
                        var nend = chunkReader.Tell() + chunkReader.Size();
                        int ncurrent = 0;
                        while (chunkReader.Tell() < nend)
                        {
                            frameitems[ncurrent].name = chunkReader.ReadYuniversal();
                            ncurrent++;
                        }
                        break;
                    case 8789: //2.5+ Object Shaders
                        var shdrstart = chunkReader.Tell();
                        var shdrend = shdrstart + chunkReader.Size();
                        if (shdrstart == shdrend) break;

                        int shdrcurrent = 0;
                        while (true)
                        {
                            var paramStart = chunkReader.Tell() + 4;
                            if (chunkReader.Tell() == shdrend) break;
                            var size = chunkReader.ReadInt32();
                            if (size == 0)
                            {
                                shdrcurrent++;
                                continue;
                            }
                            var obj = frameitems[shdrcurrent];
                                
                            var shaderHandle = chunkReader.ReadInt32();
                            var numberOfParams = chunkReader.ReadInt32();
                            if (shaders == null) break;
                            if (!shaders.ShaderList.ContainsKey(shaderHandle)) continue;
                            var shdr = shaders.ShaderList[shaderHandle];
                            obj.shaderData.name = shdr.Name;
                            obj.shaderData.ShaderHandle = shaderHandle;
                            obj.shaderData.hasShader = true;

                            for (int i = 0; i < numberOfParams; i++)
                            {
                                if (shdr.Parameters.Count < i + 1) break;
                                var param = shdr.Parameters[i];
                                object paramValue;
                                switch (param.Type)
                                {
                                    case 1:
                                        paramValue = chunkReader.ReadSingle();
                                        break;
                                    case < 4:
                                        paramValue = chunkReader.ReadInt32();
                                        break;
                                    default:
                                        paramValue = "unknownType";
                                        break;
                                }
                                obj.shaderData.parameters.Add(new ObjectInfo.ShaderParameter()
                                {
                                    Name = param.Name,
                                    ValueType = param.Type,
                                    Value = paramValue
                                });
                            }
                            chunkReader.Seek(paramStart + size);
                            shdrcurrent++;
                        }
                        break;
                    case 8790: //2.5+ object properties
                        var start = chunkReader.Tell();
                            
                        var end = start + chunkReader.Size();
                        chunkReader.ReadInt32();
                        int current = 0;
                        while (chunkReader.Tell() < end)
                        {
                            var currentPosition = chunkReader.Tell();
                            var chunkSize = chunkReader.ReadInt32();
                            var data = chunkReader.ReadBytes(chunkSize);
                            var decompressed = ZlibStream.UncompressBuffer(data);
                            var decompressedReader = new ByteReader(decompressed);

                            var objectData = frameitems[current];

                            if (objectData.ObjectType == 0)
                                objectData.properties = new Quickbackdrop();
                            else if (objectData.ObjectType == 1)
                                objectData.properties = new Backdrop();
                            else objectData.properties = new ObjectCommon(null);
                            objectData.properties.Read(decompressedReader);
                            chunkReader.Seek(currentPosition+chunkSize+8);
                               
                            current++;
                        }
                        break;
                    case 8793:
                        ttfs = new TrueTypeFonts();
                        ttfs.Read(chunkReader);
                        break;
                    case 13107:
                        if (CTFAKCore.parameters.Contains($"-excludeframe({frameIndex++})"))
                            break;
                        var frame = new Frame();
                        frame.Read(chunkReader);
                        OnFrameLoaded?.Invoke(frames.Count,header.NumberOfFrames);
                        frames.Add(frame);
                        break;
                    case 17664:
                        var ImageShapes = new ImageShapes();
                        ImageShapes.Read(chunkReader);
                        break;
                    case 26213:
                        chunkOffsets = new ChunkOffsets();
                        chunkOffsets.Read(chunkReader);
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

                        if (CTFAKCore.parameters.Contains("-nosounds")) break;
                        if (Settings.gameType == Settings.GameType.ANDROID)
                        {
                            Sounds = ApkFileReader.androidSoundBank;
                            var AndroidSounds = new AndroidSoundBank();
                            AndroidSounds.Read(chunkReader);
                            for (int i = 0; i < Sounds.Items.Count; i++)
                                Sounds.Items[i].Name = AndroidSounds.Items[Sounds.Items[i].Handle].Name;
                        }
                        else
                            Sounds.Read(chunkReader);
                        break;
                    case 26217: // old 21217 (invalid)
                        /*Music = new MusicBank();
                        Music.Read(chunkReader);*/ //Actually go fuck yourself Log0
                        break;
                    default:
                        Logger.Log("No Reader for Chunk " + newChunk.Id);
                        if (CTFAKCore.parameters.Contains("-dumpnewchunks"))
                            File.WriteAllBytes("UnkChunks\\" + newChunk.Id + ".bin", chunkReader.ReadBytes());
                        break;
                }
            }

            foreach (var readingTask in readingTasks)
            {
                readingTask.Wait();
            }
            if (Settings.Fusion3Seed) Logger.LogWarning("Seeded Fusion 3");
        }
    }
}
