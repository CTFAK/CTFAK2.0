using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CTFAK.Utils;

namespace CTFAK.Memory;

internal static class Decryption
{
    public static byte[] _decryptionKey;

    //public static byte MagicChar = 99;
    public static byte MagicChar = 54;

    
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
        var key = new byte[256];
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
        var key = new byte[256];
        Marshal.Copy(keyPtr, key, 0, 256);
        _decryptionKey = key;
        Marshal.FreeHGlobal(data1ptr);
        Marshal.FreeHGlobal(data2ptr);
        Marshal.FreeHGlobal(data3ptr);
        //Logger.Log($"First 16-Bytes of key: {_decryptionKey.GetHex(16)}", true, ConsoleColor.Yellow);
    }


    public static byte[] DecodeMode3(byte[] chunkData, int chunkSize, int chunkId, out int decompressed)
    {
        var reader = new ByteReader(chunkData);
        var decompressedSize = reader.ReadUInt32();

        var rawData = reader.ReadBytes((int)reader.Size());

        if ((chunkId & 1) == 1 && Settings.Build > 284)
            rawData[0] ^= (byte)((byte)(chunkId & 0xFF) ^ (byte)(chunkId >> 0x8));
        rawData = TransformChunk(rawData, chunkSize);

        using (var data = new ByteReader(rawData))
        {
            var compressedSize = data.ReadUInt32();
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
        var encryptedData = TransformChunk(decryptedWriter.GetBuffer(), (int)decryptedWriter.Size());
        var anotherWriter = new ByteWriter(new MemoryStream());
        anotherWriter.WriteInt32(encryptedData.Length - 12);
        anotherWriter.WriteBytes(encryptedData);
        return anotherWriter.GetBuffer();
    }

    public static unsafe byte[] TransformChunk(byte[] chunkData, int chunkSize)
    {
        fixed (byte* inputChunkPtr = chunkData)
        {
            fixed (byte* keyPtr = _decryptionKey)
            {
                var outputChunkPtr =
                    NativeLib.decode_chunk(new IntPtr(inputChunkPtr), chunkSize, MagicChar, new IntPtr(keyPtr));
                var decodedChunk = new byte[chunkSize];
                Marshal.Copy(outputChunkPtr, decodedChunk, 0, chunkSize);
                Marshal.FreeHGlobal(outputChunkPtr);
                return decodedChunk;
            }
        }
    }
}