using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

using Image = CTFAK.CCN.Chunks.Banks.Image;
using CTFAK.Utils;

namespace CTFAK.MMFParser.MFA.Loaders
{
    public class AGMIBank : ChunkLoader
    {
        private int GraphicMode;
        private int PaletteVersion;
        private int PaletteEntries;
        public Dictionary<int, Image> Items = new Dictionary<int, Image>();
        public List<Color> Palette;

        public override void Read()
        {
            GraphicMode = reader.ReadInt32();
            PaletteVersion = reader.ReadInt16();
            PaletteEntries = reader.ReadInt16();
            Palette = new List<Color>();
            for (int i = 0; i < 256; i++)
            {
                Palette.Add(reader.ReadColor());
            }

            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var item = new Image(reader);
                item.IsMFA = true;
                item.Read();
                Items.Add(item.Handle, item);
            }
        }

        private List<Task> imageWriteTasks = new List<Task>();
        private List<ByteWriter> imageWriters = new List<ByteWriter>();
        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(GraphicMode);
            writer.WriteInt16((short)PaletteVersion);
            writer.WriteInt16((short)PaletteEntries);
            for (int i = 0; i < 256; i++) writer.WriteColor(Palette[i]);
            writer.WriteInt32(Items.Count);
            foreach (var key in Items.Keys)
            {
                var newWriter = new ByteWriter(new MemoryStream());
                var writeTask = new Task(() =>
                {
                    var newOffset = Items[key].WriteNew(newWriter);
                });
                imageWriteTasks.Add(writeTask);
                imageWriters.Add(newWriter);
                writeTask.Start();
            }

            foreach (var task in imageWriteTasks)
            {
                task.Wait();
            }

            foreach (var newWriter in imageWriters)
            {
                writer.WriteWriter(newWriter);
            }


        
        }
        public AGMIBank(ByteReader reader) : base(reader) { }

    }
}