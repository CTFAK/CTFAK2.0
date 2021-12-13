using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class AnimationObject : ObjectLoader
    {
        public Dictionary<int, Animation> Items = new Dictionary<int, Animation>();
        public bool _isExt;

        public override void Read()
        {
            base.Read();

            if (reader.ReadByte() != 0)
            {
                var animationCount = reader.ReadUInt32();
                for (int i = 0; i < animationCount; i++)
                {
                    var item = new Animation(reader);
                    item.Read();
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
                foreach (Animation animation in Items.Values)
                {
                    animation.Write(Writer);
                }
            }

        }


        public AnimationObject(ByteReader reader) : base(reader) { }
    }

    public class Animation : DataLoader
    {
        public string Name = "";
        public List<AnimationDirection> Directions;

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(Directions.Count);
            foreach (AnimationDirection direction in Directions)
            {
                direction.Write(Writer);
            }
        }

        public override void Print()
        {
            Logger.Log($"   Found animation: {Name} ");
        }

        public override void Read()
        {
            Name = Reader.AutoReadUnicode();
            var directionCount = Reader.ReadInt32();
            Directions = new List<AnimationDirection>();
            for (int i = 0; i < directionCount; i++)
            {
                var direction = new AnimationDirection(Reader);
                direction.Read();
                Directions.Add(direction);
            }



        }
        public Animation(ByteReader reader) : base(reader) { }
    }

    public class AnimationDirection : DataLoader
    {
        public string Name = "Animation-UNKNOWN";
        public int Index;
        public int MinSpeed;
        public int MaxSpeed;
        public int Repeat;
        public int BackTo;
        public List<int> Frames = new List<int>();

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Index);
            Writer.WriteInt32(MinSpeed);
            Writer.WriteInt32(MaxSpeed);
            Writer.WriteInt32(Repeat);
            Writer.WriteInt32(BackTo);
            Writer.WriteInt32(Frames.Count);
            foreach (int frame in Frames)
            {
                Writer.WriteInt32(frame);
            }

        }

        public override void Print() { }

        public override void Read()
        {
            Index = Reader.ReadInt32();
            MinSpeed = Reader.ReadInt32();
            MaxSpeed = Reader.ReadInt32();
            Repeat = Reader.ReadInt32();
            BackTo = Reader.ReadInt32();
            var animCount = Reader.ReadInt32();
            for (int i = 0; i < animCount; i++)
            {
                Frames.Add(Reader.ReadInt32());
            }

        }
        public AnimationDirection(ByteReader reader) : base(reader) { }
    }
}
