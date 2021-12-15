using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Joveler.Compression.ZLib;
using DeflateStream = System.IO.Compression.DeflateStream;
using GZipStream = Joveler.Compression.ZLib.GZipStream;

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


        public static byte[] DecompressBlock(ByteReader reader, int size, int decompSize)
        {
            ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();
            MemoryStream compressedStream = new MemoryStream(reader.ReadBytes(size));
            MemoryStream decompressedStream = new MemoryStream();
            using (ZLibStream zs = new ZLibStream(compressedStream, decompOpts)) zs.CopyTo(decompressedStream);

            byte[] decompressedData = decompressedStream.GetBuffer();
            compressedStream.Dispose();
            decompressedStream.Dispose();
            // Trimming array to decompSize,
            // because ZlibStream always pads to 0x100
            Array.Resize<byte>(ref decompressedData, decompSize);
            return decompressedData;
        }
        public static byte[] DecompressBlock(ByteReader reader, int size)
        {
            /*ZLibDecompressOptions decompOpts = new ZLibDecompressOptions();
            MemoryStream compressedStream = new MemoryStream(reader.ReadBytes(size));
            MemoryStream decompressedStream = new MemoryStream();
            using (ZLibStream zs = new ZLibStream(compressedStream, decompOpts)) zs.CopyTo(decompressedStream);
            byte[] decompressedData = decompressedStream.GetBuffer();
            compressedStream.Dispose();
            decompressedStream.Dispose();
            // We have no original size, so we are gonna just leave everything as is
            return decompressedData;*/
            return Ionic.Zlib.ZlibStream.UncompressBuffer(reader.ReadBytes(size));
        }








        public static byte[] compress_block(byte[] data)
        {
            ZLibCompressOptions compOpts = new ZLibCompressOptions();
            //compOpts.Level = ZLibCompLevel.Default;
            compOpts.Level = ZLibCompLevel.BestCompression;
            MemoryStream decompressedStream = new MemoryStream(data);
            MemoryStream compressedStream = new MemoryStream();
            byte[] compressedData = null;
            ZLibStream zs = new ZLibStream(compressedStream, compOpts);
            decompressedStream.CopyTo(zs);
            zs.Close();

            compressedData = compressedStream.GetBuffer();
            Array.Resize<byte>(ref compressedData, (int)zs.TotalOut);

            return compressedData;
        }
    }
}