using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public class Movements : ChunkLoader
    {
        public List<Movement> Items = new List<Movement>();




        public override void Read(ByteReader reader)
        {
            var rootPosition = reader.Tell();
            var count = reader.ReadUInt32();
            var currentPos = reader.Tell();
            for (int i = 0; i < count; i++)
            {
                var mov = new Movement();
                mov.rootPos = (int)rootPosition;
                mov.Read(reader);
                Items.Add(mov);
                reader.Seek(currentPos + 16);
                currentPos = reader.Tell();
            }
        }

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }



    }

    public class Movement : ChunkLoader
    {
        public static int DataSize;
        public int rootPos;
        public ushort Player;
        public ushort Type;
        public byte MovingAtStart;
        public int DirectionAtStart;
        public MovementLoader Loader;



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
                reader.Seek(rootPos + newOffset);

                Player = reader.ReadUInt16();
                Type = reader.ReadUInt16();
                MovingAtStart = reader.ReadByte();
                var Moving = reader.ReadByte();
                reader.Skip(2);
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



        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }


    }
    public class MovementLoader : ChunkLoader
    {
 



        public override void Read(ByteReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }
    public class Mouse : MovementLoader
    {
        public short X1;
        public short X2;
        public short Y1;
        public short Y2;
        private short _unusedFlags;





        public override void Read(ByteReader reader)
        {
            X1 = reader.ReadInt16();
            X2 = reader.ReadInt16();
            Y1 = reader.ReadInt16();
            Y2 = reader.ReadInt16();
            _unusedFlags = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(X1);
            Writer.WriteInt16(X2);
            Writer.WriteInt16(Y1);
            Writer.WriteInt16(Y2);
            Writer.WriteInt16(_unusedFlags);

        }
    }
    public class MovementPath : MovementLoader
    {
        public short MinimumSpeed;
        public short MaximumSpeed;
        public byte Loop;
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
            for (int i = 0; i < count; i++)
            {
                var currentPosition = reader.Tell();

                var step = new MovementStep();
                step.Read(reader);
                Steps.Add(step);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short)Steps.Count); // 22 | 1
            Writer.WriteInt16(MinimumSpeed); // 0 | 1
            Writer.WriteInt16(MaximumSpeed); // 100 | 50
            Writer.WriteInt8(Loop); // 1 | 0
            Writer.WriteInt8(RepositionAtEnd); // 1 | 0
            Writer.WriteInt8(ReverseAtEnd); // 1 | 0
            Writer.WriteInt8(0);
            int i = 0;
            foreach (MovementStep step in Steps)
            {
                var newWriter = new ByteWriter(new MemoryStream());
                step.Write(newWriter);
                Writer.WriteWriter(newWriter);
            }
        }
    }
    public class MovementStep : MovementLoader
    {
        public byte Flags;
        public byte Size;
        public byte Speed;
        public byte Direction;
        public short DestinationX;
        public short DestinationY;
        public short Cosinus;
        public short Sinus;
        public short Length;
        public short Pause;
        public byte[] Data;

        public override void Read(ByteReader reader)
        {
            Flags = reader.ReadByte();
            Size = reader.ReadByte();
            Speed = reader.ReadByte();
            Direction = reader.ReadByte();
            DestinationX = reader.ReadInt16();
            DestinationY = reader.ReadInt16();
            Cosinus = reader.ReadInt16();
            Sinus = reader.ReadInt16();
            Length = reader.ReadInt16();
            Pause = reader.ReadInt16();
            Data = reader.ReadBytes(Size - 16);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt8(Flags);
            Writer.WriteInt8(Size);
            Writer.WriteInt8(Speed);
            Writer.WriteInt8(Direction);
            Writer.WriteInt16(DestinationX);
            Writer.WriteInt16(DestinationY);
            Writer.WriteInt16(Cosinus);
            Writer.WriteInt16(Sinus);
            Writer.WriteInt16(Length);
            Writer.WriteInt16(Pause);
            Writer.WriteBytes(Data);
        }
    }
    public class Ball : MovementLoader
    {
        public short Speed;
        public short Randomizer;
        public short Angles;
        public short Security;
        public short Deceleration;




        public override void Read(ByteReader reader)
        {
            Speed = reader.ReadInt16();
            Randomizer = reader.ReadInt16();
            Angles = reader.ReadInt16();
            Security = reader.ReadInt16();
            Deceleration = reader.ReadInt16();


        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Speed);
            Writer.WriteInt16(Randomizer);
            Writer.WriteInt16(Angles);
            Writer.WriteInt16(Security);
            Writer.WriteInt16(Deceleration);

        }
    }
    public class EightDirections : MovementLoader
    {
        public short Speed;
        public short Acceleration;
        public short Deceleration;
        public int Directions;
        public short BounceFactor;




        public override void Read(ByteReader reader)
        {
            Speed = reader.ReadInt16();
            Acceleration = reader.ReadInt16();
            Deceleration = reader.ReadInt16();
            BounceFactor = reader.ReadInt16();
            Directions = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Speed);
            Writer.WriteInt16(Acceleration);
            Writer.WriteInt16(Deceleration);
            Writer.WriteInt16(BounceFactor);
            Writer.WriteInt32(Directions);
        }
    }
    public class RaceMovement : MovementLoader
    {
        public short Speed;
        public short Acceleration;
        public short Deceleration;
        public short RotationSpeed;
        public short BounceFactor;
        public short Angles;
        public short ReverseEnabled;




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

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Speed);
            Writer.WriteInt16(Acceleration);
            Writer.WriteInt16(Deceleration);
            Writer.WriteInt16(RotationSpeed);
            Writer.WriteInt16(BounceFactor);
            Writer.WriteInt16(Angles);
            Writer.WriteInt16(ReverseEnabled);
        }
    }
    public class PlatformMovement : MovementLoader
    {
        public short Speed;
        public short Acceleration;
        public short Deceleration;
        public short Control;
        public short Gravity;
        public short JumpStrength;





        public override void Read(ByteReader reader)
        {
            Speed = reader.ReadInt16();
            Acceleration = reader.ReadInt16();
            Deceleration = reader.ReadInt16();
            Control = reader.ReadInt16();
            Gravity = reader.ReadInt16();
            JumpStrength = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Speed);
            Writer.WriteInt16(Acceleration);
            Writer.WriteInt16(Deceleration);
            Writer.WriteInt16(Control);
            Writer.WriteInt16(Gravity);
            Writer.WriteInt16(JumpStrength);

        }
    }
    public class ExtensionsMovement : MovementLoader
    {
        public byte[] Data;


        public override void Read(ByteReader reader)
        {
            Data = reader.ReadBytes(Movement.DataSize);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteBytes(Data);
        }
    }
}