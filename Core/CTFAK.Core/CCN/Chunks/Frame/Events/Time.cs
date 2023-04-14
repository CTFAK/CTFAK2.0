using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Time : ParameterCommon
    {
        public int Timer;
        public int Loops;
        public short Comparsion;

        public override void Read(ByteReader reader)
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
