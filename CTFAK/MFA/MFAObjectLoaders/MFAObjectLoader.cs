using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class ObjectLoader : ChunkLoader
    {
        public int ObjectFlags;
        public int NewObjectFlags;
        public Color BackgroundColor;
        public short[] Qualifiers = new short[8];
        public MFAValueList Values;
        public MFAValueList Strings;
        public Movements Movements;
        public Behaviours Behaviours;

        public override void Write(ByteWriter Writer)
        {
            // if(Qualifiers==null) throw new NullReferenceException("QUALIFIERS NULL");
            Writer.WriteInt32((int)ObjectFlags);
            Writer.WriteInt32(NewObjectFlags);
            // var col = Color.FromArgb(255,BackgroundColor.R,BackgroundColor.G,BackgroundColor.B);
            Writer.WriteColor(BackgroundColor);


            for (int i = 0; i < 8; i++)
            {
                Writer.WriteInt16(Qualifiers[i]);
            }
            Writer.WriteInt16(-1);
            Values.Write(Writer);
            Strings.Write(Writer);
            Movements.Write(Writer);
            Behaviours.Write(Writer);

            Writer.WriteInt8(0);//FadeIn
            Writer.WriteInt8(0);//FadeOut


        }



        public override void Read()
        {
            ObjectFlags = Reader.ReadInt32();
            NewObjectFlags = Reader.ReadInt32();
            BackgroundColor = Reader.ReadColor();
            var end = Reader.Tell() + 2 * (8 + 1);
            for (int i = 0; i < 8; i++)
            {
                var value = Reader.ReadInt16();
                // if(value==-1)
                // {
                // break;
                // }
                Qualifiers[i] = value;
            }
            Reader.Seek(end);

            Values = new MFAValueList(Reader);
            Values.Read();
            Strings = new MFAValueList(Reader);
            Strings.Read();
            Movements = new Movements(Reader);
            Movements.Read();
            Behaviours = new Behaviours(Reader);
            Behaviours.Read();
            Reader.Skip(2);//Transitions
                           // Print();



        }
        public ObjectLoader(ByteReader reader) : base(reader) { }
    }
}
