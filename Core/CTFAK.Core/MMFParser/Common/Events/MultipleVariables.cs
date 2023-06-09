using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.Common.Events;

public class Multivar : ChunkLoader
{
    public bool global;
    public int index;
    public bool isDouble;
    public int op;
    public double value;

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
        if (isDouble)
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

internal class MultipleVariables : ParameterCommon
{
    public int flagMasks;
    public int flags;
    public int flagValues;
    public Multivar[] values;

    public override void Read(ByteReader reader)
    {
        flags = reader.ReadInt32();
        flagMasks = reader.ReadInt32();
        flagValues = reader.ReadInt32();
        var mask = 1;
        int nValues;
        for (nValues = 0; nValues < 4; nValues++)
        {
            if ((flags & mask) == 0)
                break;
            mask <<= 4;
        }

        values = new Multivar[nValues];
        var maskGlobal = 2;
        var maskDouble = 4;
        int i;
        for (i = 0; i < nValues; i++)
        {
            var value = new Multivar();
            value.Read(reader);
            value.isDouble = (flags & maskDouble) != 0;
            maskGlobal <<= 4;
            maskDouble <<= 4;
            values[i] = value;
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(flags);
        writer.WriteInt32(flagMasks);
        writer.WriteInt32(flagValues);
        foreach (var item in values) item.Write(writer);
    }
}