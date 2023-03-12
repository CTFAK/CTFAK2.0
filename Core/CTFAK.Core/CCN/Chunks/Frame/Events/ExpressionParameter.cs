using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class ExpressionParameter:ParameterCommon
    {
        public List<Expression> Items = new List<Expression>();
        public short Comparsion;



        public override void Read(ByteReader reader)
        {
            Comparsion = reader.ReadInt16();
            while (true)
            {
                var expression = new Expression();
                expression.Read(reader);
                if (expression.ObjectType == 0&& expression.Num==0) break;
                else Items.Add(expression);
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

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Comparsion);
            foreach (Expression item in Items)
                item.Write(Writer);
            Writer.WriteInt32(0);
        }

        public override string ToString()
        {

            return  $"{(Items.Count > 0 ? "=="+Items[0].ToString() : " ")}";;
        }
    }
}