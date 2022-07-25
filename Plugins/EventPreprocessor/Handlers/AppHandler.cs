using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

namespace EventPreprocessor.Handlers
{
    public static class AppHandler
    {
        public static Dictionary<int, ActionHandler> appActionHandlers = new Dictionary<int, ActionHandler>();

        public static Dictionary<int, ConditionHandler> appConditionHandlers = new Dictionary<int, ConditionHandler>()
        {
            { -1, SimpleCall },
            { 0, SimpleCall },
            { 2, JumpToFrame },
            { 4, SimpleCall },
            { 19, SimpleCall },
            { 20, SimpleCall },
        };

        public static void SimpleCall(Condition condition,ObjectInfo obj,List<Parameter> parameters)
        {
            FTEventViewer.WriteLine($"{ConditionNames.ConditionSystemDict[condition.ObjectType][condition.Num]}");
        }

        public static void JumpToFrame(Condition condition, ObjectInfo obj, List<Parameter> parameters)
        {
            var param0 = parameters[0].Loader as StringParam;
            FTEventViewer.WriteLine($"{ConditionNames.ConditionSystemDict[condition.ObjectType][condition.Num]}({param0.Value})");
        }

    }
}