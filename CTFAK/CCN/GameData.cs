using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN
{
    public class GameData
    {
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
        public ImageBank images;

        //public GlobalValues GValues;
        //public GlobalStrings GStrings;

        //public Extensions Ext;

        public Dictionary<int,ObjectInfo> frameitems = new Dictionary<int, ObjectInfo>();

        public List<Frame> frames = new List<Frame>();
        public FrameHandles frameHandles;
        public Extensions extensions;

        public void Read(ByteReader reader)
        {
            string magic = reader.ReadAscii(4); //Reading header
            //Checking for header
            //if (magic == Constants.UnicodeGameHeader) Settings.Unicode = true;//PAMU
            //else if (magic == Constants.GameHeader) Settings.Unicode = false;//PAME
            //else Logger.Log("Couldn't found any known headers", true, ConsoleColor.Red);//Header not found
            runtimeVersion = (short)reader.ReadUInt16();
            runtimeSubversion = (short)reader.ReadUInt16();
            productVersion = reader.ReadInt32();
            productBuild = reader.ReadInt32();
            Settings.Build = productBuild;
            while(true)
            {
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                var chunkReader = new ByteReader(chunkData);
                if (newChunk.Id == 32639) break;
                switch (newChunk.Id)
                {
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
                    case 8756:
                        extensions = new Extensions(chunkReader);
                        extensions.Read();
                        break;
                    //FOUR CHUNKS SKIPPED FFS
                    case 8747:
                        frameHandles = new FrameHandles(chunkReader);
                        frameHandles.Read();
                        break;
                    case 8750:
                        var editorFile = new EditorFilename(chunkReader);
                        editorFile.Read();
                        editorFilename = editorFile.value;
                        if (Settings.Build > 284) Decryption.MakeKey(name, copyright, editorFilename);
                        else Decryption.MakeKey(editorFilename,name, copyright);
                        break;
                    case 8763:
                        var cprght = new Copyright(chunkReader);
                        cprght.Read();
                        copyright = cprght.value;
                        break;
                    case 13107:
                        var frame = new Frame(chunkReader);
                        frame.Read();
                        frames.Add(frame);
                        break;
                    case 8745:
                        var count = chunkReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            var newObjInfo = new ObjectInfo(chunkReader);
                            newObjInfo.Read();
                            frameitems.Add(newObjInfo.handle,newObjInfo);
                        }
                        break;
                    case 26214:
                        images = new ImageBank(chunkReader);
                        images.Read();
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

        }
        
    }
}
