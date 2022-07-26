using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

namespace EventPreprocessor.Handlers.ExtensionsHandlers
{
    public class SystemHandler
    {
        public static Dictionary<int, ActionHandler> systemActionHandlers = new Dictionary<int, ActionHandler>();

        public static Dictionary<int, ConditionHandler> systemConditionHandlers = new Dictionary<int, ConditionHandler>()
        {
            {-20,CompareGlobalString}
        };
        
        public static void CompareGlobalString(Condition condition, ObjectInfo obj, List<Parameter> parameters)
        {
            var param0 = parameters[0].Loader as Short;
            var param1 = parameters[1].Loader as ExpressionParameter;
            FTEventViewer.WriteLine($"GlobalValue{Convert.ToChar(param0.Value+65).ToString().ToUpper()} {param1.GetOperator()} {ExpressionConverter.ConvertExpression(param1)}");

        }
    }
}