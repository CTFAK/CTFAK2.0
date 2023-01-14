#if RELEASE
#define USE_IONIC
#endif
using System.IO;
using System.Runtime.InteropServices;
using CTFAK.Utils;
using Joveler.Compression.ZLib;

namespace CTFAK.Memory;

public static class Decompressor
{
    public static byte[] Decompress(ByteReader exeReader, out int decompressed)
    {
        var decompSize = exeReader.ReadInt32();
        var compSize = exeReader.ReadInt32();
        decompressed = decompSize;
        return DecompressBlock(exeReader, compSize, decompSize);
    }

    public static ByteReader DecompressAsReader(ByteReader exeReader, out int decompressed)
    {
        return new ByteReader(Decompress(exeReader, out decompressed));
    }

    public static byte[] DecompressBlock(byte[] data, int size, int decompSize)
    {
#if USE_IONIC
            return ZlibStream.UncompressBuffer(data);
#else
        var decompOpts = new ZLibDecompressOptions();

        using (var fsComp = new MemoryStream(data))
        using (var fsDecomp = new MemoryStream())
        using (var zs = new ZLibStream(fsComp, decompOpts))
        {
            zs.CopyTo(fsDecomp);
            var newData = fsDecomp.ToArray();
            return newData;
        }
#endif
    }

    public static byte[] DecompressBlock(byte[] data, int size)
    {
#if USE_IONIC
            return ZlibStream.UncompressBuffer(data);
#else
        var decompOpts = new ZLibDecompressOptions();

        using (var fsComp = new MemoryStream(data))
        using (var fsDecomp = new MemoryStream())
        using (var zs = new ZLibStream(fsComp, decompOpts))
        {
            zs.CopyTo(fsDecomp);
            var newData = fsDecomp.ToArray();
            return newData;
        }
#endif
    }

    public static byte[] DecompressBlock(ByteReader reader, int size, int decompSize)
    {
#if USE_IONIC
            return ZlibStream.UncompressBuffer(reader.ReadBytes(size));
#else
        var decompOpts = new ZLibDecompressOptions();

        using (var fsComp = new MemoryStream(reader.ReadBytes(size)))
        using (var fsDecomp = new MemoryStream())
        using (var zs = new ZLibStream(fsComp, decompOpts))
        {
            zs.CopyTo(fsDecomp);
            var newData = fsDecomp.ToArray();
            return newData;
        }
#endif
    }

    public static byte[] DecompressBlock(ByteReader reader, int size)
    {
#if USE_IONIC
            return ZlibStream.UncompressBuffer(reader.ReadBytes(size));
#else
        var decompOpts = new ZLibDecompressOptions();

        using (var fsComp = new MemoryStream(reader.ReadBytes(size)))
        using (var fsDecomp = new MemoryStream())
        using (var zs = new ZLibStream(fsComp, decompOpts))
        {
            zs.CopyTo(fsDecomp);
            var newData = fsDecomp.ToArray();
            return newData;
        }
#endif
    }

    public static byte[] DecompressOld(ByteReader reader)
    {
        var decompressedSize = reader.PeekInt32() != -1 ? reader.ReadInt32() : 0;
        var start = reader.Tell();
        var compressedSize = reader.Size();
        var buffer = reader.ReadBytes((int)compressedSize);
        int actualSize;
        var data = DecompressOldBlock(buffer, (int)compressedSize, decompressedSize, out actualSize);
        reader.Seek(start + actualSize);
        return data;
    }

    public static byte[] DecompressOldBlock(byte[] buff, int size, int decompSize, out int actual_size)
    {
        var originalBuff = Marshal.AllocHGlobal(size);
        Marshal.Copy(buff, 0, originalBuff, buff.Length);
        var outputBuff = Marshal.AllocHGlobal(decompSize);
        actual_size = NativeLib.decompressOld(originalBuff, size, outputBuff, decompSize);
        Marshal.FreeHGlobal(originalBuff);
        var data = new byte[decompSize];
        Marshal.Copy(outputBuff, data, 0, decompSize);
        Marshal.FreeHGlobal(outputBuff);
        return data;
    }

    public static byte[] compress_block(byte[] data)
    {
        var compOpts = new ZLibCompressOptions();
        //compOpts.Level = ZLibCompLevel.Default;
        compOpts.Level = ZLibCompLevel.Default;
        var decompressedStream = new MemoryStream(data);
        var compressedStream = new MemoryStream();
        byte[] compressedData = null;
        var
            zs = new ZLibStream(compressedStream, compOpts);
        decompressedStream.CopyTo(zs);
        zs.Close();

        compressedData = compressedStream.ToArray();
        //Array.Resize<byte>(ref compressedData, (int)zs.TotalOut);

        return compressedData;
    }
}