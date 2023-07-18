using System.Drawing;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Multivar : ChunkLoader
    {
        public bool global;
        public bool isDouble;
        public double value;
        public int index;
        public int op;



        public override void Read(ByteReader reader)
        {
            index = reader.ReadInt32();
            op = reader.ReadInt32();
            if (isDouble)
            {
                value = reader.ReadDouble();
            }
            else
            {
                value = reader.ReadInt32();
                reader.Skip(4);
            }
                    

        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(index);
            writer.WriteInt32(op);
            if(isDouble)
            {
                writer.WriteDouble(value);
            }
            else
            {
                writer.WriteInt32((int)value);
                writer.WriteInt32(0);
            }
        }
    }
    class MultipleVariables : ParameterCommon
    {
        public int flags;
        public int flagMasks;
        public int flagValues;
        public Multivar[] values;

        public override void Read(ByteReader reader)
        {
            flags = reader.ReadInt32();
            flagMasks = reader.ReadInt32();
            flagValues = reader.ReadInt32();
            int mask = 1;
            int nValues;
            for (nValues = 0; nValues < 4; nValues++)
            {
                if ((flags & mask) == 0)
                    break;
                mask <<= 4;
            }
            values = new Multivar[nValues];
            int maskGlobal = 2;
            int maskDouble = 4;
            int i;
            for (i = 0; i < nValues; i++)
            {
                Multivar value = new Multivar();
                value.isDouble = (flags & maskDouble) != 0;
                value.Read(reader);
                maskGlobal <<= 4;
                maskDouble <<= 4;
                values[i] = value;
            }

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(flags);
            Writer.WriteInt32(flagMasks);
            Writer.WriteInt32(flagValues);
            foreach (var item in values)
            {
                item.Write(Writer);
            }
        }
    }
}
