using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;

namespace EventPreprocessor.Handlers.ExtensionsHandlers
{
    public class GetObject:ExtensionHandler
    {
        public override string Name => "Get";
        public override void Init()
        {
            base.Init();
            extensionConditionHandlers.Add(-83,OnGetTimeout);
            extensionConditionHandlers.Add(-81,OnGetComplete);
        }
        public static void OnGetTimeout(Condition condition, ObjectInfo obj, List<Parameter> parameters)
        {
            FTEventViewer.WriteLine($"({obj.name})->OnGetTimeout");
        }
        public static void OnGetComplete(Condition condition, ObjectInfo obj, List<Parameter> parameters)
        {
            FTEventViewer.WriteLine($"({obj.name})->OnGetComplete");
        }
        
    }
}