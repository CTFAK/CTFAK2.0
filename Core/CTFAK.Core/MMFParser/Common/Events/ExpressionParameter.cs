using System.Collections.Generic;
using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class ExpressionParameter : ParameterCommon
{
    public short Comparsion;
    public List<Expression> Items = new();

    public override void Read(ByteReader reader)
    {
        Comparsion = reader.ReadInt16();
        while (true)
        {
            var expression = new Expression();
            expression.Read(reader);
            // Logger.Log($"Found expression {expression.ObjectType}-{expression.Num}=={((ExpressionLoader)expression.Loader)?.Value}");
            if (expression.ObjectType == 0 && expression.Num == 0)
                break;
            Items.Add(expression);
            // if(expression.Num==23||expression.Num==24||expression.Num==50||expression.Num==16||expression.Num==19)Logger.Log("CUMSHOT "+expression.Num);
        }
    }

    public string GetOperator()
    {
        switch (Comparsion)
        {
            case 0: return "==";
            case 1: return "!=";
            case 2: return "<=";
            case 3: return "<";
            case 4: return ">=";
            case 5: return ">";
            default: return "err";
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Comparsion);
        // Logger.Log("ExpressionCount: "+Items.Count);
        foreach (var item in Items)
            // Logger.Log("Writing expression: "+item.Num);
            item.Write(writer);
        writer.WriteInt32(0);
    }

    public override string ToString()
    {
        return $"{(Items.Count > 0 ? "==" + Items[0] : " ")}";
    }
}