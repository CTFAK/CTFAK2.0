using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class Time : ParameterCommon
    {
        public int Timer;
        public int Loops;
        public short Comparsion;

        public Time(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            Timer = reader.ReadInt32();
            Loops = reader.ReadInt32();
            Comparsion = reader.ReadInt16();

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Timer);
            Writer.WriteInt32(Loops);
            Writer.WriteInt16(Comparsion);
        }

        public override string ToString()
        {

            return $"Time time: {Timer} loops: {Loops}";
        }
    }
}
