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
            {-1,SimpleCall}
        };

        public static void SimpleCall(Condition condition,ObjectInfo obj,List<Parameter> parameters)
        {
            FTEventViewer.WriteLine($"{ConditionNames.ConditionSystemDict[condition.ObjectType][condition.Num]}");
        }



    }
}