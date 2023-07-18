using CTFAK.CCN.Chunks;
using CTFAK.Core.CCN.Chunks.Banks.ImageBank;
using CTFAK.Memory;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CTFAK.CTFAKCore;

namespace CTFAK.MMFParser.MFA.Loaders
{
    public class AGMIBank : ChunkLoader
    {
        private readonly List<ByteWriter> _imageWriters = new();

        private readonly List<Task> _imageWriteTasks = new();
        private int _graphicMode;
        public Dictionary<int, FusionImage> Items = new();
        public List<Color> Palette = new Color[256].ToList();
        private int _paletteEntries;
        private int _paletteVersion;
        public event SaveHandler OnImageLoaded;

        public override void Read(ByteReader reader)
        {
            _graphicMode = reader.ReadInt32();
            _paletteVersion = reader.ReadInt16();
            _paletteEntries = reader.ReadInt16();
            Palette = new List<Color>();
            for (var i = 0; i < _paletteEntries; i++) Palette.Add(reader.ReadColor());

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var item = new FusionImage();
                item.IsMFA = true;
                item.Read(reader);
                OnImageLoaded?.Invoke(i, count);
                if (!Items.ContainsKey(item.Handle))
                    Items.Add(item.Handle, item);
            }

            foreach (var task in ImageBank.imageReadingTasks) task.Wait();
            ImageBank.imageReadingTasks.Clear();
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(_graphicMode);
            writer.WriteInt16((short)_paletteVersion);
            writer.WriteInt16((short)_paletteEntries);
            for (var i = 0; i < 256; i++) writer.WriteColor(Palette[i]);

            writer.WriteInt32(Items.Count);
            foreach (var key in Items.Keys)
            {
                var newWriter = new ByteWriter(new MemoryStream());
                var writeTask = new Task(() =>
                {
                    var newOffset = Items[key].WriteNew(newWriter);
                });
                _imageWriteTasks.Add(writeTask);
                _imageWriters.Add(newWriter);
                writeTask.Start();
            }

            foreach (var task in _imageWriteTasks) task.Wait();

            foreach (var newWriter in _imageWriters) writer.WriteWriter(newWriter);
        }
    }
}