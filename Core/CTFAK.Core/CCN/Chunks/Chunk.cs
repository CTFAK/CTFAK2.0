using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
                    ChunkData = dataReader.ReadBytes(Size);
                    Decryption.TransformChunk(ChunkData);
                    break;
                case ChunkFlags.CompressedAndEncrypted:
                    ChunkData = Decryption.DecodeMode3(dataReader.ReadBytes(Size), Id, out var DecompressedSize);
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
            
            if (ChunkData == null)
            {
                Logger.Log($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
            }
            if (ChunkData?.Length == 0 && Id != 32639)
            {
                Logger.Log($"Chunk data is empty for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
            }
            return ChunkData;
        }

        public void Write(ByteWriter fileWriter, ByteWriter dataWriter)
        {

            fileWriter.WriteInt16(Id);
            fileWriter.WriteInt16((short)Flag);
            ByteWriter newWriter = null;
            switch (Flag)
            {
                case ChunkFlags.NotCompressed:
                    newWriter = dataWriter;
                    break;
                case ChunkFlags.Encrypted:
                    //newWriter = new ByteWriter(new MemoryStream(Decryption.TransformChunk(dataWriter.ToArray())));
                    break;
                case ChunkFlags.Compressed:
                    newWriter = Decompressor.Compress(dataWriter.ToArray());
                    break;
                case ChunkFlags.CompressedAndEncrypted:
                    // TODO Implement
                    break;

            }
            fileWriter.WriteWriter(newWriter);

        }
    }
    public abstract class ChunkLoader
    {

        public abstract void Read(ByteReader reader);

        public abstract void Write(ByteWriter writer);

    }
}
