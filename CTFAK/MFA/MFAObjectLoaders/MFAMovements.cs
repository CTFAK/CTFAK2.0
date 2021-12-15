using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFAMovements : ChunkLoader
    {
        public List<MFAMovement> Items = new List<MFAMovement>();
        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt32((uint)Items.Count);
            foreach (MFAMovement movement in Items)
            {
                movement.Write(Writer);
            }
        }



        public override void Read()
        {
            var count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new MFAMovement(reader);
                item.Read();
                Items.Add(item);

            }


        }
        public MFAMovements(ByteReader reader) : base(reader) { }
    }

    public class MFAMovement : ChunkLoader
    {
        public string Name = "ERROR";
        public string Extension;
        public uint Identifier;
        public ushort Player;
        public ushort Type;
        public byte MovingAtStart = 1;
        public int DirectionAtStart;
        public int DataSize;
        public byte[] extData = new byte[14];
        public MovementLoader Loader;

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.AutoWriteUnicode(Extension);
            Writer.WriteUInt32(Identifier);
            var newWriter = new ByteWriter(new MemoryStream());

            newWriter.WriteUInt16(Player);
            newWriter.WriteUInt16(Type);
            newWriter.WriteInt8(MovingAtStart);
            newWriter.Skip(3);
            newWriter.WriteInt32(DirectionAtStart);

            // newWriter.WriteBytes(extData);


            Loader?.Write(newWriter);
            newWriter.Skip(12);
            newWriter.WriteInt16(0);
            Writer.WriteInt32((int)newWriter.Size());
            Writer.WriteWriter(newWriter);


        }



        public override void Read()
        {
            Name = reader.AutoReadUnicode();
            Extension = reader.AutoReadUnicode();
            Identifier = reader.ReadUInt32();
            DataSize = (int)reader.ReadUInt32();
            if (Extension.Length > 0)
            {
                extData = reader.ReadBytes(DataSize);
            }
            else
            {
                Player = reader.ReadUInt16();
                Type = reader.ReadUInt16();
                MovingAtStart = reader.ReadByte();
                reader.Skip(3);
                DirectionAtStart = reader.ReadInt32();
                extData = reader.ReadBytes(DataSize - 12);
                switch (Type)
                {
                    case 1:
                        Loader = new Mouse(new ByteReader(extData));
                        break;
                    case 5:
                        Loader = new MovementPath(new ByteReader(extData));
                        break;
                    case 4:
                        Loader = new Ball(new ByteReader(extData));
                        break;
                }

                Loader?.Read();
            }

        }
        public MFAMovement(ByteReader reader) : base(reader) { }
    }
}
