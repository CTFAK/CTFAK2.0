using System;
using System.IO;
using CTFAK.CCN;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Expressions
{
    public class Expression : ChunkLoader
    {
        public int ObjectType;
        public int Num;
        public int ObjectInfo;
        public int ObjectInfoList;
        public ChunkLoader Loader;
        public int Unk1;
        public ushort Unk2;
        private int _unk;

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short) ObjectType);
            Writer.WriteInt16((short) Num);
            if (ObjectType == 0 && Num == 0) return;
            var newWriter = new ByteWriter(new MemoryStream());
            if (ObjectType == (int)Constants.ObjectType.System &&
                (Num == 0 || Num == 3 || Num == 23 || Num == 24 || Num == 50))
            {
                if (Loader == null) throw new NotImplementedException("Broken expression: " + Num);
                Loader.Write(newWriter);
            }
            else if (ObjectType >= 2 || ObjectType == -7)
            {
                newWriter.WriteInt16((short) ObjectInfo);
                newWriter.WriteInt16((short) ObjectInfoList);
                if (Num == 16 || Num == 19) Loader.Write(newWriter);
            }
            Writer.WriteInt16((short)(newWriter.Size() + 6));
            Writer.WriteWriter(newWriter);
        }

        public override void Read(ByteReader reader)
        {
            var currentPosition = reader.Tell();
            var old = false;//Settings.GameType == GameType.OnePointFive&&!Settings.DoMFA;
            ObjectType = (old ? reader.ReadSByte():reader.ReadInt16());
            Num = old ? reader.ReadSByte():reader.ReadInt16();
            
            if (ObjectType == 0 && Num == 0) return;

            var size = reader.ReadInt16();
            if (ObjectType == (int)Constants.ObjectType.System)
            {
                if (Num == 0) Loader = new LongExp();
                else if (Num == 3) Loader = new StringExp();
                else if (Num == 23) Loader = new DoubleExp();
                else if (Num == 24) Loader = new GlobalCommon();
                else if (Num == 50) Loader = new GlobalCommon();
                else if(ObjectType >= 2 || ObjectType == -7)
                {
                    ObjectInfo = reader.ReadUInt16();
                    ObjectInfoList = reader.ReadInt16();
                    if (Num == 16 || Num == 19) Loader = new ExtensionExp();
                    else _unk = reader.ReadInt32();
                }
            }
            else if(ObjectType>=2 || ObjectType==-7)
            {
                ObjectInfo = reader.ReadUInt16();
                ObjectInfoList = reader.ReadInt16();
                if (Num == 16 || Num == 19) Loader = new ExtensionExp();
            }
            Loader?.Read(reader);
            // Unk1 = reader.ReadInt32();
            // Unk2 = reader.ReadUInt16();
            reader.Seek(currentPosition + size);
        }

        public override string ToString()
        {
            return $"Object Type: {ObjectType}, Num: {Num}, Info: {ObjectInfo}, List: {ObjectInfoList}, Loader: {((ExpressionLoader)Loader).Value}, Unknown: {Unk1} {Unk2} {_unk}";
        }
    }
    public class ExpressionLoader:ChunkLoader
    {
        public object Value;



        public override void Read(ByteReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }


    }

    public class StringExp:ExpressionLoader
    {
        





        public override void Read(ByteReader reader)
        {
            Value = reader.ReadYuniversal();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUnicode((string) Value,true);
        }


    }
    public class LongExp:ExpressionLoader
    {
        public int Val1;





        public override void Read(ByteReader reader)
        {
            Value = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32((int) Value);
        }
    }
    public class ExtensionExp:ExpressionLoader
    {
   



        public override void Read(ByteReader reader)
        {
            Value = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
           Writer.WriteInt16((short) Value);
        }
    }
    public class DoubleExp:ExpressionLoader
    {
        public float FloatValue;





        public override void Read(ByteReader reader)
        {
            Value = reader.ReadDouble();
            FloatValue = reader.ReadSingle();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteDouble((double) Value);
            Writer.WriteSingle(FloatValue);
        }
    }
    public class GlobalCommon:ExpressionLoader
    {




        public override void Read(ByteReader reader)
        {
            reader.ReadInt32();
            Value = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(0);
            Writer.WriteInt32((int) Value);
        }
    }
}
