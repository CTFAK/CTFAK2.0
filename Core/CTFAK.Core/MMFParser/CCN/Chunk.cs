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

    public ChunkLoader Loader;

    public int FileOffset { get; private set; }
    public int FileSize { get; private set; }
    public int UnpackedSize { get; private set; }

    public byte[] Read(ByteReader reader)
    {
        FileOffset = (int)reader.Tell();
        Id = reader.ReadInt16();

        Flag = (ChunkFlags)reader.ReadInt16();
        FileSize = reader.ReadInt32();
        var rawData = reader.ReadBytes(FileSize);
        var dataReader = new ByteReader(rawData);
        byte[] chunkData = null;
        switch (Flag)
        {
            case ChunkFlags.Encrypted:
                chunkData = dataReader.ReadBytes(FileSize);
                Decryption.TransformChunk(chunkData);
                break;
            case ChunkFlags.CompressedAndEncrypted:
                chunkData = Decryption.DecodeMode3(dataReader.ReadBytes(FileSize), Id,
                    out _ /* We don't care about decompressed size */);
                break;
            case ChunkFlags.Compressed:
                if (Settings.Old)
                {
                    var start = dataReader.Tell();
                    chunkData = Decompressor.DecompressOld(dataReader);
                    dataReader.Seek(start + FileSize);
                }
                else
                {
                    chunkData = Decompressor.Decompress(dataReader, out _);
                }

                break;
            case ChunkFlags.NotCompressed:
                chunkData = dataReader.ReadBytes(FileSize);
                break;
        }

        if (chunkData == null)
            Logger.LogWarning($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
        if (chunkData?.Length == 0 && Id != 32639)
            Logger.LogWarning($"Chunk data is empty for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");

        UnpackedSize = chunkData.Length;
        return chunkData;
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteInt16(Id);
        writer.WriteInt16((short)Flag);
        ByteWriter newWriter = null;
        var dataWriter = new ByteWriter(new MemoryStream());
        Loader.Write(dataWriter);
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
                newWriter = new ByteWriter(Decryption.EncryptAndCompressMode3(dataWriter.ToArray(), Id));
                break;
        }

        writer.WriteWriter(newWriter);
    }
}

public abstract class ChunkLoader
{
    public abstract void Read(ByteReader reader);
    public abstract void Write(ByteWriter writer);
}