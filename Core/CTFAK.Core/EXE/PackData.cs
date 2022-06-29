using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace CTFAK.EXE
{
    public class PackData
    {
        public List<PackFile> Items = new List<PackFile>();
        private byte[] _header;
        public uint FormatVersion;
        public void Read(ByteReader reader)
        {
            long start = reader.Tell();
            _header = reader.ReadBytes(8);

            // reader.Skip(8);
            uint headerSize = reader.ReadUInt32();
            Debug.Assert(headerSize == 32);
            uint dataSize = reader.ReadUInt32();

            reader.Seek((int)(start + dataSize - 32));
            var uheader = reader.ReadAscii(4);
            if (uheader == "PAMU")
            {
                Settings.gameType = Settings.GameType.NORMAL;
                Settings.Unicode = true;
            }
            else if (uheader == "PAME")
            {
                Settings.gameType = Settings.GameType.MMF2;
                Settings.Unicode = false;
            }
            reader.Seek(start + 16);

            FormatVersion = reader.ReadUInt32();
            var check = reader.ReadInt32();
            Debug.Assert(check == 0);
            check = reader.ReadInt32();
            Debug.Assert(check == 0);

            uint count = reader.ReadUInt32();


            long offset = reader.Tell();
            for (int i = 0; i < count; i++)
            {
                if (!reader.Check(2)) break;
                UInt16 value = reader.ReadUInt16();
                if (!reader.Check(value)) break;
                reader.ReadBytes(value);
                reader.Skip(value);
                if (!reader.Check(value)) break;
            }

            var newHeader = reader.ReadAscii(4);
            bool hasBingo = newHeader != "PAME" && newHeader != "PAMU";

            reader.Seek(offset);
            for (int i = 0; i < count; i++)
            {
                var item = new PackFile();
                item.HasBingo = hasBingo;
                item.Read(reader);
                Items.Add(item);

            }

        }
    }
    public class PackFile
    {
        public string PackFilename = "ERROR";
        int _bingo = 0;
        public byte[] Data;
        public bool HasBingo;
        public void Read(ByteReader exeReader)
        {
            UInt16 len = exeReader.ReadUInt16();
            PackFilename = exeReader.ReadWideString(len);
            Console.WriteLine(PackFilename);
            _bingo = exeReader.ReadInt32();
            Data = exeReader.ReadBytes(exeReader.ReadInt32());
            //File.WriteAllBytes($"my ass-{PackFilename}",ZlibStream.UncompressBuffer(Data));

            //Dump();
        }
    }
}
