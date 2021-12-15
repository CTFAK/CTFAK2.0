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
        public object value;
        public object floatValue;
        public ChunkLoader Loader;
        public int Unk1;
        public ushort Unk2;
        private int _unk;
        public Expression(ByteReader reader) : base(reader) { }

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
            else if ((int) ObjectType >= 2 || (int) ObjectType == -7)
            {
                newWriter.WriteInt16((short) ObjectInfo);
                newWriter.WriteInt16((short) ObjectInfoList);
                if (Num == 16 || Num == 19) Loader.Write(newWriter);
            }
            Writer.WriteInt16((short) ((newWriter.Size() + 6)));
            Writer.WriteWriter(newWriter);

        }



        public override void Read()
        {
            var currentPosition = reader.Tell();
            var old = false;//Settings.GameType == GameType.OnePointFive&&!Settings.DoMFA;
            ObjectType = (old ? reader.ReadSByte():reader.ReadInt16());
            Num = old ? reader.ReadSByte():reader.ReadInt16();
            
            if (ObjectType == 0 && Num == 0) return;

            var size = reader.ReadInt16();
            if (ObjectType == (int)Constants.ObjectType.System)
            {
                if(Num==0) Loader=new LongExp(reader);
                else if(Num==3) Loader= new StringExp(reader);
                else if (Num == 23) Loader = new DoubleExp(reader);
                else if (Num == 24) Loader = new GlobalCommon(reader);
                else if (Num == 50) Loader = new GlobalCommon(reader);
                else if((int)ObjectType>=2|| (int)ObjectType==-7)
                {
                    ObjectInfo = reader.ReadUInt16();
                    ObjectInfoList = reader.ReadInt16();
                    if (Num == 16 || Num == 19)
                    {
                        Loader = new ExtensionExp(reader);
                    }
                    else
                    {
                        _unk = reader.ReadInt32();
                    }
                }
            }
            else if((int)ObjectType>=2|| (int)ObjectType==-7)
            {
                ObjectInfo = reader.ReadUInt16();
                ObjectInfoList = reader.ReadInt16();
                if (Num == 16 || Num == 19)
                {
                    Loader = new ExtensionExp(reader);
                }
            }
            Loader?.Read();
            // Unk1 = reader.ReadInt32();
            // Unk2 = reader.ReadUInt16();
            reader.Seek(currentPosition+size);


        }

        public override string ToString()
        {
            return $"Expression {ObjectType}=={Num}: {((ExpressionLoader)Loader)?.Value}";
        }
    }
    public class ExpressionLoader:ChunkLoader
    {
        public object Value;
        public ExpressionLoader(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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
        

        public StringExp(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            Value = reader.ReadUniversal();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUnicode((string) Value,true);
        }


    }
    public class LongExp:ExpressionLoader
    {
        public int Val1;

        public LongExp(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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
        public ExtensionExp(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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

        public DoubleExp(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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
        public GlobalCommon(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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
