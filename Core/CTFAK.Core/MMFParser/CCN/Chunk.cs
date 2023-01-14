using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN;

public enum ChunkFlags
{
    NotCompressed = 0,
    Compressed = 1,
    Encrypted = 2,
    CompressedAndEncrypted = 3
}

public class Chunk
{
    public ChunkFlags Flag;
    public short Id;
    public int Size;

    public byte[] Read(ByteReader reader)
    {
        Id = reader.ReadInt16();

        Flag = (ChunkFlags)reader.ReadInt16();
        Size = reader.ReadInt32();
        var rawData = reader.ReadBytes(Size);
        var dataReader = new ByteReader(rawData);
        byte[] chunkData = null;
        switch (Flag)
        {
            case ChunkFlags.Encrypted:
                chunkData = Decryption.TransformChunk(dataReader.ReadBytes(Size), Size);
                break;
            case ChunkFlags.CompressedAndEncrypted:
                chunkData = Decryption.DecodeMode3(dataReader.ReadBytes(Size), Size, Id, out _ /* We don't care about decompressed size */);
                break;
            case ChunkFlags.Compressed:
                if (Settings.Old)
                {
                    var start = dataReader.Tell();
                    chunkData = Decompressor.DecompressOld(dataReader);
                    dataReader.Seek(start + Size);
                }
                else
                {
                    chunkData = Decompressor.Decompress(dataReader, out _);
                }

                break;
            case ChunkFlags.NotCompressed:
                chunkData = dataReader.ReadBytes(Size);
                break;
            default:
                throw new InvalidDataException("Unsupported chunk flag");
        }

        if (chunkData == null) Logger.Log($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");

        return chunkData;
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