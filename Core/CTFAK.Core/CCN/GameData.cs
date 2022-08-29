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
        public static event Core.SaveHandler OnChunkLoaded;
        public static event Core.SaveHandler OnFrameLoaded;
        
        
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
        public ImageBank Images=new ImageBank(null);

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
            else Logger.Log("Couldn't found any known headers: "+magic, true, ConsoleColor.Red);//Header not found
            runtimeVersion = (short)reader.ReadUInt16();
            runtimeSubversion = (short)reader.ReadUInt16();
            productVersion = reader.ReadInt32();
            productBuild = reader.ReadInt32();
            Settings.Build = productBuild;
            Logger.Log("Fusion Build: "+productBuild);
            string gameExeName = Path.GetFileName(Core.path);
            if (Core.parameters.Contains("-trace_chunks"))
                Directory.CreateDirectory($"CHUNK_TRACE\\{gameExeName}");
            int chunkIndex = 0;
            List<Task> readingTasks = new List<Task>();
            while(true)
            {
                OnChunkLoaded?.Invoke(chunkIndex,0);
                if (reader.Tell() >= reader.Size()) break;
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                if (newChunk.Id == 32639) break;
                if (newChunk.Id == 8787) Settings.gameType = Settings.GameType.TWOFIVEPLUS; 
            


                    var chunkReader = new ByteReader(chunkData);



                    chunkIndex++;
                    if (Core.parameters.Contains("-trace_chunks"))
                    {
                        string chunkName = "";
                        if (!ChunkList.ChunkNames.TryGetValue(newChunk.Id, out chunkName))
                        {
                            chunkName = $"UNKOWN-{newChunk.Id}";
                        }

                        Logger.Log(
                            $"Encountered chunk: {chunkName}, chunk flag: {newChunk.Flag}, exe size: {newChunk.Size}, decompressed size: {chunkData.Length}");
                        File.WriteAllBytes($"CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin",chunkData);
                        Logger.Log($"Raw chunk data written to CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin");
                    }

                    switch (newChunk.Id)
                    {
                        case 4386:
                            //TODO: CHUNK_PREVIEW
                            break;
                        case 8739:
                            header = new AppHeader(chunkReader);
                            header.Read();
                            break;
                        case 8740:
                            var appname = new AppName(chunkReader);
                            appname.Read();
                            name = appname.value;
                            break;
                        case 8741:
                            var appauthor = new AppAuthor(chunkReader);
                            appauthor.Read();
                            author = appauthor.value;
                            break;
                        case 8742:
                            menu = new AppMenu(chunkReader);
                            menu.Read();
                            break;
                        case 8743:
                            var extPath = new ExtPath(chunkReader);
                            extPath.Read();
                            break;
                        case 8744:
                            //TODO: CHUNK_EXTENSIONS
                            break;
                        case 8745:
                            var count = chunkReader.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                var newObjInfo = new ObjectInfo(chunkReader);
                                newObjInfo.Read();
                                frameitems.Add(newObjInfo.handle, newObjInfo);
                                
                            }
                            
                            break;
                        case 8746:
                            //TODO: CHUNK_GLOBALEVENT
                            break;
                        case 8747:
                            frameHandles = new FrameHandles(chunkReader);
                            frameHandles.Read();
                            break;
                        case 8748:
                            extData = new ExtData(chunkReader);
                            extData.Read();
                            //TODO: CHUNK_EXTDATA
                            break;
                        case 8749:
                        //TODO: CHUNK_ADDITIONAL_EXTENSION
                        case 8750:
                            var editorFile = new EditorFilename(chunkReader);
                            editorFile.Read();
                            editorFilename = editorFile.value;
                            if (Settings.Build > 284) Decryption.MakeKey(name, copyright, editorFilename);
                            else Decryption.MakeKey(editorFilename, name, copyright);
                            break;
                        case 8751:
                            var trgtFile = new TargetFilename(chunkReader);
                            trgtFile.Read();
                            targetFilename = trgtFile.value;
                            break;
                        case 8752:
                            //TODO: CHUNK_APPDOC
                            break;
                        case 8753:
                            //TODO: CHUNK_OTHEREXTS
                            break;
                        case 8754:
                            globalValues = new GlobalValues(chunkReader);
                            globalValues.Read();
                            break;
                        case 8755:
                            
                            //File.WriteAllBytes("anus2000.bin",chunkReader.ReadBytes());
                            //globalStrings = new GlobalStrings(chunkReader);
                            //globalStrings.Read();
                            break;
                        case 8756:
                            extensions = new Extensions(chunkReader);
                            extensions.Read();
                            break;

                        case 8759:

                            break;
                        case 8760:
                            binaryFiles = new BinaryFiles(chunkReader);
                            binaryFiles.Read();
                            break;
                        case 8763:
                            var copyrightChunk = new Copyright(chunkReader);
                            copyrightChunk.Read();
                            copyright = copyrightChunk.value;
                            break;
                        case 8770:

                            break;
                        case 8771:
                            shaders = new Shaders(chunkReader);
                            shaders.Read();
                            break;
                        
                        case 8787: //2.5+ object headers:
                            while (true)
                            {
                                if (chunkReader.Tell() >= chunkReader.Size()) break;
                                var newObject = new ObjectInfo(null);
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
                                    newObject.rgbCoeff = Color.FromArgb(0, r, g, b);
                                    newObject.blend = chunkReader.ReadByte();
                                }
                                else
                                {
                                    var flag = chunkReader.ReadByte();
                                    chunkReader.Skip(2);
                                    newObject.InkEffectValue = chunkReader.ReadByte();
                                    chunkReader.Skip(3);
                                }
                                
                                frameitems.Add(newObject.handle,newObject);
                            }
                            break;
                        case 8788: //2.5+ object names:
                            var nstart = chunkReader.Tell();
                            
                            var nend = nstart + chunkReader.Size();
                            //chunkReader.ReadInt32();
                            int ncurrent = 0;
                            while (chunkReader.Tell() < nend)
                            {
                                
                                var newName = "sex";
                                
                                frameitems[ncurrent].name = chunkReader.ReadWideString();
                                ncurrent++;
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
                                Console.WriteLine("reading object props: "+current);
                                var chunkSize = chunkReader.ReadInt32();
                                var data = chunkReader.ReadBytes(chunkSize);
                                var decompressed = ZlibStream.UncompressBuffer(data);
                                var decompressedReader = new ByteReader(decompressed);

                                var objectData = frameitems[current];

                                if (objectData.ObjectType == 0)
                                    objectData.properties = new Quickbackdrop(decompressedReader);
                                else if (objectData.ObjectType == 1)
                                    objectData.properties = new Backdrop(decompressedReader);
                                else objectData.properties = new ObjectCommon(decompressedReader);
                                objectData.properties.Read();
                                chunkReader.Seek(currentPosition+chunkSize+8);
                                //else properties = new ObjectCommon(chunkReader, null);

                                //properties?.Read();
                                current++;
                            }
                            break;
                        case 8793:


                            break;
                        case 13107:
                            var frame = new Frame(chunkReader);
                            frame.Read();
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
                            }*/
                            
                            break;

                        case 26214:
                            
                            Images = new ImageBank(chunkReader);
                            Images.Read();
                            break;
                        case 26215:
                            Fonts = new FontBank(chunkReader);
                            Fonts.Compressed = true;
                            Fonts.Read();
                            break;
                        case 26216:
                            Sounds = new SoundBank(chunkReader);
                            Sounds.Read();
                            break;
                        case 21217:

                            Music = new MusicBank(chunkReader);
                            Music.Read();
                            break;

                    }

                

            }

            foreach (var readingTask in readingTasks)
            {
                readingTask.Wait();
            }

        }
        
    }
}
