using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Position : ParameterCommon
    {
        public int ObjectInfoParent;
        public short Flags;
        public int X;
        public int Y;
        public int Slope;
        public int Angle;
        public int Direction;
        public int TypeParent;
        public int ObjectInfoList;
        public int Layer;            

        public Position(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            ObjectInfoParent = reader.ReadInt16();
            Flags = reader.ReadInt16();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            Slope = reader.ReadInt16();
            Angle = reader.ReadInt16();
            Direction = reader.ReadInt32();
            TypeParent = reader.ReadInt16();
            ObjectInfoList = reader.ReadInt16();
            Layer = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short) ObjectInfoParent);
            Writer.WriteInt16(Flags);
            Writer.WriteInt16((short) X);
            Writer.WriteInt16((short) Y);
            Writer.WriteInt16((short) Slope);
            Writer.WriteInt16((short) Angle);
            Writer.WriteInt32(Direction);
            Writer.WriteInt16((short) TypeParent);
            Writer.WriteInt16((short) ObjectInfoList);
            Writer.WriteInt16((short) Layer);
        }

        public override string ToString()
        {
            return $"Object X:{X} Y:{Y} Angle:{Angle} Direction:{Direction} Parent:{ObjectInfoList}";
        }
    }
}
