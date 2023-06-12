//#define USE_IONIC

using System.IO;
using System.Runtime.CompilerServices;
using Ionic.Zlib;
using Joveler.Compression.ZLib;
//using Joveler.Compression.ZLib;

namespace CTFAK.Memory;

public static class Decompressor
{
    public static ByteWriter Compress(byte[] buffer)
    {
        var writer = new ByteWriter(new MemoryStream());
        var compressed = CompressBlock(buffer);
        writer.WriteInt32(buffer.Length);
        writer.WriteInt32(compressed.Length);
        writer.WriteBytes(compressed);
        return writer;
    }

    public static byte[] Decompress(ByteReader exeReader, out int decompressed)
    {
        var decompSize = exeReader.ReadInt32();
        var compSize = exeReader.ReadInt32();
        decompressed = decompSize;
        return DecompressBlock(exeReader, compSize);
    }

    public static ByteReader DecompressAsReader(ByteReader exeReader, out int decompressed)
    {
        return new ByteReader(Decompress(exeReader, out decompressed));
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static byte[] DecompressBlock(byte[] data)
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

    public static unsafe byte[] DecompressOldBlock(byte[] buff, int size, int decompSize, out int actualSize)
    {
        // this doesn't work. i gotta rewrite the entire Tinflate.cs file
        Tinflate.tinf_init();
        var outputBuffer = new byte[200000];

        fixed (byte* input = buff)
        {
            fixed (byte* output = outputBuffer)
            {
                var outputSize = (uint)decompSize;
                var result = Tinflate.tinf_uncompress(output, &outputSize, input, (uint)size);
                if (result > 0)
                    actualSize = result;
                else actualSize = (int)outputSize;
            }
        }


        return outputBuffer;
    }

    public static byte[] CompressBlock(byte[] data)
    {
        var compOpts = new ZLibCompressOptions();
        compOpts.Level = ZLibCompLevel.Default;
        var decompressedStream = new MemoryStream(data);
        var compressedStream = new MemoryStream();
        var zs = new ZLibStream(compressedStream, compOpts);
        decompressedStream.CopyTo(zs);
        decompressedStream.Close();
        decompressedStream.Dispose();
        zs.Close();
        
        return compressedStream.ToArray();
    }
}