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
    public class Extensions : ChunkLoader
    {
        internal ushort PreloadExtensions;
        public List<Extension> Items;




        public override void Read(ByteReader reader)
        {
 
            var count = reader.ReadUInt16();
            PreloadExtensions = reader.ReadUInt16();
            Items = new List<Extension>();
            for (int i = 0; i < count; i++)
            {
                var ext = new Extension();
                ext.Read(reader);
                Items.Add(ext);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short)Items.Count);
            Writer.WriteInt16((short)PreloadExtensions);
            foreach (Extension item in Items)
            {

                item.Write(Writer);
            }

        }


    }
    public class Extension : ChunkLoader
    {
        public short Handle;
        public int MagicNumber;
        public int VersionLs;
        public int VersionMs;
        public string Name = "";
        public string Ext;
        public string SubType = "";


        public override void Read(ByteReader reader)
        {
            var currentPosition = reader.Tell();
            var size = reader.ReadInt16();
            if (size < 0) size = (short)(size * -1);
            Handle = reader.ReadInt16();
            MagicNumber = reader.ReadInt32();
            VersionLs = reader.ReadInt32();
            VersionMs = reader.ReadInt32();
            var extName = reader.ReadYuniversal();
            if(extName.Length==0)
            {

                reader.Seek(currentPosition + size);
                return;
            }
            string[] arr;
            arr = extName.Split('.');
            if (!extName.Contains(".")) return;
            Name = arr[0];
            Ext = arr[1];
            SubType = reader.ReadYuniversal();
            reader.Seek(currentPosition + size);

            var newString = string.Empty;
            newString += $"MagicNumber={MagicNumber}\n";
            newString += $"VersionLs={VersionLs}\n";
            newString += $"VersionMs={VersionMs}\n";
            newString += $"SubType={SubType}\n";
        }

        public override void Write(ByteWriter Writer)
        {
            var newWriter = new ByteWriter(new MemoryStream());
            newWriter.WriteInt16(Handle);
            newWriter.WriteInt32(MagicNumber);
            newWriter.WriteInt32(VersionLs);
            newWriter.WriteInt32(VersionMs);
            newWriter.WriteUnicode(string.Join(".", new string[] { Name, Ext }));
            newWriter.WriteUnicode(SubType);
            Writer.WriteInt16((short)(newWriter.Size() + 2));
            Writer.WriteWriter(newWriter);
        }


    }
}
