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

namespace CTFAK.FileReaders
{
    public class ExeFileReader : IFileReader
    {
        public string Name => "Normal EXE";

        public GameData game;
        public Dictionary<int, Bitmap> icons = new Dictionary<int, Bitmap>();
        

        public void LoadGame(string gamePath)
        {
            Core.currentReader = this;
            Settings.gameType = Settings.GameType.NORMAL;
            var icoExt = new IconExtractor(gamePath);
            var icos = icoExt.GetAllIcons();
            foreach (var icon in icos)
            {
                
                icons.Add(icon.Width, icon.ToBitmap());
            }

            if (!icons.ContainsKey(16)) icons.Add(16, icons[32].resizeImage(new Size(16, 16)));
            if (!icons.ContainsKey(48)) icons.Add(48, icons[32].resizeImage(new Size(48, 48)));
            if (!icons.ContainsKey(128)) icons.Add(128, icons[32].resizeImage(new Size(128, 128)));
            if (!icons.ContainsKey(256)) icons.Add(256, icons[32].resizeImage(new Size(256, 256)));


            var reader = new ByteReader(gamePath, System.IO.FileMode.Open);
            ReadHeader(reader);
            PackData packData=null;
            if (Settings.Old)
            {
                Settings.Unicode = false;
                while (true)
                {
                    if (reader.Tell() >= reader.Size()) break;
                    var newChunk = new Chunk(reader);
                    var chunkData = newChunk.Read();
                    if (newChunk.Id == 32639) break;
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
            else if (firstShort == 0x222c) Settings.gameType = Settings.GameType.MMF15;
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
            return icons;
        }

        public void PatchMethods()
        {
        }
    }
}

