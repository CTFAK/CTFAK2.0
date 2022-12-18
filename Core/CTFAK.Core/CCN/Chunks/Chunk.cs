using CTFAK.Memory;
using CTFAK.Utils;
using System.IO;

namespace CTFAK.CCN.Chunks
{
    public enum ChunkFlags
    {
        NotCompressed = 0,
        Compressed = 1,
        Encrypted = 2,
        CompressedAndEncrypted = 3
    }

    public class Chunk
    {
        public short Id;
        public ChunkFlags Flag;
        public int Size;

        public byte[] Read(ByteReader reader)
        {
            Id = reader.ReadInt16();

            Flag = (ChunkFlags)reader.ReadInt16();
            Size = reader.ReadInt32();
            var rawData = reader.ReadBytes(Size);
            var dataReader = new ByteReader(rawData);
            byte[] ChunkData = null;

            switch (Flag)
            {
                case ChunkFlags.Encrypted:
                    ChunkData = Decryption.DecryptChunk(dataReader.ReadBytes(Size), Size);
                    break;
                case ChunkFlags.CompressedAndEncrypted:
                    ChunkData = Decryption.DecodeMode3(dataReader.ReadBytes(Size), Size, Id, out var DecompressedSize);
                    break;
                case ChunkFlags.Compressed:
                    if (Settings.Old)
                    {
                        var start = dataReader.Tell();
                        ChunkData = Decompressor.DecompressOld(dataReader);
                        dataReader.Seek(start + Size);
                    }
                    else ChunkData = Decompressor.Decompress(dataReader, out DecompressedSize);

                    break;
                case ChunkFlags.NotCompressed:
                    ChunkData = dataReader.ReadBytes(Size);
                    break;
                default:
                    throw new InvalidDataException("Unsupported chunk flag");
            }

            if (ChunkData == null)
            {
                Logger.Log($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
            }

            return ChunkData;
        }

        public void Write(ByteWriter fileWriter, ByteWriter dataWriter)
        {
            
        }
    }

    public abstract class ChunkLoader
    {
        public abstract void Read(ByteReader reader);
        public abstract void Write(ByteWriter writer);
    }
}
