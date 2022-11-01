using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFAExtensionObject : MFAAnimationObject
    {
        public string ExtensionName;
        public int ExtensionType;
        public string Filename;
        public uint Magic;
        public string SubType;
        public int ExtensionVersion;
        public int ExtensionId;
        public int ExtensionPrivate;
        public byte[] ExtensionData;
        private byte[] _unk;



        public override void Read(ByteReader reader)
        {
            base.Read(reader);
            ExtensionType = reader.ReadInt32();
            if (ExtensionType == -1)
            {
                ExtensionName = reader.AutoReadUnicode();
                Filename = reader.AutoReadUnicode();
                Magic = reader.ReadUInt32();
                SubType = reader.AutoReadUnicode();
            }

            var newReader = new ByteReader(reader.ReadBytes((int)reader.ReadUInt32()));
            var dataSize = newReader.ReadInt32() - 20;
            _unk = newReader.ReadBytes(4);
            ExtensionVersion = newReader.ReadInt32();
            ExtensionId = newReader.ReadInt32();
            ExtensionPrivate = newReader.ReadInt32();
            ExtensionData = newReader.ReadBytes(dataSize);

        }

        public override void Write(ByteWriter Writer)
        {
            _isExt = true;
            base.Write(Writer);
            if (ExtensionType == -1)
            {
                Writer.AutoWriteUnicode(ExtensionName);
                Writer.AutoWriteUnicode(Filename);
                Writer.WriteUInt32(Magic);
                Writer.AutoWriteUnicode(SubType);
            }

            if (ExtensionData == null) ExtensionData = new byte[1] { 0 };
            Writer.WriteInt32(ExtensionData.Length + 20);
            Writer.WriteInt32(ExtensionData.Length + 20);
            Writer.WriteInt32(-1);
            Writer.WriteInt32(ExtensionVersion);
            Writer.WriteInt32(ExtensionId);
            Writer.WriteInt32(ExtensionPrivate);
            Writer.WriteBytes(ExtensionData);


        }
    }
}
