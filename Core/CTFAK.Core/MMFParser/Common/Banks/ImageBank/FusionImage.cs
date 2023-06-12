using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MMFUtils;
using CTFAK.Utils;
using K4os.Compression.LZ4;

namespace CTFAK.MMFParser.Common.Banks;

// ===GRAPHIC MODES===

// 0 - android, transparency, 4 bytes per pixel, 8 bits per channel
// 3 - android, no transparency, 3 bytes per pixel, 8 bits per channel

// 2 - android, transparency, 2 bytes per pixel, 5 bits per channel
// 1 - android, transparency, 2 bytes per pixel, 4 bits per channel
// 4 - android, no transparency, 2 bytes per pixel, 5 bits per channel

// 5 - android, no transparency, JPEG

// 4 - normal, 24 bits per color, 8 bit-deep alpha mask at the end
// 4 - mmf1.5, i don't like that
// 6 - normal, 15 bits per pixel, but it's actually 16 but retarded
// 7 - normal, 16 bits per pixel
// 8 - 2.5+, 32 bits per pixel, 8 bits per color

public class FusionImage : ChunkLoader
{
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
        "Mac",
        "RGBA"
    });

    public byte GraphicMode;
    public int Handle;
    public int Height;
    public short HotspotX;
    public short HotspotY;

    public byte[] imageData;
    public bool IsMFA;
    public byte[] newImageData;

    public int onepointfiveDecompressedSize;
    public int onepointfiveStart;
    public Bitmap realBitmap;
    public int references;
    public Color Transparent;

    public int Width;

    public Bitmap bitmap
    {
        get
        {
            if (realBitmap == null)
            {
                realBitmap = new Bitmap(Width, Height);
                var bmpData = realBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                byte[] colorArray = null;
                switch (GraphicMode)
                {
                    case 0:
                        colorArray = ImageTranslator.AndroidMode0ToRGBA(imageData, Width, Height, false);
                        break;
                    case 1:
                        colorArray = ImageTranslator.AndroidMode1ToRGBA(imageData, Width, Height, false);
                        break;
                    case 2:
                        colorArray = ImageTranslator.AndroidMode2ToRGBA(imageData, Width, Height, false);
                        break;
                    case 3:
                        colorArray = ImageTranslator.AndroidMode3ToRGBA(imageData, Width, Height, false);
                        break;
                    case 4:
                        if (Settings.Android) //comment this out of if android breaks
                            colorArray =
                                ImageTranslator.AndroidMode4ToRGBA(imageData, Width, Height,
                                    false); //comment this out of if android breaks
                        else //comment this out of if android breaks
                            colorArray = ImageTranslator.Normal24BitMaskedToRGBA(imageData, Width, Height,
                                Flags["Alpha"], Transparent, Settings.F3);
                        break;
                    case 5:
                        colorArray = ImageTranslator.AndroidMode5ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                        break;
                    case 6:
                        colorArray =
                            ImageTranslator.Normal15BitToRGBA(imageData, Width, Height, Flags["Alpha"], Transparent);
                        break;
                    case 7:
                        colorArray =
                            ImageTranslator.Normal16BitToRGBA(imageData, Width, Height, Flags["Alpha"], Transparent);
                        break;
                    case 8:
                        colorArray = ImageTranslator.TwoFivePlusToRGBA(imageData, Width, Height, Flags["Alpha"],
                            Transparent, Flags["RGBA"], Settings.F3);
                        break;
                }

                //Logger.Log($"Loading image {Handle} with mode {GraphicMode}");
                if (colorArray == null) Logger.LogWarning("colorArray is null for image mode " + GraphicMode);


                Marshal.Copy(colorArray, 0, bmpData.Scan0, colorArray.Length);

                realBitmap.UnlockBits(bmpData);
            }

            return realBitmap;
        }
    }


    public void FromBitmap(Bitmap bmp)
    {
        Width = bmp.Width;
        Height = bmp.Height;
        if (!CTFAKCore.Parameters.Contains("-noalpha"))
            Flags["Alpha"] = true;
        GraphicMode = 4;

        var bitmapData = bmp.LockBits(new Rectangle(0, 0,
                bmp.Width,
                bmp.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
        var copyPad = ImageHelper.GetPadding(Width, 4);
        var length = bitmapData.Height * bitmapData.Stride + copyPad * 4;

        var bytes = new byte[length];
        var stride = bitmapData.Stride;
        // Copy bitmap to byte[]
        Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
        bmp.UnlockBits(bitmapData);

        imageData = new byte[Width * Height * 6];
        var position = 0;
        var pad = ImageHelper.GetPadding(Width, 3);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var newPos = y * stride + x * 3;
                imageData[position] = bytes[newPos];
                imageData[position + 1] = bytes[newPos + 1];
                imageData[position + 2] = bytes[newPos + 2];
                position += 3;
            }

            position += 3 * pad;
        }

        try
        {
            var bitmapDataAlpha = bmp.LockBits(new Rectangle(0, 0,
                    bmp.Width,
                    bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            var copyPadAlpha = ImageHelper.GetPadding(Width, 1);
            var lengthAlpha = bitmapDataAlpha.Height * bitmapDataAlpha.Stride + copyPadAlpha * 4;

            var bytesAlpha = new byte[lengthAlpha];
            var strideAlpha = bitmapDataAlpha.Stride;
            // Copy bitmap to byte[]
            Marshal.Copy(bitmapDataAlpha.Scan0, bytesAlpha, 0, lengthAlpha);
            bmp.UnlockBits(bitmapDataAlpha);

            var aPad = ImageHelper.GetPadding(Width, 1, 4);
            var alphaPos = position;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    imageData[alphaPos] = bytesAlpha[y * strideAlpha + x * 4 + 3];
                    alphaPos += 1;
                }

                alphaPos += aPad;
            }
        }
        catch
        {
        } /*(Exception ex){Console.WriteLine(ex);}*/
    }

    public override void Read(ByteReader reader)
    {
        // none of this is actually used for reading exes, ignore. it's still used for MFAs though, because I was too lazy to rewrite the MFA part
        var start = reader.Tell();
        var dataSize = 0;
        if (Settings.Android)
        {
            Handle = reader.ReadInt16();

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
            GraphicMode = (byte)reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            HotspotX = reader.ReadInt16();
            HotspotY = reader.ReadInt16();
            ActionX = reader.ReadInt16();
            ActionY = reader.ReadInt16();
            dataSize = reader.ReadInt32();

            if (reader.PeekByte() == 255)
                imageData = reader.ReadBytes(dataSize);
            else
                imageData = Decompressor.DecompressBlock(reader, dataSize);

            return;

            // couldn't care less
        }

        Handle = reader.ReadInt32();
        if (Settings.Build >= 284 && !IsMFA)
            Handle--;

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
                var decompressedSize = reader.ReadInt32();
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
                    decompressedReader = new ByteReader(Decompressor.DecompressOldBlock(newImageData,
                        newImageData.Length, onepointfiveDecompressedSize, out var actualSize));
                    reader.Seek(onepointfiveStart + actualSize);
                }

                else
                {
                    decompressedReader =
                        new ByteReader(Decompressor.DecompressBlock(newImageData));
                }

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
            Flags.Flag = decompressedReader.ReadByte();
            if (!Settings.Old)
                decompressedReader.ReadInt16();
            HotspotX = decompressedReader.ReadInt16();
            HotspotY = decompressedReader.ReadInt16();
            ActionX = decompressedReader.ReadInt16();
            ActionY = decompressedReader.ReadInt16();
            if (!Settings.Old)
                Transparent = decompressedReader.ReadColor();
            else
                Transparent = Color.Black; //ig?


            if (Settings.Android)
            {
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
                        (int)(decompressedReader.Size() - decompressedReader.Tell()));
                }
                else
                {
                    imageData = decompressedReader.ReadBytes(dataSize);
                }
            }

            newImageData = null;
        });
        ImageBank.imageReadingTasks.Add(mainRead);
        if (!IsMFA && !Settings.Old && !Settings.TwoFivePlus)
            mainRead.Start();
        else mainRead.RunSynchronously();
    }

    public int WriteNew(ByteWriter writer)
    {
        //PrepareForMfa();
        var start = writer.Tell();

        byte[] compressedImg = null;
        Flags["LZX"] = true;

        compressedImg = Decompressor.CompressBlock(imageData);
        writer.WriteInt32(Handle);
        writer.WriteInt32(Checksum); //4
        writer.WriteInt32(references); //8
        writer.WriteUInt32((uint)compressedImg.Length + 4); //12
        writer.WriteInt16((short)Width); //14
        writer.WriteInt16((short)Height); //16
        writer.WriteInt8(GraphicMode); //17
        writer.WriteInt8((byte)Flags.Flag); //18
        writer.WriteInt16(0); //20
        writer.WriteInt16(HotspotX); //22
        writer.WriteInt16(HotspotY); //24
        writer.WriteInt16(ActionX); //26
        writer.WriteInt16(ActionY); //28
        writer.WriteColor(Transparent); //32
        writer.WriteInt32(imageData.Length); //36
        writer.WriteBytes(compressedImg);
        //writer.WriteWriter(chunk);
        // writer.WriteInt32(Handle-1);//FNAC3 FIX

        var chunkSize = 36 + compressedImg.Length;

        return (int)(chunkSize + 4 + start);
    }

    public override void Write(ByteWriter writer)
    {
    }

    public void PrepareForMfa()
    {
        switch (GraphicMode)
        {
            case 0:
                imageData = ImageTranslator.AndroidMode0ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
            case 1:
                imageData = ImageTranslator.AndroidMode1ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
            case 2:
                imageData = ImageTranslator.AndroidMode2ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
            case 3:
                imageData = ImageTranslator.AndroidMode3ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
            case 4:
                if (Settings.Android)
                {
                    imageData = ImageTranslator.AndroidMode4ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                    imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                    GraphicMode = 4;
                }
                else if (Settings.F3)
                {
                    imageData = ImageTranslator.Normal24BitMaskedToRGBA(imageData, Width, Height, Flags["Alpha"],
                        Transparent, true);
                    imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                    GraphicMode = 4;
                }

                break;
            case 5:
                imageData = ImageTranslator.AndroidMode5ToRGBA(imageData, Width, Height, Flags["Alpha"]);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
            case 8:
                imageData = ImageTranslator.TwoFivePlusToRGBA(imageData, Width, Height, Flags["Alpha"], Transparent,
                    Flags["RGBA"], Settings.F3);
                imageData = ImageTranslator.RGBAToRGBMasked(imageData, Width, Height, Flags["Alpha"]);
                GraphicMode = 4;
                break;
        }
    }
}