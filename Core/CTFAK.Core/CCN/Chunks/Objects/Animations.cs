using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public class Animations : ChunkLoader
    {
        public Dictionary<int, Animation> AnimationDict;

        public override void Read(ByteReader reader)
        {
            var currentPosition = reader.Tell();
            var size = reader.ReadInt16();
            var count = reader.ReadInt16();

            var offsets = new List<short>();
            for (int i = 0; i < count; i++)
            {
                offsets.Add(reader.ReadInt16());
            }
            AnimationDict = new Dictionary<int, Animation>();
            for (int i = 0; i < offsets.Count; i++)
            {
                var offset = offsets[i];
                if (offset != 0)
                {
                    reader.Seek(currentPosition + offset);
                    var anim = new Animation();

                    anim.Read(reader);
                    AnimationDict.Add(i, anim);
                }
                else
                {
                    AnimationDict.Add(i, new Animation());
                }
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Animation : ChunkLoader
    {
        public Dictionary<int, AnimationDirection> DirectionDict;


        public override void Read(ByteReader reader)
        {
            var currentPosition = reader.Tell();
            var offsets = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                offsets.Add(reader.ReadInt16());
            }

            DirectionDict = new Dictionary<int, AnimationDirection>();
            for (int i = 0; i < offsets.Count; i++)
            {
                var offset = offsets[i];
                if (offset != 0)
                {
                    reader.Seek(currentPosition + offset);
                    var dir = new AnimationDirection();
                    dir.Read(reader);
                    DirectionDict.Add(i, dir);
                }
                else
                {
                    DirectionDict.Add(i, new AnimationDirection());
                }

            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }



    }

    public class AnimationDirection : ChunkLoader
    {
        public int MinSpeed;
        public int MaxSpeed;
        public bool HasSingle;
        public int Repeat;
        public int BackTo;
        public List<int> Frames = new List<int>();



        public override void Read(ByteReader reader)
        {
            MinSpeed = reader.ReadSByte();
            MaxSpeed = reader.ReadSByte();
            Repeat = reader.ReadInt16();
            BackTo = reader.ReadInt16();
            var frameCount = reader.ReadUInt16();
            for (int i = 0; i < frameCount; i++)
            {
                var handle = reader.ReadInt16();
                Frames.Add(handle);


            }


        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }


    }
}