using System;
using System.Runtime.InteropServices;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Group : ParameterCommon
    {
        public long Offset;
        public ushort Flags;
        public ushort Id;
        public string Name;
        public int Password;
        public byte[] Unk1;
        public byte[] Unk2;

        public Group(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
        {
            Offset = reader.Tell() - 24;
            Flags = reader.ReadUInt16();
            Id = reader.ReadUInt16();
            Name = reader.ReadWideString();
            Unk1 = reader.ReadBytes(190-Name.Length*2);
            Password = reader.ReadInt32();
            reader.ReadInt16();

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt16(Flags);
            Writer.WriteUInt16(Id);
            Writer.WriteUnicode(Name, true);
            Writer.WriteBytes(Unk1);
            var namePtr = Marshal.StringToHGlobalUni(Name);
            var passPtr = Marshal.StringToHGlobalUni("");
            var result = NativeLib.GenChecksum(namePtr, passPtr);
            Marshal.FreeHGlobal(namePtr);
            Marshal.FreeHGlobal(passPtr);
            Password = (int)result;
            Writer.WriteInt32(Password);
            Writer.WriteInt16(0);
        }

        public override string ToString()
        {
            return $"Group: {Name}";
        }
    }
}