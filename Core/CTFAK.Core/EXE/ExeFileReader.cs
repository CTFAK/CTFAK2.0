using CTFAK.CCN;
using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.CCN.Chunks;
using TsudaKageyu;
using System.Runtime.CompilerServices;

namespace CTFAK.FileReaders
{
    public class ExeFileReader : IFileReader
    {
        public string Name => "Normal EXE";

        public GameData game;
        public Dictionary<int, Bitmap> Icons = new Dictionary<int, Bitmap>();


        public void LoadGame(string gamePath)
        {
            CTFAKCore.currentReader = this;
            Settings.gameType = Settings.GameType.NORMAL;
            var icoExt = new IconExtractor(gamePath);
            var icos = IconUtil.Split(icoExt.GetIcon(0));
            
            foreach (var icon in icos)
            {
                if (IconUtil.GetBitCount(icon) == 8)
                    Icons.TryAdd(icon.Width + 1, icon.ToBitmap());
                else
                    Icons.TryAdd(icon.Width, icon.ToBitmap());
            }

            // 256c 16x16
            if (!Icons.ContainsKey(17))
                Icons.Add(17, Icons.Last().Value.ResizeImage(new Size(16, 16)));
            // 256c 32x32
            if (!Icons.ContainsKey(33))
                Icons.Add(33, Icons.Last().Value.ResizeImage(new Size(32, 32)));
            // 256c 48x48
            if (!Icons.ContainsKey(49))
                Icons.Add(49, Icons.Last().Value.ResizeImage(new Size(48, 48)));

            // 32-Bit 16x16
            if (!Icons.ContainsKey(16))
                Icons.Add(16, Icons.Last().Value.ResizeImage(new Size(16, 16)));
            // 32-Bit 32x32
            if (!Icons.ContainsKey(32))
                Icons.Add(32, Icons.Last().Value.ResizeImage(new Size(32, 32)));
            // 32-Bit 48x48
            if (!Icons.ContainsKey(48))
                Icons.Add(48, Icons.Last().Value.ResizeImage(new Size(48, 48)));
            // 32-Bit 128x128
            if (!Icons.ContainsKey(128))
                Icons.Add(128, Icons.Last().Value.ResizeImage(new Size(128, 128)));
            // 32-Bit 256x256
            if (!Icons.ContainsKey(256))
                Icons.Add(256, Icons.Last().Value.ResizeImage(new Size(256, 256)));


            var reader = new ByteReader(gamePath, System.IO.FileMode.Open);
            ReadHeader(reader);
            PackData packData=null;
            if (Settings.Old)
            {
                Settings.Unicode = false;
                if (reader.PeekInt32() != 1162690896)//PAME magic
                {
                    while (true)
                    {
                        if (reader.Tell() >= reader.Size()) break;
                        var ID = reader.ReadInt16();
                        var flag = reader.ReadInt16();
                        var size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        //var newChunk = new Chunk(reader);
                        //var chunkData = newChunk.Read();
                        if (ID == 32639) break;
                    } 
                }
                
                
            }
            else
            {
                packData = new PackData();
                packData.Read(reader);
                
            }
            
            game = new GameData();
            game.Read(reader);
            if(!Settings.Old)game.packData = packData;
        }

        public int ReadHeader(ByteReader reader)
        {
            var entryPoint = CalculateEntryPoint(reader);
            reader.Seek(0);
            byte[] exeHeader = reader.ReadBytes(entryPoint);


            var firstShort = reader.PeekUInt16();
            if (firstShort == 0x7777) Settings.gameType = Settings.GameType.NORMAL;
            else/* if (firstShort == 0x222c)*/ Settings.gameType = Settings.GameType.MMF15;
            if(Settings.Old)Logger.Log($"1.5 game detected. First short: {firstShort.ToString("X")}");
            return (int)reader.Tell();
        }
        public int CalculateEntryPoint(ByteReader exeReader)
        {
            var sig = exeReader.ReadAscii(2);
            if (sig != "MZ") Logger.Log("Invalid executable signature", true, ConsoleColor.Red);

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

        public GameData getGameData()
        {
            return game;
        }

        public Dictionary<int,Bitmap> getIcons()
        {
            return Icons;
        }

        public void PatchMethods()
        {
        }

        public IFileReader Copy()
        {
            ExeFileReader reader = new ExeFileReader();
            reader.game = game;
            reader.Icons = Icons;
            return reader;
        }
    }
}

