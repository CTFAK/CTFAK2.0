using System.Collections.Generic;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Shared.Events;

namespace EventPreprocessor.Handlers;

public static class AppHandler
{
    public static Dictionary<int, ActionHandler> appActionHandlers = new();

    public static Dictionary<int, ConditionHandler> appConditionHandlers = new()
    {
        { -1, SimpleCall }
    };

    public static void SimpleCall(Condition condition, ObjectInfo obj, List<Parameter> parameters)
    {
        FTEventViewer.WriteLine($"{ConditionNames.ConditionSystemDict[condition.ObjectType][condition.Num]}");
    }
}