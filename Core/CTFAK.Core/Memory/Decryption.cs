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

    public static byte[] KeyString(string str)
    {
        // thank you LAK
        // fuck you openai
        // this code is quite stupid, but i will not touch it for now
        var result = new List<byte>();
        result.Capacity = str.Length * 2;
        foreach (char code in str)
        {
            if ((code & 0xFF) != 0)
            {
                result.Add((byte)(code & 0xFF));
            }
            if (((code >> 8) & 0xFF) != 0)
            {
                result.Add((byte)((code >> 8) & 0xFF));
            }
        }
        return result.ToArray();
    }
    public static byte[] MakeKeyCombined(byte[] data)
    {
        int dataLen = data.Length;
        Array.Resize(ref data, 256);
        
        byte lastKeyByte = MagicChar;
        byte v34 = MagicChar;

        for (int i = 0; i <= dataLen; i++)
        {
            v34 = (byte)((v34 << 7) + (v34 >> 1));
            data[i] ^= v34;
            lastKeyByte += (byte)(data[i] * ((v34 & 1) + 2));
        }
        
        data[dataLen + 1] = lastKeyByte;
        return data;
    }
    public static void MakeKey(string data1, string data2, string data3)
    {
        var bytes = new List<byte>();
        bytes.AddRange(KeyString(data1 ?? ""));
        bytes.AddRange(KeyString(data2 ?? ""));
        bytes.AddRange(KeyString(data3 ?? ""));
        _decryptionKey = MakeKeyCombined(bytes.ToArray()); 
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