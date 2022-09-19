using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks
{
    public class AppMenu : ChunkLoader
    {
        public List<AppMenuItem> Items = new List<AppMenuItem>();
        public List<byte> AccelShift;
        public List<short> AccelKey;
        public List<short> AccelId;





        public override void Read(ByteReader reader)
        {
            long currentPosition = reader.Tell();
            uint headerSize = reader.ReadUInt32();
            int menuOffset = reader.ReadInt32();
            int menuSize = reader.ReadInt32();
            if (menuSize == 0) return;
            int accelOffset = reader.ReadInt32();
            int accelSize = reader.ReadInt32();
            reader.Seek(currentPosition + menuOffset);
            reader.Skip(4);

            Load(reader);

            reader.Seek(currentPosition + accelOffset);
            AccelShift = new List<byte>();
            AccelKey = new List<short>();
            AccelId = new List<short>();
            for (int i = 0; i < accelSize / 8; i++)
            {
                AccelShift.Add(reader.ReadByte());
                reader.Skip(1);
                AccelKey.Add(reader.ReadInt16());
                AccelId.Add(reader.ReadInt16());
                reader.Skip(2);
            }
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(20);
            writer.WriteInt32(20);
            //writer.WriteInt32(0);

            ByteWriter menuDataWriter = new ByteWriter(new MemoryStream());

            foreach (AppMenuItem item in Items)
            {
                item.Write(menuDataWriter);
            }


            writer.WriteUInt32((uint)menuDataWriter.BaseStream.Position + 4);
            //

            writer.WriteUInt32((uint)(24 + menuDataWriter.BaseStream.Position));
            writer.WriteInt32(AccelKey.Count * 8);
            writer.WriteInt32(0);
            writer.WriteWriter(menuDataWriter);

            for (Int32 i = 0; i < AccelKey.Count; i++)
            {
                writer.WriteInt8(AccelShift[i]);
                writer.WriteInt8(0);
                writer.WriteInt16(AccelKey[i]);
                writer.WriteInt16(AccelId[i]);
                writer.WriteInt16(0);
            }

        }

        public void Load(ByteReader reader)
        {
            while (true)
            {
                AppMenuItem newItem = new AppMenuItem();
                newItem.Read(reader);
                Items.Add(newItem);

                // if (newItem.Name.Contains("About")) break;
                if (ByteFlag.GetFlag((uint)newItem.Flags, 4))
                {
                    Load(reader);
                }

                if (ByteFlag.GetFlag((uint)newItem.Flags, 7))
                {
                    break;
                }
            }
        }
    }

    public class AppMenuItem : ChunkLoader
    {
        public string Name = "";
        public Int16 Flags = 0;
        public Int16 Id = 0;
        public string Mnemonic = null;



        public override void Read(ByteReader reader)
        {
            Flags = reader.ReadInt16();
            if (!ByteFlag.GetFlag((uint)Flags, 4))
            {
                Id = reader.ReadInt16();
            }

            Name = reader.ReadWideString();

            for (int i = 0; i < Name.Length; i++)
            {
                if (Name[i] == '&')
                {
                    Mnemonic = Name[i + 1].ToString();
                    Name = Name.Replace("&", "");
                    break;
                }
            }


        }

        public void Load()
        {
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt16(Flags);
            if (!ByteFlag.GetFlag((uint)Flags, 4))
            {
                writer.WriteInt16(Id);
            }

            String MName = Name;
            if (Mnemonic != null)
            {
                MName = MName.ReplaceFirst(Mnemonic, "&" + Mnemonic);
            }

            writer.WriteUnicode(MName, true);
        }
    }
}
