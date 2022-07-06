using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Chunk(ByteReader reader)
        {
            this.reader = reader;
        }
        ByteReader reader;
        public short Id;
        public ChunkFlags Flag;
        public int Size;

        public byte[] Read()
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
            }
            return ChunkData;

        }
    }
    public abstract class ChunkLoader
    {
        public ByteReader reader;
        public ChunkLoader(ByteReader reader)
        {
            this.reader = reader;
        }
        public abstract void Read();

        public abstract void Write(ByteWriter writer);

    }
}
