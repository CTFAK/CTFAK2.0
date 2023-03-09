using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN;

public enum ChunkFlags
{
    NotCompressed = 0, // MODE0
    Compressed = 1, // MODE1
    Encrypted = 2, // MODE2
    CompressedAndEncrypted = 3 //MODE3
}

public class Chunk
{
    public ChunkFlags Flag;
    public short Id;

    public byte[] Read(ByteReader reader)
    {
        Id = reader.ReadInt16();

        Flag = (ChunkFlags)reader.ReadInt16();
        var fileSize = reader.ReadInt32();
        var rawData = reader.ReadBytes(fileSize);
        var dataReader = new ByteReader(rawData);
        byte[] chunkData = null;
        switch (Flag)
        {
            case ChunkFlags.Encrypted:
                chunkData = Decryption.TransformChunk(dataReader.ReadBytes(fileSize));
                break;
            case ChunkFlags.CompressedAndEncrypted:
                chunkData = Decryption.DecodeMode3(dataReader.ReadBytes(fileSize), Id, out _ /* We don't care about decompressed size */);
                break;
            case ChunkFlags.Compressed:
                if (Settings.Old)
                {
                    var start = dataReader.Tell();
                    chunkData = Decompressor.DecompressOld(dataReader);
                    dataReader.Seek(start + fileSize);
                }
                else
                {
                    chunkData = Decompressor.Decompress(dataReader, out _);
                }

                break;
            case ChunkFlags.NotCompressed:
                chunkData = dataReader.ReadBytes(fileSize);
                break;
            default:
                throw new InvalidDataException("Unsupported chunk flag");
        }

        if (chunkData == null) Logger.Log($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");

        return chunkData;
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
                newWriter = new ByteWriter(new MemoryStream(Decryption.TransformChunk(dataWriter.ToArray())));
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