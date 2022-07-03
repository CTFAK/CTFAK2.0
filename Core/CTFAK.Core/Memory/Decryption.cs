using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CTFAK.Memory
{
    static class Decryption
    {
        public static byte[] _decryptionKey;
        public static byte MagicChar = 99;
        //public static byte MagicChar = 54;


        public static void MakeKey(string data1, string data2, string data3)
        {
            // MakeKeyUnicode(data1,data2,data3);
            // return;
            IntPtr keyPtr;
            if (data1 == null) data1 = "";
            if (data2 == null) data2 = "";
            if (data3 == null) data3 = "";

            keyPtr = Marshal.AllocHGlobal(256);
            var data1Ptr = Marshal.StringToHGlobalAnsi(data1);
            var data2Ptr = Marshal.StringToHGlobalAnsi(data2);
            var data3Ptr = Marshal.StringToHGlobalAnsi(data3);

            keyPtr = NativeLib.make_key(data1Ptr, data2Ptr, data3Ptr, MagicChar);
            byte[] key = new byte[256];
            Marshal.Copy(keyPtr, key, 0, 256);
            //Marshal.FreeHGlobal(keyPtr);
            _decryptionKey = key;
            //Logger.Log($"First 16-Bytes of key: {_decryptionKey.GetHex(16)}", true, ConsoleColor.Yellow);
            //File.WriteAllBytes($"{Settings.DumpPath}\\key.bin", _decryptionKey);
        }

        public static void MakeKeyUnicode(string data1, string data2, string data3)
        {
            IntPtr data1ptr;
            IntPtr data2ptr;
            IntPtr data3ptr;
            IntPtr keyPtr;
            data1ptr = Marshal.StringToHGlobalUni(data1);
            data2ptr = Marshal.StringToHGlobalUni(data2);
            data3ptr = Marshal.StringToHGlobalUni(data3);
            keyPtr = NativeLib.make_key_w(data1ptr, data2ptr, data3ptr, MagicChar);
            byte[] key = new byte[256];
            Marshal.Copy(keyPtr, key, 0, 256);
            _decryptionKey = key;
            Marshal.FreeHGlobal(data1ptr);
            Marshal.FreeHGlobal(data2ptr);
            Marshal.FreeHGlobal(data3ptr);
            //Logger.Log($"First 16-Bytes of key: {_decryptionKey.GetHex(16)}", true, ConsoleColor.Yellow);


        }

        public static byte[] MakeKeyFromComb(string data, byte magicChar = 54)
        {
            var rawKeyPtr = Marshal.StringToHGlobalAnsi(data);
            var ptr = NativeLib.make_key_combined(rawKeyPtr, magicChar);

            byte[] key = new byte[256];
            Marshal.Copy(ptr, key, 0, 256);
            Marshal.FreeHGlobal(rawKeyPtr);
            Array.Resize(ref key, data.Length);
            Array.Resize(ref key, 256);

            return key;
        }


        public static byte[] DecodeMode3(byte[] chunkData, int chunkSize, int chunkId, out int decompressed)
        {
            ByteReader reader = new ByteReader(chunkData);
            uint decompressedSize = reader.ReadUInt32();

            byte[] rawData = reader.ReadBytes((int)reader.Size());
            if ((chunkId & 1) == 1 && Settings.Build > 284)
            {
                rawData[0] ^= (byte)((byte)(chunkId & 0xFF) ^ (byte)(chunkId >> 0x8));
            }

            rawData = DecryptChunk(rawData, chunkSize);
            using (ByteReader data = new ByteReader(rawData))
            {
                uint compressedSize = data.ReadUInt32();
                decompressed = (int)decompressedSize;
                return Decompressor.DecompressBlock(data, (int)compressedSize, (int)decompressedSize);
            }
        }

        public static byte[] EncryptAndCompressMode3(byte[] chunkData, int chunkId)
        {

            var compressedData = Decompressor.compress_block(chunkData);
            var decryptedWriter = new ByteWriter(new MemoryStream());
            decryptedWriter.WriteInt32(compressedData.Length);

            decryptedWriter.WriteBytes(compressedData);
            var encryptedData = EncryptChunk(decryptedWriter.GetBuffer(), (int)decryptedWriter.Size());
            var anotherWriter = new ByteWriter(new MemoryStream());
            anotherWriter.WriteInt32(encryptedData.Length - 12);
            anotherWriter.WriteBytes(encryptedData);
            return anotherWriter.GetBuffer();

        }
        public static byte[] DecryptChunk(byte[] chunkData, int chunkSize)
        {
            IntPtr inputChunkPtr = Marshal.AllocHGlobal(chunkData.Length);
            Marshal.Copy(chunkData, 0, inputChunkPtr, chunkData.Length);

            IntPtr keyPtr = Marshal.AllocHGlobal(_decryptionKey.Length);
            Marshal.Copy(_decryptionKey, 0, keyPtr, _decryptionKey.Length);

            var outputChunkPtr = NativeLib.decode_chunk(inputChunkPtr, chunkSize, MagicChar, keyPtr);

            byte[] decodedChunk = new byte[chunkSize];
            Marshal.Copy(outputChunkPtr, decodedChunk, 0, chunkSize);

            Marshal.FreeHGlobal(inputChunkPtr);
            Marshal.FreeHGlobal(keyPtr);

            return decodedChunk;
        }

        public static byte[] EncryptChunk(byte[] chunkData, int chunkSize)
        {
            IntPtr inputChunkPtr = Marshal.AllocHGlobal(chunkData.Length);
            Marshal.Copy(chunkData, 0, inputChunkPtr, chunkData.Length);

            IntPtr keyPtr = Marshal.AllocHGlobal(_decryptionKey.Length);
            Marshal.Copy(_decryptionKey, 0, keyPtr, _decryptionKey.Length);

            var outputChunkPtr = NativeLib.decode_chunk(inputChunkPtr, chunkSize, MagicChar, keyPtr);

            byte[] decodedChunk = new byte[chunkSize];
            Marshal.Copy(outputChunkPtr, decodedChunk, 0, chunkSize);

            Marshal.FreeHGlobal(inputChunkPtr);
            Marshal.FreeHGlobal(keyPtr);

            return decodedChunk;

        }




    }
}