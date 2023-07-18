using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CTFAK.Memory
{
    public static class Decryption
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
                    result.Add((byte)(code & 0xFF));

                if (((code >> 8) & 0xFF) != 0)
                    result.Add((byte)((code >> 8) & 0xFF));
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
            InitDecryptionTable(_decryptionKey, Decryption.MagicChar);
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

        //private static byte* decodeBuffer;
        private static byte[] decodeBuffer = new byte[256];
        public static bool valid;

        // Thx LAK
        // It's 0:40, I might revisit this part again when I don't feel as crappy
        // I might even just redo it with a single byte array with 256 elements
        // But hey, at least this works ;)
        public static bool InitDecryptionTable(byte[] magic_key, byte magic_char)
        {
            //decodeBuffer = (byte*)Marshal.AllocHGlobal(256);
            for (int i = 0; i < 256; i++)
            {
                decodeBuffer[i] = (byte)i;
            }

            Func<byte, byte> rotate = (byte value) => (byte)((value << 7) | (value >> 1));

            byte accum = (byte)magic_char;
            byte hash = (byte)magic_char;

            bool never_reset_key = true;

            byte i2 = 0;
            byte key = 0;
            for (uint i = 0; i < 256; ++i, ++key)
            {

                hash = rotate(hash);

                if (never_reset_key)
                {
                    accum += ((hash & 1) == 0) ? (byte)2 : (byte)3;
                    accum *= magic_key[key];
                }

                if (hash == magic_key[key])
                {
                    /*if (never_reset_key && !(accum == *(key + 1)))
                    {
                        // Ignoring this, because it's not being triggered by the same input data in c++

                        //Console.WriteLine("Failed To Generate Decode Table");
                        //return false;
                    }*/

                    hash = rotate((byte)magic_char);
                    key = 0;

                    never_reset_key = false;
                }

                i2 += (byte)((hash ^ magic_key[key]) + decodeBuffer[i]);

                (decodeBuffer[i2], decodeBuffer[i]) = (decodeBuffer[i], decodeBuffer[i2]);
            }
            valid = true;
            return true;
        }

        public static bool TransformChunk(byte[] chunk)
        {
            if (!valid) return false;
            byte[] tempBuf = new byte[256];
            Array.Copy(decodeBuffer, tempBuf, 256);

            byte i = 0;
            byte i2 = 0;
            for (int j = 0; j < chunk.Length; j++)
            {
                ++i;
                i2 += (byte)tempBuf[i];
                (tempBuf[i2], tempBuf[i]) = (tempBuf[i], tempBuf[i2]);
                var xor = tempBuf[(byte)(tempBuf[i] + tempBuf[i2])];
                chunk[j] ^= xor;
            }
            return true;
        }
    }
}