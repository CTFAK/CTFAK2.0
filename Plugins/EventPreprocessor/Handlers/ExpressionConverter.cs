using System;
using System.Text;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

namespace EventPreprocessor.Handlers
{
    public static class ExpressionConverter
    {
        static string LongConverter(Expression exp)
        {
            return $"{((LongExp)exp.Loader).Value}";
        }
        static string DoubleConverter(Expression exp)
        {
            return $"{((DoubleExp)exp.Loader).Value}";
        }
        static string StringConverter(Expression exp)
        {
            return $"\"{((StringExp)exp.Loader).Value}\"";
        }
        static string FuncCall(Expression exp)
        {
            return $"{ExpressionNames.ExpressionSystemDict[exp.ObjectType][exp.Num]}(";
        }
        static string Punctuation(Expression exp)
        {
            return $"{ExpressionNames.ExpressionSystemDict[exp.ObjectType][exp.Num]}";
        }
        static string GetObjMember(Expression exp)
        {
            return
                $"{FTEventViewer.objects[exp.ObjectInfo]}->{ExpressionNames.ExpressionSystemDict[exp.ObjectType][exp.Num]}";
        }
        static string Timer(Expression exp)
        {
            return "I_DONT_KNOW_WHAT_THIS_DOES_SORRY";
        }

        private static string GetObjAlt(Expression exp)
        {
            return
                $"{FTEventViewer.objects[exp.ObjectInfo]}->{ExpressionNames.ExpressionSystemDict[exp.ObjectType][exp.Num]}_NO_IDEA_WHAT_THIS_DOES";
        }
        static string GetObjFlag(Expression exp)
        {
            return
                $"{FTEventViewer.objects[exp.ObjectInfo]}->{ExpressionNames.ExpressionSystemDict[exp.ObjectType][exp.Num]}[PUT_FLAG_NUMBER_HERE] == 1";
        }
        
        public static string ConvertExpression(ExpressionParameter expressionParameter)
        {
            StringBuilder acc = new StringBuilder();
            foreach (var item in expressionParameter.Items)
            {
                switch (item.ObjectType)
                {
                    case -1:
                        switch (item.Num)
                        {
                            case 0:
                                acc.Append(LongConverter(item));
                                break;
                            case 23: 
                                acc.Append(DoubleConverter(item));
                                break;
                            case 1:
                            case 65:
                                acc.Append(FuncCall(item));
                                break;
                            case 4:
                                acc.Append(StringConverter(item));
                                break;
                            case -1:
                            case -2:
                            case -3:
                                acc.Append(Punctuation(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    case 7:
                        switch (item.Num)
                        {
                            case 80:
                                acc.Append(GetObjMember(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    case -2:
                        switch (item.Num)
                        {
                            case 2:
                                acc.Append(FuncCall(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    case 2:
                        switch (item.Num)
                        {
                            case 16:
                                acc.Append(GetObjAlt(item));
                                break;
                            case -24:
                                acc.Append(GetObjFlag(item));
                                break;
                            case 27:
                                acc.Append(GetObjMember(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    case -4:
                        switch (item.Num)
                        {
                            case -8:
                                acc.Append(Timer(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    case 0:
                        switch (item.Num)
                        {
                            case 0:
                            case 2:
                            case 4:
                            case 6:
                            case 8:
                            case 10:
                            case 12:
                            case 14:
                                acc.Append(Punctuation(item));
                                break;
                            default:
                                FTEventViewer.WriteLine($"UNIMPLEMENTED EXPRESSION NUM. Obj:{item.ObjectType},Num:{item.Num}");
                                Console.ReadKey(); 
                                break;
                        }
                        break;
                    default:
                        FTEventViewer.WriteLine("UNIMPLEMENTED EXPRESSION OBJECT TYPE: "+item.ObjectType);
                        Console.ReadKey();
                        break;

                }
            }



            return acc.ToString();

        }
        
    }
}