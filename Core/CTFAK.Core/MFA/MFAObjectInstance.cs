using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAObjectInstance : ChunkLoader
    {
        public int X;
        public int Y;
        public uint Layer;
        public int Handle;
        public short Flags;
        public short Instance;
        public uint ParentType;
        public uint ParentHandle;
        public uint ItemHandle;

        public override void Read(ByteReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            Layer = reader.ReadUInt32();
            Handle = reader.ReadInt32();
            Flags = reader.ReadInt16();
            Instance = reader.ReadInt16();
            ParentType = reader.ReadUInt32();
            ItemHandle = reader.ReadUInt32();
            ParentHandle = (uint)reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(X);
            Writer.WriteInt32(Y);
            Writer.WriteUInt32(Layer);
            Writer.WriteInt32(Handle);
            Writer.WriteInt16(Flags);
            Writer.WriteInt16(Instance);
            Writer.WriteUInt32(ParentType);
            Writer.WriteUInt32(ItemHandle);
            Writer.WriteInt32((int)ParentHandle);
        }


    }
}
