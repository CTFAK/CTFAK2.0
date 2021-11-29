using CTFAK.CCN;
using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.FileReaders
{
    class ExeFileReader : IFileReader
    {

        public GameData game;
        public void LoadGame(ByteReader reader)
        {
            ReadHeader(reader);
            var packData = new PackData();
            packData.Read(reader);

            game = new GameData();
            game.Read(reader);
        }

        public int ReadHeader(ByteReader reader)
        {
            var entryPoint = CalculateEntryPoint(reader);
            reader.Seek(0);
            byte[] exeHeader = reader.ReadBytes(entryPoint);


            var firstShort = reader.PeekUInt16();
            bool retard = false;
            Logger.Log("First Short: " + firstShort.ToString("X2"), true, ConsoleColor.Yellow);
            //if (firstShort == 0x7777) Settings.GameType = GameType.Normal;
            //else if (firstShort == 0x222c) Settings.GameType = GameType.OnePointFive;
            return (int)reader.Tell();
        }
        public int CalculateEntryPoint(ByteReader exeReader)
        {
            var sig = exeReader.ReadAscii(2);
            Logger.Log("EXE Header: " + sig, true, ConsoleColor.Yellow);
            if (sig != "MZ") Logger.Log("Invalid executable signature", true, ConsoleColor.Red);

            exeReader.Seek(60);

            var hdrOffset = exeReader.ReadUInt16();


            exeReader.Seek(hdrOffset);
            var peHdr = exeReader.ReadAscii(2);
            Logger.Log("PE Header: " + peHdr, true, ConsoleColor.Yellow);
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
    }
}

