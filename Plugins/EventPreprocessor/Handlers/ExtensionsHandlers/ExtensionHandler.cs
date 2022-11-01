using System.Collections.Generic;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;

namespace EventPreprocessor.Handlers.ExtensionsHandlers
{
    public class ExtensionHandler
    {
        public Dictionary<int, ConditionHandler> extensionConditionHandlers=new Dictionary<int, ConditionHandler>();
        public Dictionary<int, ActionHandler> extensionActionHandlers=new Dictionary<int, ActionHandler>();

        public virtual string Name { get; }
        public virtual void Init()
        {
            
        }
    }
}