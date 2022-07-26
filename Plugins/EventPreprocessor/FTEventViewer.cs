using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.FileReaders;
using CTFAK.Tools;
using EventPreprocessor.Handlers;
using EventPreprocessor.Handlers.ExtensionsHandlers;
using Action = CTFAK.CCN.Chunks.Frame.Action;

namespace EventPreprocessor
{
    public class FTEventViewer:IFusionTool
    {
        public string Name => "Event Viewer";

        public static Dictionary<int,ObjectInfo> objects;

        public static StreamWriter streamWriter;

        public Dictionary<int, Dictionary<int, ConditionHandler>> conditionHandlers=new Dictionary<int, Dictionary<int, ConditionHandler>>();
        public Dictionary<int, Dictionary<int, ActionHandler>> actionHandlers=new Dictionary<int, Dictionary<int, ActionHandler>>();
        public void Execute(IFileReader reader)
        {
            STARTUP:
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Event Viewer v1.0");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("by 1987kostya");
            Console.WriteLine("specially for JR's team");
            Console.WriteLine();
            
            var game = reader.getGameData();
            
            
            
            conditionHandlers.Add(-3,AppHandler.appConditionHandlers);
            conditionHandlers.Add(2,ActiveHandler.activeConditionHandlers);
            conditionHandlers.Add(-1,SystemHandler.systemConditionHandlers);
            conditionHandlers.Add(-6,KeyboardHandler.keyboardConditionHandlers);

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(ExtensionHandler)))
                {
                    
                    var newInstance = (ExtensionHandler)Activator.CreateInstance(type);
                    Console.WriteLine("Found extension handlers for extension " + newInstance.Name);
                    foreach (var extension in game.extensions.Items)
                    {
                        if (extension.Name == newInstance.Name)
                        {
                            Console.WriteLine($"Adding handlers for extension {extension.Name} with handle {extension.Handle+32}");
                            newInstance.Init();
                            conditionHandlers.Add(extension.Handle+32,newInstance.extensionConditionHandlers);
                            actionHandlers.Add(extension.Handle+32,newInstance.extensionActionHandlers);
                            
                        }
                    }
                }
            }
            
            
            
            
            
            SELECT_FRAME:
            
            objects = game.frameitems;
            Console.WriteLine("Select frame");
            
            for (int i = 0; i < game.frames.Count; i++)
            {
                var frame = game.frames[i];
                Console.WriteLine($"{i+1}. {frame.name}");
            }
            Console.WriteLine("0. All frames");

            var gotValue = int.TryParse(Console.ReadLine(),out int selectedIndex);
            if(!gotValue)goto SELECT_FRAME;

            if (selectedIndex == 0)
            {
                Console.WriteLine("Selected all frames");
                foreach (var frameToProcess in game.frames)
                {
                    
                    Directory.CreateDirectory($"Dumps\\{game.name}\\Events");
                    streamWriter = new StreamWriter($"Dumps\\{game.name}\\Events\\{frameToProcess.name}.log",false);
                    ProcessFrame(frameToProcess);
                }
            }
            else
            {
                var selectedFrame = game.frames[selectedIndex - 1];
                Console.WriteLine("Selected frame: "+selectedFrame.name);
                Directory.CreateDirectory($"Dumps\\{game.name}\\Events");
                streamWriter = new StreamWriter($"Dumps\\{game.name}\\Events\\{selectedFrame.name}.log",false);
                ProcessFrame(selectedFrame);
            }
            
        }
        
        public static void WriteLine(string str,int indent=2)
        {
            for (int i = 0; i < indent; i++)
            {
                streamWriter.Write(" ");
                Console.Write(" ");
            }
            streamWriter.WriteLine(str);
            streamWriter.Flush();
            Console.WriteLine(str);
        }
        public void ProcessFrame(Frame frame)
        {
            
            foreach (var eventGroup in frame.events.Items)
            {
                ProcessEventGroup(eventGroup);
            }
        }

        public void ProcessEventGroup(EventGroup eventGroup)
        {
            WriteLine("IF:",0);
            WriteLine("{",0);
            foreach (var condition in eventGroup.Conditions)
            {
                ProcessCondition(condition);
            }
            WriteLine("}",0);
            WriteLine("DO:",0);
            WriteLine("{",0);
            foreach (var action in eventGroup.Actions)
            {
                ProcessAction(action);
            }
            WriteLine("}",0);
            
        }

        public void ProcessCondition(Condition condition)
        {
            
            Dictionary<int, ConditionHandler> currentHandlers = new Dictionary<int, ConditionHandler>();
            var conditionObject = objects[condition.ObjectInfo];
            var gotHandlers = conditionHandlers.TryGetValue(condition.ObjectType,out currentHandlers);
            if (!gotHandlers)
            {
                WriteLine($"Object not implemented: {(Constants.ObjectType)condition.ObjectType}({condition.ObjectType})");
                return;
            }


            if (currentHandlers != null)
            {
                ConditionHandler handler;
                var success = currentHandlers.TryGetValue(condition.Num, out handler);
                if (success)
                {
                    handler.Invoke(condition,conditionObject,condition.Items);
                }
                else
                {
                    try
                    {
                        WriteLine(
                            $"UNIMPLEMENTED CONDITION. {(Constants.ObjectType)condition.ObjectType}.{ConditionNames.ConditionSystemDict[condition.ObjectType][condition.Num]}({condition.ObjectType}::{condition.Num})");

                    }
                    catch
                    {
                        WriteLine($"UNKNOWN CONDITION {(Constants.ObjectType)condition.ObjectType}.{condition.Num}");
                    }
                    WriteLine("Params: ");
                    foreach (var param in condition.Items)
                    {
                        WriteLine($"Loader: {param.Loader}, Value: {param.Value}");
                    }
                    //Console.ReadKey();
                }
            }
            else WriteLine("CRITICAL IMPLEMENTATION ERROR");
        }

        public void ProcessAction(Action action)
        {
            return;
            Dictionary<int, ActionHandler> currentHandlers = new Dictionary<int, ActionHandler>();
            var conditionObject = objects[action.ObjectInfo];
            var gotHandlers = actionHandlers.TryGetValue(action.ObjectType,out currentHandlers);
            if (!gotHandlers)
            {
                WriteLine("Object not implemented: "+action.ObjectType);
                return;
            }


            if (currentHandlers != null)
            {
                ActionHandler handler;
                var success = currentHandlers.TryGetValue(action.Num, out handler);
                if (success)
                {
                    handler.Invoke(action,conditionObject,action.Items);
                }
                else
                {
                    WriteLine($"UNIMPLEMENTED ACTION. Num:{action.Num}, ObjectType:{action.ObjectType}");
                    WriteLine("Params: ");
                    foreach (var param in action.Items)
                    {
                        WriteLine($"Loader: {param.Loader}, Value: {param.Value}");
                    }
                    Console.ReadKey();
                }
            }
            else WriteLine("CRITICAL IMPLEMENTATION ERROR");
        }
    }

    public delegate void ActionHandler(Action action,ObjectInfo obj,List<Parameter> parameters);
    public delegate void ConditionHandler(Condition condition,ObjectInfo obj,List<Parameter> parameters);


}

