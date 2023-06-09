using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Banks;

[ChunkLoader(26214, "ImageBank")]
public class ImageBank : ChunkLoader
{
    public static List<Task> imageReadingTasks = new();

    public static List<Task> imageWritingTasks = new();
    public static int realGraphicMode = 4;
    public Dictionary<int, FusionImage> Items = new();
    public static event SaveHandler OnImageLoaded;


    public static FusionImage CreateImage()
    {
        if (Settings.Android)
            return new AndroidImage();
        if (Settings.TwoFivePlus)
            return new TwoFivePlusImage();
        if (Settings.Old)
            return new MMFImage();
        return new NormalImage();
    }

    public override void Read(ByteReader reader)
    {
        // tysm LAK
        // Yuni asked my to add this back
        // This comment doesn't belong here, but I'm still keeping it
        if (CTFAKCore.Parameters.Contains("-noimg")) return;

        var count = 0;

        if (Settings.Android)
        {
            var maxHandle = reader.ReadInt16();
            count = reader.ReadInt16();
        }
        else
        {
            count = reader.ReadInt32();
        }

        for (var i = 0; i < count; i++)
        {
            var newImg = CreateImage();
            newImg.Read(reader);
            OnImageLoaded?.Invoke(i, count);
            Items.Add(newImg.Handle, newImg);
        }

        /*if (Settings.Android)
            foreach (var img in Items)
            {
                var image = img.Value;
                image.FromBitmap(ImageHelper.DumpImage(image.Handle, image.imageData, image.Width, image.Height,
                    image.GraphicMode));
            }
*/

        foreach (var task in imageReadingTasks) task.Wait();
        imageReadingTasks.Clear();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}