using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

namespace EventPreprocessor.Handlers.ExtensionsHandlers
{
    public class KeyboardHandler
    {
        public static Dictionary<int, ActionHandler> keyboardActionHandlers = new Dictionary<int, ActionHandler>();

        public static Dictionary<int, ConditionHandler> keyboardConditionHandlers = new Dictionary<int, ConditionHandler>()
        {
            {-1,KeyPressed}
        };
        
        public static void KeyPressed(Condition condition, ObjectInfo obj, List<Parameter> parameters)
        {
            var param0 = parameters[0].Loader as KeyParameter;
            FTEventViewer.WriteLine($"KeyPressed({KeyConvert.GetKeyText(param0.Key)})");
        }
    }
}