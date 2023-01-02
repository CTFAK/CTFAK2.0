using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;
using K4os.Compression.LZ4;

namespace CTFAK.CCN.Chunks.Banks;

[ChunkLoader(26214, "ImageBank")]
public class ImageBank : ChunkLoader
{
    public static int realGraphicMode = 4;
    public Dictionary<int, Image> Items = new();
    public static event SaveHandler OnImageLoaded;

    public override void Read(ByteReader reader)
    {
        if (Core.parameters.Contains("-noimg")) return;

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
            var newImg = new Image();
            newImg.Read(reader);
            OnImageLoaded?.Invoke(i, count);
            Items.Add(newImg.Handle, newImg);
        }

        foreach (var task in Image.imageReadingTasks) task.Wait();
        Image.imageReadingTasks.Clear();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Image : ChunkLoader
{
    public static List<Task> imageReadingTasks = new();

    public static List<Task> imageWritingTasks = new();
    public short ActionX;
    public short ActionY;
    public int Checksum;

    public BitDict Flags = new(new[]
    {
        "RLE",
        "RLEW",
        "RLET",
        "LZX",
        "Alpha",
        "ACE",
        "Mac"
    });

    public byte GraphicMode;

    public int Handle;
    public int Height;
    public short HotspotX;
    public short HotspotY;
    public byte[] imageData;


    public bool IsMFA;
    public byte[] newImageData;
    public Bitmap realBitmap;
    public int references;
    public int Transparent;
    public int Width;

#pragma warning disable CA1416
    public unsafe Bitmap bitmap
    {
        get
        {
            if (realBitmap == null)
            {
                realBitmap = new Bitmap(Width, Height);
                var bmpData = realBitmap.LockBits(new Rectangle(0, 0, Width, Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


                var internalColorMode = -1;
                /*
                 * Internal color mode values
                 * 0 - 24+8 color mode (16-mil no 2.5+)
                 * TODO: implement other types 
                 */
                switch (GraphicMode)
                {
                    case 4:
                        internalColorMode = 0;
                        break;
                }

                fixed (byte* dataPtr = imageData)
                {
                    NativeLib.TranslateToRGBA(bmpData.Scan0, Width, Height, Flags["Alpha"] ? 1 : 0, imageData.Length,
                        new IntPtr(dataPtr), Transparent, internalColorMode);
                }

                realBitmap.UnlockBits(bmpData);
            }

            return realBitmap;
        }
    }
#pragma warning restore CA1416

    public void FromBitmap(Bitmap bmp)
    {
        Width = bmp.Width;
        Height = bmp.Height;
        imageData = new byte[Width * Height * 3];
        Flags["Alpha"] = false;
    }

    public int onepointfiveDecompressedSize;
    public int onepointfiveStart;
    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var dataSize = 0;
        if (Settings.Android)
        {
            /*Handle = reader.ReadInt16();

            switch (Handle >> 16)
            {
                case 0:
                    GraphicMode = 0;
                    break;
                case 3:
                    GraphicMode = 2;
                    break;
                case 5:
                    GraphicMode = 7;
                    break;
            }

            if (Settings.Build >= 284 && !IsMFA)
                Handle--;
            reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            HotspotX = reader.ReadInt16();
            HotspotY = reader.ReadInt16();
            ActionX = reader.ReadInt16();
            ActionY = reader.ReadInt16();
            DataSize = reader.ReadInt32();*/

            // couldn't care less
        }
        else
        {
            Handle = reader.ReadInt32();
            //if (Settings.Build >= 284 && !IsMFA)
             //   Handle--;

            if (!IsMFA)
            {
                if (Settings.Old)
                {
                    onepointfiveDecompressedSize = reader.ReadInt32();
                    onepointfiveStart = (int)reader.Tell();
                    newImageData = reader.ReadBytes(); 
                }
                else
                {
                    onepointfiveDecompressedSize = reader.ReadInt32();
                    var compSize = reader.ReadInt32();
                    newImageData = reader.ReadBytes(compSize);
                }
                
            }

            var mainRead = new Task(() =>
            {
                ByteReader decompressedReader;
                if (!IsMFA)
                {
                    if (Settings.Old)
                    {
                        decompressedReader = new ByteReader(Decompressor.DecompressOldBlock(newImageData, newImageData.Length,onepointfiveDecompressedSize,out var actualSize));
                        reader.Seek(onepointfiveStart+actualSize);
                    }
                       
                    else                     
                        decompressedReader = new ByteReader(Decompressor.DecompressBlock(newImageData, newImageData.Length));

                    newImageData = null;
                }
                else
                {
                    decompressedReader = reader;
                }


                if (Settings.Old)
                    Checksum = decompressedReader.ReadInt16();
                else
                    Checksum = decompressedReader.ReadInt32();
                references = decompressedReader.ReadInt32();
                if (Settings.TwoFivePlus)
                    decompressedReader.Skip(4);
                dataSize = decompressedReader.ReadInt32();
                if (IsMFA)
                    decompressedReader = new ByteReader(decompressedReader.ReadBytes(dataSize + 20));
                Width = decompressedReader.ReadInt16();
                Height = decompressedReader.ReadInt16();
                GraphicMode = decompressedReader.ReadByte();
                Flags.flag = decompressedReader.ReadByte();
                if (!Settings.Old)
                    decompressedReader.ReadInt16();
                HotspotX = decompressedReader.ReadInt16();
                HotspotY = decompressedReader.ReadInt16();
                ActionX = decompressedReader.ReadInt16();
                ActionY = decompressedReader.ReadInt16();
                if (!Settings.Old)
                    Transparent = decompressedReader.ReadInt32();
                else
                    Transparent = 0; //ig?


                if (Settings.Android)
                {
                    /*
                    if (reader.PeekByte() == 255)
                        imageData = reader.ReadBytes(datas);
                    else
                        imageData = Decompressor.DecompressBlock(reader, DataSize);
                        */
                    //couldn't care less
                }
                else
                {
                    if (Settings.TwoFivePlus)
                    {
                        var decompSizePlus = decompressedReader.ReadInt32();
                        var rawImg = decompressedReader.ReadBytes(dataSize - 4);
                        var target = new byte[decompSizePlus];
                        LZ4Codec.Decode(rawImg, target);
                        imageData = target;
                    }
                    else if (Flags["LZX"])
                    {
                        var decompSize = decompressedReader.ReadInt32();
                        imageData = Decompressor.DecompressBlock(decompressedReader,
                            (int)(decompressedReader.Size() - decompressedReader.Tell()),
                            decompSize);
                    }
                    else
                    {
                        imageData = decompressedReader.ReadBytes(dataSize);
                    }
                }

                newImageData = null;
            });
            imageReadingTasks.Add(mainRead);
            if (!IsMFA&&!Settings.Old)
                mainRead.Start();
            else mainRead.RunSynchronously();
        }
    }

    public int WriteNew(ByteWriter writer)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var start = writer.Tell();

        byte[] compressedImg = null;
        Flags["LZX"] = true;

        compressedImg = Decompressor.compress_block(imageData);

        writer.WriteInt32(Handle);
        writer.WriteInt32(Checksum); //4
        writer.WriteInt32(references); //8
        writer.WriteUInt32((uint)compressedImg.Length + 4); //12
        writer.WriteInt16((short)Width); //14
        writer.WriteInt16((short)Height); //16
        writer.WriteInt8(GraphicMode); //17
        writer.WriteInt8((byte)Flags.flag); //18
        writer.WriteInt16(0); //20
        writer.WriteInt16(HotspotX); //22
        writer.WriteInt16(HotspotY); //24
        writer.WriteInt16(ActionX); //26
        writer.WriteInt16(ActionY); //28
        writer.WriteInt32(Transparent); //32
        writer.WriteInt32(imageData.Length); //36
        writer.WriteBytes(compressedImg);
        //writer.WriteWriter(chunk);
        // writer.WriteInt32(Handle-1);//FNAC3 FIX

        var chunkSize = 36 + compressedImg.Length;
        stopwatch.Stop();
        //Console.WriteLine($"Image: {Handle}, decompressed/compressed ratio: {((float)compressedImg.Length)/imageData.Length}, time: {stopwatch.ElapsedMilliseconds}, level:{Decompressor.compressionLevel}");
        //Console.ReadKey();
        return (int)(chunkSize + 4 + start);
    }

    public override void Write(ByteWriter writer)
    {
    }
}