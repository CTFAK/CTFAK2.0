using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class Movements : ChunkLoader
{
    public List<Movement> Items = new();

    public override void Read(ByteReader reader)
    {
        var rootPosition = reader.Tell();
        var count = reader.ReadUInt32();
        var currentPos = reader.Tell();
        for (var i = 0; i < count; i++)
        {
            var mov = new Movement();
            mov.RootPos = (int)rootPosition;
            mov.Read(reader);
            Items.Add(mov);
            reader.Seek(currentPos + 16);
            currentPos = reader.Tell();
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Movement : ChunkLoader
{
    public static int DataSize;
    public int DirectionAtStart;
    public MovementLoader Loader;
    public byte MovingAtStart;
    public ushort Player;
    public int RootPos;
    public ushort Type;

    public override void Read(ByteReader reader)
    {
        if (Settings.Old)
        {
            Player = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            MovingAtStart = reader.ReadByte();
            reader.Skip(3);
            DirectionAtStart = reader.ReadInt32();
            if (Type == 5 && Settings.Old)
            {
                Type = 0;
                Loader = null;
                return;
            }

            switch (Type)
            {
                case 1:
                    Loader = new Mouse();
                    break;
                case 2:
                    Loader = new RaceMovement();
                    break;
                case 3:
                    Loader = new EightDirections();
                    break;
                case 4:
                    Loader = new Ball();
                    break;
                case 5:
                    Loader = new MovementPath();
                    break;
                case 9:
                    Loader = new PlatformMovement();
                    break;
                case 14:
                    Loader = new ExtensionsMovement();
                    break;
            }

            if (Loader == null && Type != 0) throw new Exception("Unsupported movement: " + Type);
            Loader?.Read(reader);
        }
        else
        {
            var nameOffset = reader.ReadInt32();
            var movementId = reader.ReadInt32();
            var newOffset = reader.ReadInt32();
            DataSize = reader.ReadInt32();
            reader.Seek(RootPos + newOffset);

            Player = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            MovingAtStart = reader.ReadByte();
            reader.Skip(3);
            DirectionAtStart = reader.ReadInt32();
            switch (Type)
            {
                case 1:
                    Loader = new Mouse();
                    break;
                case 2:
                    Loader = new RaceMovement();
                    break;
                case 3:
                    Loader = new EightDirections();
                    break;
                case 4:
                    Loader = new Ball();
                    break;
                case 5:
                    Loader = new MovementPath();
                    break;
                case 9:
                    Loader = new PlatformMovement();
                    break;
                case 14:
                    Loader = new ExtensionsMovement();
                    break;
            }

            if (Loader == null && Type != 0) return; //throw new Exception("Unsupported movement: "+Type);
            Loader?.Read(reader);
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class MovementLoader : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Mouse : MovementLoader
{
    private short _unusedFlags;
    public short X1;
    public short X2;
    public short Y1;
    public short Y2;

    public override void Read(ByteReader reader)
    {
        X1 = reader.ReadInt16();
        X2 = reader.ReadInt16();
        Y1 = reader.ReadInt16();
        Y2 = reader.ReadInt16();
        _unusedFlags = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(X1);
        writer.WriteInt16(X2);
        writer.WriteInt16(Y1);
        writer.WriteInt16(Y2);
        writer.WriteInt16(_unusedFlags);
    }
}

public class MovementPath : MovementLoader
{
    public byte Loop;
    public short MaximumSpeed;
    public short MinimumSpeed;
    public byte RepositionAtEnd;
    public byte ReverseAtEnd;
    public List<MovementStep> Steps;

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt16();
        MinimumSpeed = reader.ReadInt16();
        MaximumSpeed = reader.ReadInt16();
        Loop = reader.ReadByte();
        RepositionAtEnd = reader.ReadByte();
        ReverseAtEnd = reader.ReadByte();
        reader.Skip(1);
        Steps = new List<MovementStep>();
        for (var i = 0; i < count; i++)
        {
            var currentPosition = reader.Tell();

            reader.Skip(1);
            var size = reader.ReadByte();
            var step = new MovementStep();
            step.Read(reader);
            Steps.Add(step);
            reader.Seek(currentPosition + size);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)Steps.Count);
        writer.WriteInt16(MinimumSpeed);
        writer.WriteInt16(MaximumSpeed);
        writer.WriteInt8(Loop);
        writer.WriteInt8(RepositionAtEnd);
        writer.WriteInt8(ReverseAtEnd);
        writer.WriteInt8(0);
        foreach (var step in Steps)
        {
            writer.WriteInt8(0);
            var newWriter = new ByteWriter(new MemoryStream());
            step.Write(newWriter);
            writer.WriteInt8((byte)(newWriter.Size() + 2));
            writer.WriteWriter(newWriter);
        }
    }
}

public class MovementStep : MovementLoader
{
    public short Cosinus;
    public short DestinationX;
    public short DestinationY;
    public byte Direction;
    public short Length;
    public string Name;
    public short Pause;
    public short Sinus;
    public byte Speed;

    public override void Read(ByteReader reader)
    {
        Speed = reader.ReadByte();
        Direction = reader.ReadByte();
        DestinationX = reader.ReadInt16();
        DestinationY = reader.ReadInt16();
        Cosinus = reader.ReadInt16();
        Sinus = reader.ReadInt16();
        Length = reader.ReadInt16();
        Pause = reader.ReadInt16();
        Name = reader.ReadAscii();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt8(Speed);
        writer.WriteInt8(Direction);
        writer.WriteInt16(DestinationX);
        writer.WriteInt16(DestinationY);
        writer.WriteInt16(Cosinus);
        writer.WriteInt16(Sinus);
        writer.WriteInt16(Length);
        writer.WriteInt16(Pause);
        writer.WriteAscii(Name);
    }
}

public class Ball : MovementLoader
{
    public short Angles;
    public short Deceleration;
    public short Randomizer;
    public short Security;
    public short Speed;

    public override void Read(ByteReader reader)
    {
        Speed = reader.ReadInt16();
        Randomizer = reader.ReadInt16();
        Angles = reader.ReadInt16();
        Security = reader.ReadInt16();
        Deceleration = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Speed);
        writer.WriteInt16(Randomizer);
        writer.WriteInt16(Angles);
        writer.WriteInt16(Security);
        writer.WriteInt16(Deceleration);
    }
}

public class EightDirections : MovementLoader
{
    public short Acceleration;
    public short BounceFactor;
    public short Deceleration;
    public int Directions;
    public short Speed;

    public override void Read(ByteReader reader)
    {
        Speed = reader.ReadInt16();
        Acceleration = reader.ReadInt16();
        Deceleration = reader.ReadInt16();
        BounceFactor = reader.ReadInt16();
        Directions = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Speed);
        writer.WriteInt16(Acceleration);
        writer.WriteInt16(Deceleration);
        writer.WriteInt16(BounceFactor);
        writer.WriteInt32(Directions);
    }
}

public class RaceMovement : MovementLoader
{
    public short Acceleration;
    public short Angles;
    public short BounceFactor;
    public short Deceleration;
    public short ReverseEnabled;
    public short RotationSpeed;
    public short Speed;

    public override void Read(ByteReader reader)
    {
        Speed = reader.ReadInt16();
        Acceleration = reader.ReadInt16();
        Deceleration = reader.ReadInt16();
        RotationSpeed = reader.ReadInt16();
        BounceFactor = reader.ReadInt16();
        Angles = reader.ReadInt16();
        ReverseEnabled = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Speed);
        writer.WriteInt16(Acceleration);
        writer.WriteInt16(Deceleration);
        writer.WriteInt16(RotationSpeed);
        writer.WriteInt16(BounceFactor);
        writer.WriteInt16(Angles);
        writer.WriteInt16(ReverseEnabled);
    }
}

public class PlatformMovement : MovementLoader
{
    public short Acceleration;
    public short Control;
    public short Deceleration;
    public short Gravity;
    public short JumpStrength;
    public short Speed;

    public override void Read(ByteReader reader)
    {
        Speed = reader.ReadInt16();
        Acceleration = reader.ReadInt16();
        Deceleration = reader.ReadInt16();
        Control = reader.ReadInt16();
        Gravity = reader.ReadInt16();
        JumpStrength = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Speed);
        writer.WriteInt16(Acceleration);
        writer.WriteInt16(Deceleration);
        writer.WriteInt16(Control);
        writer.WriteInt16(Gravity);
        writer.WriteInt16(JumpStrength);
    }
}

public class ExtensionsMovement : MovementLoader
{
    public byte[] Data;

    public override void Read(ByteReader reader)
    {
        Data = reader.ReadBytes(Movement.DataSize);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteBytes(Data);
    }
}