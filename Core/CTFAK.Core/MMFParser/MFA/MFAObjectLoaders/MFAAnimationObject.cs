using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFAAnimationObject : ObjectLoader
{
    public bool _isExt;
    public Dictionary<int, MFAAnimation> Items = new();

    public override void Read(ByteReader reader)
    {
        base.Read(reader);

        if (reader.ReadByte() != 0)
        {
            var animationCount = reader.ReadUInt32();
            for (var i = 0; i < animationCount; i++)
            {
                var item = new MFAAnimation();
                item.Read(reader);
                Items.Add(i, item);
            }
        }
    }

    public void Write(ByteWriter writer, bool ext)
    {
        _isExt = ext;
        Write(writer);
    }

    public override void Write(ByteWriter writer)
    {
        base.Write(writer);
        if (_isExt)
        {
            writer.WriteInt8(0);
            writer.WriteInt32(-1);
        }
        else
        {
            writer.WriteInt8(1);
            writer.WriteUInt32((uint)Items.Count);
            foreach (var animation in Items.Values) animation.Write(writer);
        }
    }
}

public class MFAAnimation : ChunkLoader
{
    public List<MFAAnimationDirection> Directions;
    public string Name = "";

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Name);
        writer.WriteInt32(Directions.Count);
        foreach (var direction in Directions) direction.Write(writer);
    }

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();
        var directionCount = reader.ReadInt32();
        Directions = new List<MFAAnimationDirection>();
        for (var i = 0; i < directionCount; i++)
        {
            var direction = new MFAAnimationDirection();
            direction.Read(reader);
            Directions.Add(direction);
        }
    }
}

public class MFAAnimationDirection : ChunkLoader
{
    public int BackTo;
    public List<int> Frames = new();
    public int Index;
    public int MaxSpeed;
    public int MinSpeed;
    public string Name = "Animation-UNKNOWN";
    public int Repeat;

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Index);
        writer.WriteInt32(MinSpeed);
        writer.WriteInt32(MaxSpeed);
        writer.WriteInt32(Repeat);
        writer.WriteInt32(BackTo);
        writer.WriteInt32(Frames.Count);
        foreach (var frame in Frames) writer.WriteInt32(frame);
    }

    public override void Read(ByteReader reader)
    {
        Index = reader.ReadInt32();
        MinSpeed = reader.ReadInt32();
        MaxSpeed = reader.ReadInt32();
        Repeat = reader.ReadInt32();
        BackTo = reader.ReadInt32();
        var animCount = reader.ReadInt32();
        for (var i = 0; i < animCount; i++) Frames.Add(reader.ReadInt32());
    }
}