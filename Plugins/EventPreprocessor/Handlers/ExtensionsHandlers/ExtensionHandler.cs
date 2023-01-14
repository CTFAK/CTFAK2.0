using System.Collections.Generic;

namespace EventPreprocessor.Handlers.ExtensionsHandlers;

public class ExtensionHandler
{
    public Dictionary<int, ActionHandler> extensionActionHandlers = new();
    public Dictionary<int, ConditionHandler> extensionConditionHandlers = new();

    public virtual string Name { get; }

    public virtual void Init()
    {
    }
}