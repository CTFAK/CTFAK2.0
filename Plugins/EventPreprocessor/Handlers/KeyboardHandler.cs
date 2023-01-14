using System.Collections.Generic;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Shared.Events;

namespace EventPreprocessor.Handlers.ExtensionsHandlers;

public class KeyboardHandler
{
    public static Dictionary<int, ActionHandler> keyboardActionHandlers = new();

    public static Dictionary<int, ConditionHandler> keyboardConditionHandlers = new()
    {
        { -1, KeyPressed }
    };

    public static void KeyPressed(Condition condition, ObjectInfo obj, List<Parameter> parameters)
    {
        var param0 = parameters[0].Loader as KeyParameter;
        FTEventViewer.WriteLine($"KeyPressed({KeyConvert.GetKeyText(param0.Key)})");
    }
}