using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

namespace EventPreprocessor.Handlers
{
    
    
    public static class ActiveHandler
    {
        public static Dictionary<int, ActionHandler> activeActionHandlers = new Dictionary<int, ActionHandler>();
        public static Dictionary<int, ConditionHandler> activeConditionHandlers = new Dictionary<int, ConditionHandler>()
        {
            {-27,CompareAlterableValue}
        };
        
        public static void CompareAlterableValue(Condition condition,ObjectInfo obj,List<Parameter> parameters)
        {
            var param0 = parameters[0].Loader as AlterableValue;
            var param1 = parameters[1].Loader as ExpressionParameter;
            FTEventViewer.WriteLine($"({obj.name})->AlterableValue{Convert.ToChar(param0.Value+65).ToString().ToUpper()} {param1.GetOperator()} {ExpressionConverter.ConvertExpression(param1)}");
        }
        

    }
}