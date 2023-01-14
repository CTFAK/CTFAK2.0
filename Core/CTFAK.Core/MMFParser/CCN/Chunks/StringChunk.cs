using System;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks;

public class StringChunk : ChunkLoader
{
    public string Value = "";

    public override void Read(ByteReader reader)
    {
        Value = reader.ReadUniversal();
        if (Value == null) Value = "";
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(0x2224, "AppName")]
internal class AppName : StringChunk
{
    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var str = reader.ReadAscii();
        if (str.Length + 1 == reader.Size())
        {
            Settings.Unicode = false;
            Value = str;
        }
        else
        {
            reader.Seek(start);
            Value = reader.ReadWideString();
            Settings.Unicode = true;
        }
    }
}

[ChunkLoader(0x2225, "AppAuthor")]
public class AppAuthor : StringChunk
{
}

[ChunkLoader(0x2227, "ExtPath")]
internal class ExtPath : StringChunk
{
}

[ChunkLoader(0x222E, "EditorFilename")]
public class EditorFilename : StringChunk
{
}

[ChunkLoader(0x222F, "TargetFilename")]
public class TargetFilename : StringChunk
{
}

internal class AppDoc : StringChunk
{
}

internal class AboutText : StringChunk
{
}

[ChunkLoader(0x223B, "Copyright")]
public class Copyright : StringChunk
{
}

internal class DemoFilePath : StringChunk
{
}