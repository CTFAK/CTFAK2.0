using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Core.CCN.Chunks.Banks.SoundBank
{
    public class AndroidSoundBank : ChunkLoader
    {
        public static event CTFAKCore.SaveHandler OnSoundLoaded;

        public int NumOfHandles = 0;
        public int NumOfItems = 0;
        public AndroidSoundItem[] Items = new AndroidSoundItem[0];
        public static Dictionary<int, string> oldAndroidNames = new();

        public override void Read(ByteReader reader)
        {
            NumOfHandles = reader.ReadInt16();
            NumOfItems = reader.ReadInt16();

            Items = new AndroidSoundItem[NumOfHandles];
            for (int i = 0; i < NumOfItems; i++)
            {
                var item = new AndroidSoundItem();
                item.Read(reader);
                OnSoundLoaded?.Invoke(i, NumOfItems);
                Items[item.Handle] = item;
            }
        }
        public override void Write(ByteWriter writer)
        {
            int i = 0;
            foreach (var item in Items)
                i++;

            writer.WriteInt32(i);
            foreach (var item in Items)
                item.Write(writer);
        }
    }

    public class AndroidSoundItem : SoundBase
    {
        public int Handle;
        public int Flags;
        public int Length;
        public int Frequency;
        public string Name = "";
        public byte[] Data;
        public int Size;

        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt16();
            Flags = reader.ReadInt16();
            Length = reader.ReadInt32();
            Frequency = reader.ReadInt32();

            if (Settings.Build >= 287 && (Flags & 0x100) != 0)
                Name = reader.ReadWideString(reader.ReadInt16());
            else
                Name = AndroidSoundBank.oldAndroidNames[Handle];
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteUInt32((uint)Handle);
            writer.WriteInt32(0);
            writer.WriteUInt32(0);
            writer.WriteInt32(Data.Length + Name.Length * 2);
            writer.WriteUInt32((uint)Flags);
            writer.WriteInt32(0);
            writer.WriteInt32(Name.Length);
            writer.WriteUnicode(Name);

            writer.WriteBytes(Data);
        }
    }
}
