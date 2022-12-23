#define USE_IONIC
using System;
using System.IO;
using System.Runtime.InteropServices;
using CTFAK.Utils;
using Ionic.Zlib;
using Joveler.Compression.ZLib;
using DeflateStream = Joveler.Compression.ZLib.DeflateStream;


namespace CTFAK.Memory
{
    public static class Decompressor
    {

        public static byte[] Decompress(ByteReader exeReader, out int decompressed)
        {
            Int32 decompSize = exeReader.ReadInt32();
            Int32 compSize = exeReader.ReadInt32();
            decompressed = decompSize;
            return DecompressBlock(exeReader, compSize, decompSize);
        }

        public static ByteReader DecompressAsReader(ByteReader exeReader, out int decompressed) =>
            new ByteReader(Decompress(exeReader, out decompressed));

        public static byte[] DecompressBlock(byte[] data, int size, int decompSize)
        {
#if USE_IONIC
            return ZlibStream.UncompressBuffer(data);
#else
            ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();

            using (MemoryStream fsComp = new MemoryStream(data))
            using (MemoryStream fsDecomp = new MemoryStream())
            using (ZLibStream zs = new ZLibStream(fsComp, decompOpts))
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
            ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();

            using (MemoryStream fsComp = new MemoryStream(data))
            using (MemoryStream fsDecomp = new MemoryStream())
            using (ZLibStream zs = new ZLibStream(fsComp, decompOpts))
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
            ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();

            using (MemoryStream fsComp = new MemoryStream(reader.ReadBytes(size)))
            using (MemoryStream fsDecomp = new MemoryStream())
            using (ZLibStream zs = new ZLibStream(fsComp, decompOpts))
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
            ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();

            using (MemoryStream fsComp = new MemoryStream(reader.ReadBytes(size)))
            using (MemoryStream fsDecomp = new MemoryStream())
            using (ZLibStream zs = new ZLibStream(fsComp, decompOpts))
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
            Int32 actualSize;
            var data = DecompressOldBlock(buffer, (int)compressedSize, decompressedSize, out actualSize);
            reader.Seek(start + actualSize);
            return data;
        }

        public static byte[] DecompressOldBlock(byte[] buff, int size, int decompSize, out Int32 actual_size)
        {
            var originalBuff = Marshal.AllocHGlobal(size);
            Marshal.Copy(buff, 0, originalBuff, buff.Length);
            var outputBuff = Marshal.AllocHGlobal(decompSize);
            actual_size = NativeLib.decompressOld(originalBuff, size, outputBuff, decompSize);
            Marshal.FreeHGlobal(originalBuff);
            byte[] data = new byte[decompSize];
            Marshal.Copy(outputBuff, data, 0, decompSize);
            Marshal.FreeHGlobal(outputBuff);
            return data;
        }

        public static byte[] compress_block(byte[] data)
        {
            ZLibCompressOptions compOpts = new ZLibCompressOptions();
            //compOpts.Level = ZLibCompLevel.Default;
            compOpts.Level = ZLibCompLevel.Default;
            MemoryStream decompressedStream = new MemoryStream(data);
            MemoryStream compressedStream = new MemoryStream();
            byte[] compressedData = null;
            Joveler.Compression.ZLib.ZLibStream
                zs = new Joveler.Compression.ZLib.ZLibStream(compressedStream, compOpts);
            decompressedStream.CopyTo(zs);
            zs.Close();

            compressedData = compressedStream.ToArray();
            //Array.Resize<byte>(ref compressedData, (int)zs.TotalOut);

            return compressedData;
        }
    }
}

