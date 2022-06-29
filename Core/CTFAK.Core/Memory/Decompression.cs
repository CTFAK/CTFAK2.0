using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using CTFAK.Utils;
using Ionic.Zlib;
using Joveler.Compression.ZLib;
using ZLibStream = System.IO.Compression.ZLibStream;


namespace CTFAK.Memory
{
    public static class Decompressor
    {
        public static ZLibCompLevel compressionLevel = ZLibCompLevel.Level5;
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
            var newData = ZlibStream.UncompressBuffer(reader.ReadBytes(size));
            // Trimming array to decompSize,
            // because ZlibStream always pads to 0x100
            Array.Resize<byte>(ref newData, decompSize);
            return newData;
        }
        public static byte[] DecompressBlock(ByteReader reader, int size)
        {
           
            // We have no original size, so we are gonna just leave everything as is
            return ZlibStream.UncompressBuffer(reader.ReadBytes(size));
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
            compOpts.Level = compressionLevel;
            MemoryStream decompressedStream = new MemoryStream(data);
            MemoryStream compressedStream = new MemoryStream();
            byte[] compressedData = null;
            Joveler.Compression.ZLib.ZLibStream zs = new Joveler.Compression.ZLib.ZLibStream(compressedStream, compOpts);
            decompressedStream.CopyTo(zs);
            zs.Close();

            compressedData = compressedStream.GetBuffer();
            Array.Resize<byte>(ref compressedData, (int) zs.TotalOut);

            return compressedData;
        }
    }
}