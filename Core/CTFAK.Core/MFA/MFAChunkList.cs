using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAChunkList : ChunkLoader//This is used for MFA reading/writing
    {
        public byte[] Saved;
        public List<MFAChunk> Items = new List<MFAChunk>();
        public bool Log = false;

        public T GetOrCreateChunk<T>() where T : MFAChunkLoader, new()
        {
            foreach (MFAChunk chunk in Items)
            {
                if (chunk.Loader.GetType() == typeof(T))
                {
                    return (T)chunk.Loader;
                }
            }
            var newChunk = new MFAChunk(null);
            if (typeof(T)==typeof(Opacity)) newChunk.Id = 45;
            else if (typeof(T)==typeof(FrameVirtualRect)) newChunk.Id = 33;
            newChunk.Loader = new T();
            Items.Add(newChunk);
            return (T)newChunk.Loader;
        }

        public bool ContainsChunk<T>() where T : MFAChunkLoader
        {
            foreach (MFAChunk chunk in Items)
            {
                if (chunk.Loader.GetType() == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }

        public MFAChunk NewChunk<T>() where T : MFAChunkLoader, new()
        {
            var newChunk = new MFAChunk(null);
            newChunk.Id = 33;
            newChunk.Loader = new T();
            return newChunk;
        }
        public override void Write(ByteWriter Writer)
        {
            foreach (MFAChunk chunk in Items)
            {
                chunk.Write(Writer);
            }
            Writer.WriteInt8(0);
        }




        public override void Read(ByteReader reader)
        {
            var start = reader.Tell();
            while (true)
            {
                var newChunk = new MFAChunk(reader);
                newChunk.Read();
                if (newChunk.Id == 0) break;
                else Items.Add(newChunk);



            }

            var size = reader.Tell() - start;
            reader.Seek(start);
            Saved = reader.ReadBytes((int)size);


        }
    }


    public class MFAChunk
    {

        public ByteReader Reader;
        public MFAChunkLoader Loader;
        public byte Id;
        public byte[] Data;

        public MFAChunk(ByteReader reader)
        {
            Reader = reader;
        }
        public void Read()
        {
            Id = Reader.ReadByte();
            if (Id == 0) return;
            var size = Reader.ReadInt32();
            Data = Reader.ReadBytes(size);
            var dataReader = new ByteReader(Data);
            switch (Id)
            {
                case 33:
                    Loader = new FrameVirtualRect();

                    break;

                case 45:
                    Loader = new Opacity();
                    break;
                default:
                    Loader = null;
                    // Logger.Log($"{Id} - {Data.GetHex()}");
                    break;

            }
            Loader?.Read(dataReader);



        }

        public void Write(ByteWriter writer)
        {
            writer.WriteInt8(Id);
            if (Id == 0) return;
            if (Loader == null)
            {
                writer.WriteInt32(Data.Length);
                writer.WriteBytes(Data);
            }
            else
            {
                var newWriter = new ByteWriter(new MemoryStream());
                Loader.Write(newWriter);
                writer.WriteInt32((int)newWriter.Size());
                writer.WriteWriter(newWriter);
            }



        }
    }

    public class Opacity : MFAChunkLoader
    {
        public Color RGBCoeff;
        public byte Blend;




        public override void Read(ByteReader reader)
        {
            var b = reader.ReadByte();
            var g = reader.ReadByte();
            var r = reader.ReadByte();
            Blend = reader.ReadByte();
            RGBCoeff = Color.FromArgb(Blend, r, g, b);
            var unk = reader.ReadInt32();

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt8(RGBCoeff.B);
            Writer.WriteInt8(RGBCoeff.G);
            Writer.WriteInt8(RGBCoeff.R);
            Writer.WriteInt8(Blend);
            Writer.WriteInt32(0);
        }
    }

    public class FrameVirtualRect : MFAChunkLoader
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override void Read(ByteReader reader)
        {
            Left = reader.ReadInt32();
            Top = reader.ReadInt32();
            Right = reader.ReadInt32();
            Bottom = reader.ReadInt32();

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Left);
            Writer.WriteInt32(Top);
            Writer.WriteInt32(Right);
            Writer.WriteInt32(Bottom);
        }
    }
    public abstract class MFAChunkLoader
    {




        public abstract void Read(ByteReader reader);
        public abstract void Write(ByteWriter Writer);
    }
}
