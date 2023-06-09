using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Utils;

namespace CTFAK.Memory;

public static class Decryption
{
    public static byte[] DecryptionKey;

    public const byte MagicChar = 54;

    //private static byte* decodeBuffer;
    private static readonly byte[] DecodeBuffer = new byte[256];
    public static bool valid;

    public static byte[] KeyString(string str)
    {
        // thank you LAK
        // fuck you openai
        // this code is quite stupid, but i will not touch it for now
        var result = new List<byte>();
        result.Capacity = str.Length * 2;
        foreach (var code in str)
        {
            if ((code & 0xFF) != 0) result.Add((byte)(code & 0xFF));

            if (((code >> 8) & 0xFF) != 0) result.Add((byte)((code >> 8) & 0xFF));
        }

        return result.ToArray();
    }

    public static byte[] MakeKeyCombined(byte[] data)
    {
        var dataLen = data.Length;
        Array.Resize(ref data, 256);

        var lastKeyByte = MagicChar;
        var v34 = MagicChar;

        for (var i = 0; i <= dataLen; i++)
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
        var combinedData = data1 + data2 + data3;
        Console.WriteLine("Data: " + combinedData);
        Console.WriteLine("Total length: " + combinedData.Length);
        DecryptionKey = MakeKeyCombined(bytes.ToArray());
        InitDecryptionTable(DecryptionKey, MagicChar);
    }


    public static byte[] DecodeMode3(byte[] chunkData, int chunkId, out int decompressed)
    {
        var reader = new ByteReader(chunkData);
        var decompressedSize = reader.ReadUInt32();

        var rawData = reader.ReadBytes((int)reader.Size());

        if ((chunkId & 1) == 1 && Settings.Build > 284)
            rawData[0] ^= (byte)((byte)(chunkId & 0xFF) ^ (byte)(chunkId >> 0x8));
        TransformChunk(rawData);

        using (var data = new ByteReader(rawData))
        {
            var compressedSize = data.ReadUInt32();
            decompressed = (int)decompressedSize;
            return Decompressor.DecompressBlock(data, (int)compressedSize);
        }
    }

    public static byte[] EncryptAndCompressMode3(byte[] chunkData, int chunkId)
    {
        var compressedData = Decompressor.CompressBlock(chunkData);
        var decryptedWriter = new ByteWriter(new MemoryStream());
        decryptedWriter.WriteInt32(compressedData.Length);

        decryptedWriter.WriteBytes(compressedData);
        var encryptedData = decryptedWriter.GetBuffer();
        TransformChunk(encryptedData);
        var anotherWriter = new ByteWriter(new MemoryStream());
        anotherWriter.WriteInt32(encryptedData.Length - 12);
        anotherWriter.WriteBytes(encryptedData);
        return anotherWriter.GetBuffer();
    }

    // Thx LAK
    // It's 0:40, I might revisit this part again when I don't feel as crappy
    // I might even just redo it with a single byte array with 256 elements
    // Revisiting this day after, I literally did just that. Wasn't hard at all
    // But hey, at least this works ;)
    public static bool InitDecryptionTable(byte[] magicKey, byte magicChar)
    {
        //decodeBuffer = (byte*)Marshal.AllocHGlobal(256);
        for (var i = 0; i < 256; i++) DecodeBuffer[i] = (byte)i;

        Func<byte, byte> rotate = value => (byte)((value << 7) | (value >> 1));

        var accum = magicChar;
        var hash = magicChar;

        var never_reset_key = true;

        byte i2 = 0;
        byte key = 0;
        for (uint i = 0; i < 256; ++i, ++key)
        {
            hash = rotate(hash);

            if (never_reset_key)
            {
                accum += (byte)((hash & 1) == 0 ? 2 : 3);
                accum *= magicKey[key];
            }

            if (hash == magicKey[key])
            {
                /*if (never_reset_key && !(accum == magic_key[key+1]))
                {
                    // Ignoring this, because it's not being triggered by the same input data in c++
                    
                    //Console.WriteLine("Failed To Generate Decode Table");
                    //return false;
                }*/

                hash = rotate(magicChar);
                key = 0;

                never_reset_key = false;
            }

            i2 += (byte)((hash ^ magicKey[key]) + DecodeBuffer[i]);

            (DecodeBuffer[i2], DecodeBuffer[i]) = (DecodeBuffer[i], DecodeBuffer[i2]);
        }

        valid = true;
        return true;
    }

    public static bool TransformChunk(byte[] chunk)
    {
        if (!valid) return false;
        var tempBuf = new byte[256];
        Array.Copy(DecodeBuffer, tempBuf, 256);

        byte i = 0;
        byte i2 = 0;
        for (var j = 0; j < chunk.Length; j++)
        {
            ++i;
            i2 += tempBuf[i];
            (tempBuf[i2], tempBuf[i]) = (tempBuf[i], tempBuf[i2]);
            var xor = tempBuf[(byte)(tempBuf[i] + tempBuf[i2])];
            chunk[j] ^= xor;
        }

        return true;
    }
}