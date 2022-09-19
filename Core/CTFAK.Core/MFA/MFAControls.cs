using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAControls : ChunkLoader
    {
        public List<MFAPlayerControl> Items=new List<MFAPlayerControl>();


        public override void Read(ByteReader reader)
        {
            Items = new List<MFAPlayerControl>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new MFAPlayerControl();
                Items.Add(item);
                item.Read(reader);
            }
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(Items.Count);
            foreach (var item in Items)
            {
                item.Write(writer);
            }

        }

    }

    public class MFAPlayerControl : ChunkLoader
    {
        public int ControlType;
        public int Up;
        public int Down;
        public int Left;
        public int Right;
        public int Button1;
        public int Button2;
        public int Button3;
        public int Button4;
        public int Unk1;
        public int Unk2;
        public int Unk3;
        public int Unk4;
        public int Unk5;
        public int Unk6;
        public int Unk7;
        public int Unk8;





       

        public override void Read(ByteReader reader)
        {
            ControlType = reader.ReadInt32();
            var count = reader.ReadInt32();
            Up = reader.ReadInt32();
            Down = reader.ReadInt32();
            Left = reader.ReadInt32();
            Right = reader.ReadInt32();
            Button1 = reader.ReadInt32();
            Button2 = reader.ReadInt32();
            Button3 = reader.ReadInt32();
            Button4 = reader.ReadInt32();
            Unk1 = reader.ReadInt32();
            Unk2 = reader.ReadInt32();
            Unk3 = reader.ReadInt32();
            Unk4 = reader.ReadInt32();
            Unk5 = reader.ReadInt32();
            Unk6 = reader.ReadInt32();
            Unk7 = reader.ReadInt32();
            Unk8 = reader.ReadInt32();

        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(ControlType);
            writer.WriteUInt32(16);
            writer.WriteInt32(Up);
            writer.WriteInt32(Down);
            writer.WriteInt32(Left);
            writer.WriteInt32(Right);
            writer.WriteInt32(Button1);
            writer.WriteInt32(Button2);
            writer.WriteInt32(Button3);
            writer.WriteInt32(Button4);
            writer.WriteInt32(Unk1);
            writer.WriteInt32(Unk2);
            writer.WriteInt32(Unk3);
            writer.WriteInt32(Unk4);
            writer.WriteInt32(Unk5);
            writer.WriteInt32(Unk6);
            writer.WriteInt32(Unk7);
            writer.WriteInt32(Unk8);



        }
    }
}
