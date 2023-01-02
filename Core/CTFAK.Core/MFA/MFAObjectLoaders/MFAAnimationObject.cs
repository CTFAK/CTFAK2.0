using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MFA.MFAObjectLoaders;

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

    public void Write(ByteWriter Writer, bool ext)
    {
        _isExt = ext;
        Write(Writer);
    }

    public override void Write(ByteWriter Writer)
    {
        base.Write(Writer);
        if (_isExt)
        {
            Writer.WriteInt8(0);
            Writer.WriteInt32(-1);
        }
        else
        {
            Writer.WriteInt8(1);
            Writer.WriteUInt32((uint)Items.Count);
            foreach (var animation in Items.Values) animation.Write(Writer);
        }
    }
}

public class MFAAnimation : ChunkLoader
{
    public List<MFAAnimationDirection> Directions;
    public string Name = "";

    public override void Write(ByteWriter Writer)
    {
        Writer.AutoWriteUnicode(Name);
        Writer.WriteInt32(Directions.Count);
        foreach (var direction in Directions) direction.Write(Writer);
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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Index);
        Writer.WriteInt32(MinSpeed);
        Writer.WriteInt32(MaxSpeed);
        Writer.WriteInt32(Repeat);
        Writer.WriteInt32(BackTo);
        Writer.WriteInt32(Frames.Count);
        foreach (var frame in Frames) Writer.WriteInt32(frame);
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