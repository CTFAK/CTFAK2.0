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

    public static byte[] MakeKeyCombined(string data)
    {
        byte[] dataBytes = Encoding.ASCII.GetBytes(data);
        int dataLen = dataBytes.Length;
        Array.Resize(ref dataBytes, 256);
        
        byte lastKeyByte = MagicChar;
        byte v34 = MagicChar;

        for (int i = 0; i <= dataLen; i++)
        {
            v34 = (byte)((v34 << 7) + (v34 >> 1));
            dataBytes[i] ^= v34;
            lastKeyByte += (byte)(dataBytes[i] * ((v34 & 1) + 2));
        }
        
        dataBytes[dataLen + 1] = lastKeyByte;
        return dataBytes;
    }

    public static void MakeKey(string data1, string data2, string data3)
    {

        _decryptionKey = MakeKeyCombined(data1 + data2 + data3);
    }

    public static void MakeKeyNative(string data1, string data2, string data3)
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


    public static byte[] DecodeMode3(byte[] chunkData, int chunkId, out int decompressed)
    {
        var reader = new ByteReader(chunkData);
        var decompressedSize = reader.ReadUInt32();

        var rawData = reader.ReadBytes((int)reader.Size());

        if ((chunkId & 1) == 1 && Settings.Build > 284)
            rawData[0] ^= (byte)((byte)(chunkId & 0xFF) ^ (byte)(chunkId >> 0x8));
        rawData = TransformChunk(rawData);

        using (var data = new ByteReader(rawData))
        {
            var compressedSize = data.ReadUInt32();
            decompressed = (int)decompressedSize;
            return Decompressor.DecompressBlock(data, (int)compressedSize);
        }
    }

    public static byte[] EncodeMode3(byte[] chunkData, int chunkId)
    {
        var compressedData = Decompressor.CompressBlock(chunkData);
        var decryptedWriter = new ByteWriter(new MemoryStream());
        decryptedWriter.WriteInt32(compressedData.Length);

        decryptedWriter.WriteBytes(compressedData);
        var encryptedData = TransformChunk(decryptedWriter.GetBuffer());
        var anotherWriter = new ByteWriter(new MemoryStream());
        anotherWriter.WriteInt32(encryptedData.Length - 12);
        anotherWriter.WriteBytes(encryptedData);
        return anotherWriter.GetBuffer();
    }

    public static unsafe byte[] TransformChunk(byte[] chunkData)
    {
        fixed (byte* inputChunkPtr = chunkData)
        {
            fixed (byte* keyPtr = _decryptionKey)
            {
                var outputChunkPtr =
                    NativeLib.decode_chunk(new IntPtr(inputChunkPtr), chunkData.Length, MagicChar, new IntPtr(keyPtr));
                var decodedChunk = new byte[chunkData.Length];
                Marshal.Copy(outputChunkPtr, decodedChunk, 0, chunkData.Length);
                Marshal.FreeHGlobal(outputChunkPtr);
                return decodedChunk;
            }
        }
    }
}