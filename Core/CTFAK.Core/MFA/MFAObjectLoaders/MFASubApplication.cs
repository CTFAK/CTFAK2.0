using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFASubApplication : ObjectLoader
    {
        string Name = ""; //unicode
        int FrameNumber;

        public override void Read()
        {
            Name = reader.AutoReadUnicode();
            FrameNumber = reader.ReadInt32();
            reader.Skip(8);

            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            reader.AutoReadUnicode();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt64();
            reader.Skip(24);

            reader.ReadInt64();
            reader.ReadInt32();
            reader.ReadInt64();
            reader.ReadInt16();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();

            reader.Skip(24);

            reader.ReadInt64();
            reader.ReadInt32();
            reader.ReadInt16();

            reader.ReadAscii(4);

            reader.ReadInt32();
            reader.ReadInt64();
            reader.ReadInt32();

            reader.ReadInt16();
            reader.ReadInt16();

            reader.AutoReadUnicode();

            reader.ReadInt32();
            reader.ReadInt64();

            reader.ReadInt16();

            reader.ReadAscii(4);

            reader.ReadInt32();

            reader.Skip(2);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.Write(0x00);
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(FrameNumber);
            Writer.Skip(8);
            Writer.WriteInt64(2305843009213693952);
            Writer.WriteInt32(16777216);
            Writer.WriteInt32(234881024);
            Writer.WriteInt64(44053479555072);
            Writer.WriteInt64(72057589742960648);
            Writer.WriteUInt64(18446744073709551615);
            Writer.WriteUInt64(18446744073709551615);
            Writer.WriteInt32(65535);
            Writer.Skip(1);
            Writer.WriteInt64(72057594037927936);
            Writer.WriteInt32(184549376);
            Writer.WriteUInt16(0);
            Writer.AutoWriteUnicode("Movement");
            Writer.WriteInt32(-2147483648);
            Writer.WriteInt64(111669149696);
            Writer.WriteInt64(4294967296);
            Writer.WriteUInt64(9223372036854775808);
            Writer.WriteUInt64(2061584302464);
            Writer.WriteUInt16(65535);
            Writer.WriteUInt64(4503599627371533);
            Writer.WriteInt64(1407374883618816);
            Writer.WriteInt64(281474976739328);
            Writer.WriteUInt64(18374686479671623680);
            Writer.WriteInt32(117440511);
            Writer.WriteInt8(4);
            Writer.WriteUInt16(0);
            Writer.WriteAscii("EvOb");
            Writer.WriteUInt64(1);
            Writer.WriteInt32(589825);
            Writer.WriteInt8(16);
            Writer.WriteUInt16(0);
            Writer.AutoWriteUnicode(Name);
            Writer.WriteUInt64(2147483648);
            Writer.WriteInt32(-65536);
            Writer.WriteUInt16(65535);
        }

        public MFASubApplication(ByteReader reader) : base(reader) { }
    }
}
