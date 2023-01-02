using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using Image = CTFAK.CCN.Chunks.Banks.Image;

namespace CTFAK.MMFParser.MFA.Loaders;

public class AGMIBank : ChunkLoader
{
    private int GraphicMode;
    private readonly List<ByteWriter> imageWriters = new();

    private readonly List<Task> imageWriteTasks = new();
    public Dictionary<int, Image> Items = new();
    public List<Color> Palette = new Color[256].ToList();
    private int PaletteEntries;
    private int PaletteVersion;
    public event SaveHandler OnImageLoaded;

    public override void Read(ByteReader reader)
    {
        GraphicMode = reader.ReadInt32();
        PaletteVersion = reader.ReadInt16();
        PaletteEntries = reader.ReadInt16();
        Palette = new List<Color>();
        for (var i = 0; i < PaletteEntries; i++) Palette.Add(reader.ReadColor());

        var count = reader.ReadInt32();

        for (var i = 0; i < count; i++)
        {
            var item = new Image();
            item.IsMFA = true;
            item.Read(reader);
            OnImageLoaded?.Invoke(i, count);
            if (!Items.ContainsKey(item.Handle))
                Items.Add(item.Handle, item);
        }

        foreach (var task in Image.imageReadingTasks) task.Wait();
        Image.imageReadingTasks.Clear();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(GraphicMode);
        writer.WriteInt16((short)PaletteVersion);
        writer.WriteInt16((short)PaletteEntries);
        for (var i = 0; i < 256; i++) writer.WriteColor(Palette[i]);

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

        foreach (var task in imageWriteTasks) task.Wait();

        foreach (var newWriter in imageWriters) writer.WriteWriter(newWriter);
    }
}