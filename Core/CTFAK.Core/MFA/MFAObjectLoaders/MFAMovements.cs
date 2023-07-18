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



        public override void Read(ByteReader reader)
        {
            var count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new MFAMovement();
                item.Read(reader);
                Items.Add(item);

            }


        }
    }

    public class MFAMovement : ChunkLoader
    {
        public string Name = "";
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
            Writer.AutoWriteUnicode(Name); // | Movement #0
            Writer.AutoWriteUnicode(Extension); // |
            Writer.WriteUInt32(Identifier); // 5 | 5
            var newWriter = new ByteWriter(new MemoryStream());

            newWriter.WriteUInt16(Player); // 0 | 0
            newWriter.WriteUInt16(Type); // 5 | 5
            newWriter.WriteInt8(MovingAtStart); // 1 | 1
            newWriter.Skip(3);
            newWriter.WriteInt32(DirectionAtStart); // 1 | 1

            // newWriter.WriteBytes(extData);


            Loader?.Write(newWriter);
            newWriter.Skip(12);
            newWriter.WriteInt16(0);
            Writer.WriteInt32((int)newWriter.Size()); // 684 | 86
            Writer.WriteWriter(newWriter);


        }



        public override void Read(ByteReader reader)
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
                        Loader = new Mouse();
                        break;
                    case 5:
                        Loader = new MovementPath();
                        break;
                    case 4:
                        Loader = new Ball();
                        break;
                }

                Loader?.Read(new ByteReader(extData));
            }

        }
    }
}
