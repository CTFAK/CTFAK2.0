using CTFAK.CCN;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using Microsoft.VisualBasic;
using Action = CTFAK.CCN.Chunks.Frame.Action;
using Constants = CTFAK.CCN.Constants;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using CTFAK.Core.Properties;
using CTFAK.Core.CCN.Chunks.Banks;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using static System.Collections.Specialized.BitVector32;
using CTFAK.Core.CCN.Chunks.Banks.ImageBank;
using CTFAK.Core.MFA.MFAObjectLoaders;

namespace CTFAK.Tools
{
    class FTDecompile : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Export as MFA";
        public static int lastAllocatedHandleImg = 15;

        public static Dictionary<int, MFAObjectInfo> FrameItems;
        public void Execute(IFileReader reader)
        {
            var game = reader.getGameData();
            var mfa = new MFAData();
            Settings.GameType originalGameType = Settings.gameType;
            int orginalBuild = game.productBuild;
            Settings.gameType = Settings.GameType.NORMAL;
            Settings.isMFA = true;
            Dictionary<int, FusionImage> imgs = game.Images.Items;

            if (CTFAKCore.parameters.Contains("-noimg"))
                game.Images.Items.Clear();

            mfa.Read(new ByteReader("template.mfa", FileMode.Open));

            Settings.gameType = originalGameType;
            Settings.isMFA = false;

            mfa.Name = game.name;
            mfa.LangId = 0;//8192;
            mfa.Description = "";
            mfa.Path = game.editorFilename;
            //mfa.Menu = game.menu;

            //if (game.Fonts != null) mfa.Fonts = game.Fonts;
            // mfa.Sounds.Items.Clear();
            if (game.Sounds != null && game.Sounds.Items != null)
            {
                foreach (var item in game.Sounds.Items)
                {
                    mfa.Sounds.Items.Add(item);
                }
                if (CTFAKCore.parameters.Contains("-nosounds"))
                    mfa.Sounds.Items.Clear();
            }
            mfa.Fonts.Items.Clear();
            if (game.Fonts?.Items != null)
            {
                foreach (var item in game.Fonts.Items)
                {
                    item.Compressed = false;
                    mfa.Fonts.Items.Add(item);
                }
            }

            if (game.Music != null)
                mfa.Music = game.Music; 
            mfa.Images.Items = imgs;
            mfa.GraphicMode = 4;

            foreach (var item in mfa.Icons.Items)
            {
                try
                {
                    switch (item.Key)
                    {
                        case 2:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon);
                            else
                                item.Value.FromBitmap(reader.getIcons()[16]);
                            break;
                        case 5:
                        case 8:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon);
                            else
                                item.Value.FromBitmap(reader.getIcons()[17]);
                            break;
                        case 1:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(32));
                            else
                                item.Value.FromBitmap(reader.getIcons()[32]);
                            break;
                        case 4:
                        case 7:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(32));
                            else
                                item.Value.FromBitmap(reader.getIcons()[33]);
                            break;
                        case 0:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(48));
                            else
                                item.Value.FromBitmap(reader.getIcons()[48]);
                            break;
                        case 3:
                        case 6:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(48));
                            else
                                item.Value.FromBitmap(reader.getIcons()[49]);
                            break;
                        case 9:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(128));
                            else
                                item.Value.FromBitmap(reader.getIcons()[128]);
                            break;
                        case 10:
                            if (reader.getIcons().Count == 0 && game.Icon != null)
                                item.Value.FromBitmap(game.Icon.ResizeImage(256));
                            else
                                item.Value.FromBitmap(reader.getIcons()[256]);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    Logger.LogWarning($"Requested icon is not found: {item.Key} - {item.Value.Width}");
                }
            }
            var imageNull = new FusionImage();
            imageNull.Handle = 14;
            imageNull.Transparent = Color.Brown;
            imageNull.FromBitmap((Bitmap)Resources.EmptyIcon);
            mfa.Icons.Items.Add(14, imageNull);
            // game.Images.Images.Clear();

            mfa.Author = game.author;
            mfa.Copyright = game.copyright;
            mfa.Company = "";
            mfa.Version = "";
            mfa.binaryFiles = game.binaryFiles;
            mfa.Extensions.Clear();

            mfa.DisplayFlags.flag = 0;
            mfa.DisplayFlags["MaximizedOnBoot"] = game.header.Flags["MaximizedOnBoot"];
            mfa.DisplayFlags["ResizeDisplay"] = game.header.Flags["ResizeDisplay"];
            mfa.DisplayFlags["FullscreenAtStart"] = game.header.Flags["MaximizedOnBoot"];
            mfa.DisplayFlags["AllowFullscreen"] = game.header.Flags["SwitchToFromFullscreen"];
            mfa.DisplayFlags["Heading"] = !game.header.Flags["NoHeading"];
            mfa.DisplayFlags["HeadingWhenMaximized"] = game.header.Flags["HeadingMaximized"];
            mfa.DisplayFlags["MenuBar"] = game.header.Flags["MenuBar"];
            mfa.DisplayFlags["MenuOnBoot"] = !game.header.Flags["DontDisplayMenu"];
            mfa.DisplayFlags["NoMinimize"] = game.header.NewFlags["NoMinimizeBox"];
            mfa.DisplayFlags["NoMaximize"] = game.header.NewFlags["NoMaximizeBox"];
            mfa.DisplayFlags["NoThickFrame"] = game.header.NewFlags["NoThickFrame"];
            mfa.DisplayFlags["NoCenter"] = game.header.NewFlags["DoNotCenterFrame"];
            mfa.DisplayFlags["DisableClose"] = game.header.NewFlags["DisableClose"];
            mfa.DisplayFlags["HiddenAtStart"] = game.header.NewFlags["HiddenAtStart"];
            mfa.DisplayFlags["MDI"] = game.header.NewFlags["MDI"];

            mfa.GraphicFlags.flag = 0;
            mfa.GraphicFlags["MultiSamples"] = game.header.Flags["MultiSamples"];
            mfa.GraphicFlags["MachineIndependentSpeed"] = game.header.Flags["MachineIndependentSpeed"];
            mfa.GraphicFlags["SamplesOverFrames"] = game.header.NewFlags["SamplesOverFrames"];
            mfa.GraphicFlags["PlaySamplesWhenUnfocused"] = game.header.NewFlags["PlaySamplesWhenUnfocused"];
            mfa.GraphicFlags["IgnoreInputOnScreensaver"] = game.header.NewFlags["IgnoreInputOnScreensaver"];
            mfa.GraphicFlags["VisualThemes"] = game.header.NewFlags["VisualThemes"];
            mfa.GraphicFlags["VSync"] = game.header.NewFlags["VSync"];
            mfa.GraphicFlags["RunWhenMinimized"] = game.header.NewFlags["RunWhenMinimized"];
            mfa.GraphicFlags["RunWhenResizing"] = game.header.NewFlags["RunWhileResizing"];
            mfa.GraphicFlags["EnableDebuggerShortcuts"] = game.header.OtherFlags["DebuggerShortcuts"];
            mfa.GraphicFlags["NoDebugger"] = !game.header.OtherFlags["ShowDebugger"];
            mfa.GraphicFlags["NoSubappSharing"] = game.header.OtherFlags["DontShareSubData"];
            mfa.GraphicFlags["Direct3D9"] = game.header.OtherFlags["Direct3D9or11"] && !game.header.OtherFlags["Direct3D8or11"];
            mfa.GraphicFlags["Direct3D8"] = game.header.OtherFlags["Direct3D8or11"] && !game.header.OtherFlags["Direct3D9or11"];
            mfa.GraphicFlags["DisableIME"] = game.ExtHeader.Flags["DisableIME"];
            mfa.GraphicFlags["ReduceCPUUsage"] = game.ExtHeader.Flags["ReduceCPUUsage"];
            mfa.GraphicFlags["Direct3D11"] = game.header.OtherFlags["Direct3D8or11"] && game.header.OtherFlags["Direct3D9or11"];
            mfa.GraphicFlags["PremultipliedAlpha"] = game.ExtHeader.Flags["PremultipliedAlpha"];

            try
            {
                foreach (var globalValue in game.globalValues.Items)
                {
                    mfa.GlobalValues.Items.Add(new ValueItem()
                    {
                        Value = globalValue,
                    });
                }

                foreach (var globalString in game.globalStrings.Items)
                {
                    mfa.GlobalStrings.Items.Add(new ValueItem()
                    {
                        Value = globalString,
                    });
                }
            }
            catch { }
            mfa.WindowX = game.header.WindowWidth;
            mfa.WindowY = game.header.WindowHeight;
            mfa.BorderColor = game.header.BorderColor;
            mfa.HelpFile = "";
            mfa.InitialScore = game.header.InitialScore;
            mfa.InitialLifes = game.header.InitialLives;
            mfa.FrameRate = game.header.FrameRate;
            mfa.BuildType = 0;
            mfa.BuildPath = game.targetFilename;
            mfa.CommandLine = "";
            mfa.Aboutbox = game.aboutText ?? "Decompiled with CTFAK 2.0";
            //TODO: Controls

            //Object Section
            FrameItems = new Dictionary<int, MFAObjectInfo>();
            //Logger.Log("Frame Items: " + game.frameitems.Count);
            for (int i = 0; i < game.frameitems.Keys.Count; i++)
            {
                var key = game.frameitems.Keys.ToArray()[i];
                var item = game.frameitems[key];
                var newItem = new MFAObjectInfo();
                if (item.ObjectType >= 32)
                {
                    newItem = TranslateObject(mfa, game, item, true);
                }
                else
                {
                    newItem = TranslateObject(mfa, game, item, false);
                }
                if (newItem.Loader == null)
                {
                    Logger.LogWarning("NOT IMPLEMENTED OBJECT: " + newItem.ObjectType);
                    continue;
                }
                FrameItems.Add(newItem.Handle, newItem);
            }

            // var reference = mfa.Frames.FirstOrDefault();
            mfa.Frames.Clear();

            Dictionary<int, int> indexHandles = new Dictionary<int, int>();
            if (game.frameHandles != null)
            {
                foreach (var pair in game.frameHandles.Items)
                {
                    var key = pair.Key;
                    var handle = pair.Value;
                    if (!indexHandles.ContainsKey(handle)) indexHandles.Add(handle, key);
                    else indexHandles[handle] = key;
                }
            }

            Logger.Log($"Preparing to translate {game.frames.Count} frames");
            for (int a = 0; a < game.frames.Count; a++)
            {

                var frame = game.frames[a];

                if (frame.flags["DontInclude"]) continue;
                //if(frame.Palette==null|| frame.Events==null|| frame.Objects==null) continue;
                var newFrame = new MFAFrame();
                newFrame.Chunks = new MFAChunks();//MFA.MFA.emptyFrameChunks;
                newFrame.Handle = a;
                if (!indexHandles.TryGetValue(a, out newFrame.Handle)) Logger.Log("Error while getting frame handle");

                newFrame.Name = frame.name;
                newFrame.SizeX = frame.width;
                newFrame.SizeY = frame.height;

                newFrame.Background = frame.background;
                newFrame.FadeIn = frame.fadeIn != null ? ConvertTransition(frame.fadeIn) : null;
                newFrame.FadeOut = frame.fadeOut != null ? ConvertTransition(frame.fadeOut) : null;

                newFrame.Flags.flag = 0;
                newFrame.Flags["GrabDesktop"] = frame.flags["GrabDesktop"];
                newFrame.Flags["KeepDisplay"] = frame.flags["KeepDisplay"];
                newFrame.Flags["DisplayFrameTitle"] = frame.flags["DisplayTitle"];
                newFrame.Flags["BackgroundCollisions"] = frame.flags["HandleCollision"];
                newFrame.Flags["ResizeToScreen"] = frame.flags["ResizeAtStart"];
                newFrame.Flags["TimerBasedMovements"] = frame.flags["TimeMovements"];
                newFrame.Flags["DontEraseBG"] = frame.flags["DontEraseBG"];

                newFrame.MaxObjects = frame.events?.MaxObjects ?? 10000;
                newFrame.Password = "";
                newFrame.LastViewedX = 320;
                newFrame.LastViewedY = 240;
                //if (frame.palette == null) continue;
                newFrame.Palette = frame.palette ?? new List<Color>();
                newFrame.StampHandle = 13;
                newFrame.ActiveLayer = 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Left = frame.virtualRect?.left ?? 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Top = frame.virtualRect?.top ?? 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Right = frame.virtualRect?.right ?? frame.width;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Bottom = frame.virtualRect?.bottom ?? frame.height;
                newFrame.Chunks.GetOrCreateChunk<FrameMovementTimer>().Timer = frame.movementTimer;
                var lyrShdrData = new LayerShaderSettings();
                var shdrData = new FrameShaderSettings();

                //LayerInfo
                if (Settings.Old)
                {
                    var tempLayer = new MFALayer();

                    tempLayer.Name = "Layer 1";
                    tempLayer.XCoefficient = 1;
                    tempLayer.YCoefficient = 1;
                    tempLayer.Flags["Visible"] = true;
                    newFrame.Layers.Add(tempLayer);
                }
                else
                {
                    var count = frame.layers.Items.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var layer = frame.layers.Items[i];
                        var newLayer = new MFALayer();
                        newLayer.Name = layer.Name;
                        newLayer.Flags["HideAtStart"] = layer.Flags["ToHide"];
                        newLayer.Flags["Visible"] = true;
                        newLayer.Flags["NoBackground"] = layer.Flags["DoNotSaveBackground"];
                        newLayer.Flags["WrapHorizontally"] = layer.Flags["WrapHorizontally"];
                        newLayer.XCoefficient = layer.XCoeff;
                        newLayer.YCoefficient = layer.YCoeff;
                        LayerShaderSettings.MFALayerShader lyrShdr = new();

                        lyrShdr.InkInMyBum = layer.Effect;
                        if (!game.header.OtherFlags["Direct3D8or11"] && !game.header.OtherFlags["Direct3D9or11"])
                            lyrShdr.RGBCoeff = Color.FromArgb(layer.RGBCoeff.A, 255 - layer.RGBCoeff.R, 255 - layer.RGBCoeff.G, 255 - layer.RGBCoeff.B);
                        else
                            lyrShdr.RGBCoeff = layer.RGBCoeff;

                        if (layer.shaderData.hasShader)
                        {
                            var newShader = new LayerShaderSettings.MFAShader();
                            newShader.Name = layer.shaderData.name;
                            //Logger.Log($"Found shader '{newShader.Name}' on layer '{newLayer.Name}'");
                            foreach (var param in layer.shaderData.parameters)
                            {
                                var newParam = new LayerShaderSettings.ShaderParameter();
                                newParam.Name = param.Name;
                                newParam.Value = param.Value;
                                newParam.ValueType = param.ValueType;
                                newShader.Parameters.Add(newParam);
                            }
                            lyrShdr.Shaders.Add(newShader);
                        }

                        lyrShdrData.LayerShaders.Add(lyrShdr);
                        newFrame.Layers.Add(newLayer);
                    }
                }

                shdrData.Shaders = new();
                shdrData.Effect = frame.Effect;
                if (!game.header.OtherFlags["Direct3D8or11"] && !game.header.OtherFlags["Direct3D9or11"])
                    shdrData.RGBCoeff = Color.FromArgb(frame.RGBCoeff.A, 255 - frame.RGBCoeff.R, 255 - frame.RGBCoeff.G, 255 - frame.RGBCoeff.B);
                else
                    shdrData.RGBCoeff = Color.FromArgb(frame.RGBCoeff.A, frame.RGBCoeff.R, frame.RGBCoeff.G, frame.RGBCoeff.B);

                if (frame.shaderData.hasShader)
                {
                    var newShader = new FrameShaderSettings.MFAShader();
                    newShader.Name = frame.shaderData.name;
                    foreach (var param in frame.shaderData.parameters)
                    {
                        var newParam = new FrameShaderSettings.ShaderParameter();
                        newParam.Name = param.Name;
                        newParam.Value = param.Value;
                        newParam.ValueType = param.ValueType;
                        newShader.Parameters.Add(newParam);
                    }
                    shdrData.Shaders.Add(newShader);
                }

                if (!CTFAKCore.parameters.Contains("-noshaders"))
                {
                    if (!CTFAKCore.parameters.Contains("-nolayershaders"))
                        newFrame.Chunks.GetOrCreateChunk<LayerShaderSettings>().LayerShaders = lyrShdrData.LayerShaders;
                    if (!CTFAKCore.parameters.Contains("-noframeshaders"))
                    {
                        newFrame.Chunks.GetOrCreateChunk<FrameShaderSettings>().Effect = shdrData.Effect;
                        newFrame.Chunks.GetOrCreateChunk<FrameShaderSettings>().RGBCoeff = shdrData.RGBCoeff;
                        newFrame.Chunks.GetOrCreateChunk<FrameShaderSettings>().Shaders = shdrData.Shaders;
                    }
                }

                var newFrameItems = new List<MFAObjectInfo>();
                var newInstances = new List<MFAObjectInstance>();
                if (frame.objects != null)
                //if (false)
                {
                    for (int i = 0; i < frame.objects.Count; i++)
                    {
                        var instance = frame.objects[i];
                        MFAObjectInfo frameItem;

                        if (FrameItems.ContainsKey(instance.objectInfo))
                        {
                            frameItem = FrameItems[instance.objectInfo];
                            if (!newFrameItems.Contains(frameItem)) newFrameItems.Add(frameItem);
                            var newInstance = new MFAObjectInstance();
                            newInstance.X = instance.x;
                            newInstance.Y = instance.y;
                            newInstance.Handle = i;//instance.handle;
                            if (instance.parentType != 0) newInstance.Flags = 8;
                            else newInstance.Flags = 0;
                            // newInstance.Flags = ((instance.FrameItem.Properties.Loader as ObjectCommon)?.Preferences?.flag ?? (uint)instance.FrameItem.Flags);
                            newInstance.Instance = instance.instance;

                            newInstance.ParentType = (uint)instance.parentType;
                            newInstance.ItemHandle = (uint)(instance.objectInfo);
                            newInstance.ParentHandle = (uint)instance.parentHandle;
                            newInstance.Layer = (uint)(instance.layer);
                            newInstances.Add(newInstance);
                        }
                        else
                        {
                            Logger.Log($"Warning: Object not found ({instance.objectInfo})");
                            continue;
                        }
                    }
                }

                newFrame.Items = newFrameItems;
                newFrame.Instances = newInstances;
                newFrame.Folders = new List<MFAItemFolder>();
                foreach (MFAObjectInfo newFrameItem in newFrame.Items)
                {
                    var newFolder = new MFAItemFolder();
                    newFolder.isRetard = true;
                    newFolder.Items = new List<uint>() { (uint)newFrameItem.Handle };
                    newFrame.Folders.Add(newFolder);
                }
                //if(false)
                {
                    newFrame.Events = new MFAEvents();
                    newFrame.Events.Items = new List<EventGroup>();
                    newFrame.Events.Objects = new List<EventObject>();
                    newFrame.Events._ifMFA = true;
                    newFrame.Events.Version = 1028;
                    //if(false)
                    if (frame.events != null)
                    {
                        if (!CTFAKCore.parameters.Contains("-noevnt"))
                        {
                            foreach (var item in newFrame.Items)
                            {
                                var newObject = new EventObject();

                                newObject.Handle = (uint)item.Handle;
                                newObject.Name = item.Name ?? "";
                                newObject.TypeName = "";
                                newObject.ItemType = (ushort)item.ObjectType;
                                newObject.ObjectType = 1;
                                newObject.Flags = 0;
                                newObject.ItemHandle = (uint)item.Handle;
                                newObject.InstanceHandle = 0xFFFFFFFF;
                                newFrame.Events.Objects.Add(newObject);
                            }

                            newFrame.Events.Items = frame.events.Items;

                            Dictionary<int, Quailifer> qualifiers = new Dictionary<int, Quailifer>();
                            foreach (Quailifer qualifer in frame.events.QualifiersList)
                            {
                                int newHandle = 0;
                                while (true)
                                {
                                    if (!newFrame.Items.Any(item => item.Handle == newHandle) &&
                                        !qualifiers.Keys.Any(item => item == newHandle)) break;
                                    newHandle++;
                                }
                                qualifiers.Add(newHandle, qualifer);
                                var qualItem = new EventObject();
                                qualItem.Handle = (uint)newHandle;
                                qualItem.SystemQualifier = (ushort)qualifer.Qualifier;
                                qualItem.Name = "";
                                qualItem.TypeName = "";
                                qualItem.ItemType = (ushort)qualifer.Type;
                                qualItem.ObjectType = 3;
                                newFrame.Events.Objects.Add(qualItem);
                            }
                            for (int eg = 0; eg < newFrame.Events.Items.Count; eg++)//foreach (EventGroup eventGroup in newFrame.Events.Items)
                            {
                                var eventGroup = newFrame.Events.Items[eg];
                                foreach (Action action in eventGroup.Actions)
                                {
                                    if (action.ObjectType == -5 && action.Num == 0)
                                        continue;
                                    foreach (var qualifer in qualifiers)
                                    {
                                        if (qualifer.Value.ObjectInfo == action.ObjectInfo &&
                                            qualifer.Value.Type == action.ObjectType)
                                        {
                                            action.ObjectInfo = qualifer.Key;
                                            action.ObjectType = qualifer.Value.Type;
                                        }
                                        foreach (var param in action.Items)
                                        {
                                            if (param.Loader is ExpressionParameter expr)
                                            {
                                                foreach (var actualExpr in expr.Items)
                                                {
                                                    if (qualifer.Value.ObjectInfo == actualExpr.ObjectInfo)
                                                    {
                                                        actualExpr.ObjectInfo = qualifer.Key;
                                                        actualExpr.ObjectType = qualifer.Value.Type;
                                                    }
                                                }
                                            }
                                            else if (param.Loader is ParamObject obj)
                                            {
                                                if (qualifer.Value.ObjectInfo == obj.ObjectInfo)
                                                {
                                                    obj.ObjectInfo = qualifer.Key;
                                                    obj.ObjectType = qualifer.Value.Type;
                                                }
                                            }
                                            else if (param.Loader is Position pos)
                                            {
                                                if (qualifer.Value.ObjectInfo == pos.ObjectInfoParent)
                                                {
                                                    pos.ObjectInfoParent = (uint)qualifer.Key;
                                                }
                                            }
                                        }
                                    }
                                }
                                foreach (Condition cond in eventGroup.Conditions)
                                {
                                    foreach (var qualifer in qualifiers)
                                    {
                                        if (qualifer.Value.ObjectInfo == cond.ObjectInfo &&
                                            qualifer.Value.Type == cond.ObjectType)
                                        {
                                            cond.ObjectInfo = qualifer.Key;
                                            cond.ObjectType = qualifer.Value.Type;
                                        }
                                        foreach (var param in cond.Items)
                                        {
                                            if (param.Loader is ExpressionParameter expr)
                                            {
                                                foreach (var actualExpr in expr.Items)
                                                {
                                                    if (qualifer.Value.ObjectInfo == actualExpr.ObjectInfo)
                                                    {
                                                        actualExpr.ObjectInfo = qualifer.Key;
                                                        actualExpr.ObjectType = qualifer.Value.Type;
                                                    }
                                                }
                                            }
                                            else if (param.Loader is ParamObject obj)
                                            {
                                                if (qualifer.Value.ObjectInfo == obj.ObjectInfo)
                                                {
                                                    obj.ObjectInfo = qualifer.Key;
                                                    obj.ObjectType = qualifer.Value.Type;
                                                }
                                            }
                                            else if (param.Loader is Position pos)
                                            {
                                                if (qualifer.Value.ObjectInfo == pos.ObjectInfoParent)
                                                {
                                                    pos.ObjectInfoParent = (uint)qualifer.Key;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Logger.Log($"Translating frame {frame.name} - {a}");
                        mfa.Frames.Add(newFrame);
                    }
                }
            }
            Settings.gameType = Settings.GameType.NORMAL;

            var outPath = reader.getGameData().name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            Directory.CreateDirectory($"Dumps\\{outPath}");
            mfa.Write(new ByteWriter(new FileStream($"Dumps\\{outPath}\\{Path.GetFileNameWithoutExtension(game.editorFilename)}.mfa", FileMode.Create)));
            Settings.gameType = originalGameType;
            game.productBuild = orginalBuild;

            static MFATransition ConvertTransition(Transition gameTrans)
            {
                var newName = "";
                newName = gameTrans.Name;
                newName = newName.ToLower();
                var mfaTrans = new MFATransition()
                {
                    Module = "cctrans.dll",//gameTrans.ModuleFile,
                    Name = newName,
                    Id = gameTrans.Module,
                    TransitionId = gameTrans.Name,
                    Flags = gameTrans.Flags,
                    Color = gameTrans.Color,
                    ParameterData = gameTrans.ParameterData,
                    Duration = gameTrans.Duration
                };
                return mfaTrans;
            }

            static MFAObjectInfo TranslateObject(MFAData mfa, GameData game, ObjectInfo item, bool exyt)
            {
                var newItem = new MFAObjectInfo();
                newItem.Chunks = new MFAChunks();
                newItem.Name = item.name;
                newItem.ObjectType = (int)item.ObjectType;
                newItem.Handle = item.handle;
                newItem.Transparent = 1;
                newItem.InkEffect = item.InkEffect;
                newItem.InkEffectParameter = (uint)item.InkEffectValue;
                newItem.AntiAliasing = 0;
                newItem.Flags = item.Flags;
                int type = 2;
                bool noicon = false;
                Bitmap iconBmp = null;
                if (newItem.ObjectType >= 32)
                {
                    CTFAK.CCN.Chunks.Extension ext = null;

                    foreach (var testExt in game.extensions.Items)
                    {
                        if (testExt.Handle == (int)item.ObjectType - 32) ext = testExt;
                    }
                    switch (ext.Name)
                    {
                        case "KcBoxA":
                            iconBmp = Resources.ActiveSystemBox;
                            break;
                        case "kcpop":
                            iconBmp = Resources.PopupMessageobject2;
                            break;
                        case "EasyScrollbar":
                            iconBmp = Resources.EasyScrollbar;
                            break;
                        case "InternalList":
                            iconBmp = Resources.InternalListObject;
                            break;
                        case "PopupMenu":
                            iconBmp = Resources.PopupMenu;
                            break;
                        case "RunInConsole":
                            iconBmp = Resources.ExecuteInConsole;
                            break;
                        case "KcBoxB":
                            iconBmp = Resources.ComboBox;
                            break;
                        case "TreeControl":
                            iconBmp = Resources.TreeControl;
                            break;
                        case "kcinput":
                            iconBmp = Resources.InputObject;
                            break;
                        case "kcedit":
                            iconBmp = Resources.EditBoxSel;
                            break;
                        case "kcriched":
                            iconBmp = Resources.EEEditbox;
                            break;
                        case "fontembed":
                            iconBmp = Resources.FontEmbedObject;
                            break;
                        case "kcfile":
                            iconBmp = Resources.File;
                            break;
                        case "fcFolder":
                            iconBmp = Resources.FileFolderObject;
                            break;
                        case "FileReadWrite":
                            //by default
                            break;
                        case "kcpica":
                            iconBmp = Resources.Active_Picture;
                            break;
                        case "kclist":
                            iconBmp = Resources.List;
                            break;
                        case "kccombo":
                            iconBmp = Resources.ComboBox;
                            break;
                        case "EditBoxSel":
                            iconBmp = Resources.EditBoxSel;
                            break;
                        case "JSON_Object":
                            //by default
                            break;
                        case "CalcRect":
                            iconBmp = Resources.CalcRect;
                            break;
                        case "IIF":
                            iconBmp = Resources.IIF;
                            break;
                        case "StringReplace":
                            iconBmp = Resources.StringReplace;
                            break;
                        case "ObjResize":
                            iconBmp = Resources.ObjResize;
                            break;
                        case "xlua":
                            iconBmp = Resources.XLua;
                            break;
                        case "kcini":
                            iconBmp = Resources.Ini;
                            break;
                        case "INI++15":
                            iconBmp = Resources.IniPLUS;
                            break;
                        case "kcwctrl":
                            iconBmp = Resources.WindowControl;
                            break;
                        case "KcButton":
                            iconBmp = Resources.Button;
                            break;
                        case "Perspective":
                            iconBmp = Resources.Perspective;
                            break;
                        case "kcclock":
                            iconBmp = Resources.DateAndTime;
                            break;
                        default:
                            noicon = true;
                            Logger.Log($"No icon found for {ext.Name}");
                            //System.Threading.Thread.Sleep(500);
                            break;
                    }
                }
                else
                {
                    switch (item.ObjectType)
                    {
                        case 0: //Quick Backdrop
                            try
                            {
                                Bitmap bmp = game.Images.Items[((Quickbackdrop)item.properties).Image].bitmap;
                                if (bmp.Width > bmp.Height)
                                    iconBmp = bmp.ResizeImage(new Size(32, (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                                else
                                    iconBmp = bmp.ResizeImage(new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                            }
                            catch
                            {
                                iconBmp = Resources.Backdrop;
                            }
                            break;
                        case 1: //Backdrop
                            try
                            {
                                Bitmap bmp = game.Images.Items[((Backdrop)item.properties).Image].bitmap;
                                if (bmp.Width > bmp.Height)
                                    iconBmp = bmp.ResizeImage(new Size(32, (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                                else
                                    iconBmp = bmp.ResizeImage(new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                            }
                            catch
                            {
                                iconBmp = Resources.Backdrop;
                            }
                            break;
                        case 2: //Active
                            try
                            {
                                Bitmap bmp = game.Images.Items[((ObjectCommon)item.properties).Animations.AnimationDict.First().Value.DirectionDict.First().Value.Frames.First()].bitmap;
                                if (bmp.Width > bmp.Height)
                                    iconBmp = bmp.ResizeImage(new Size(32, (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                                else
                                    iconBmp = bmp.ResizeImage(new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                            }
                            catch
                            {
                                iconBmp = Resources.Active;
                            }
                            break;
                        case 3: //String
                            iconBmp = Resources.String;
                            break;
                        case 4: //Question and Answer
                            iconBmp = Resources.QandA;
                            break;
                        case 5: //Score
                            iconBmp = Resources.Score;
                            break;
                        case 6: //Lives
                            iconBmp = Resources.Lives;
                            break;
                        case 7: //Counter
                            try
                            {
                                Bitmap bmp = game.Images.Items[((ObjectCommon)item.properties).Counters.Frames.First()].bitmap;
                                if (bmp.Width > bmp.Height)
                                    iconBmp = bmp.ResizeImage(new Size(32, (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                                else
                                    iconBmp = bmp.ResizeImage(new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                            }
                            catch
                            {
                                iconBmp = Resources.Counter;
                            }
                            break;
                        case 8: //Formatted Text
                            iconBmp = Resources.Formatted_Text;
                            break;
                        case 9: //Sub-Application
                            iconBmp = Resources.SubApp;
                            break;
                        default:
                            noicon = true;
                            break;
                    }
                }
                //Logger.Log($"Generating Icon: {item.name} - {item.ObjectType}");
                if (CTFAKCore.parameters.Contains("-noicons"))
                {
                    noicon = false;
                    iconBmp = Resources.Active;
                }

                if (!noicon)
                {
                    FTDecompile.lastAllocatedHandleImg++;
                    var newIconImage = new FusionImage();
                    newIconImage.Handle = lastAllocatedHandleImg;
                    newIconImage.FromBitmap(iconBmp);
                    mfa.Icons.Items.Add(lastAllocatedHandleImg, newIconImage);
                }

                newItem.IconHandle = noicon ? 14 : lastAllocatedHandleImg;

                if (!CTFAKCore.parameters.Contains("-noobjectshaders") && !CTFAKCore.parameters.Contains("-noshaders"))
                {
                    var shdrData = newItem.Chunks.GetOrCreateChunk<ObjectShaderSettings>();
                    if (item.InkEffect != 1 && !CTFAKCore.parameters.Contains("-notrans"))
                        shdrData.Blend = item.blend;
                    shdrData.RGBCoeff = Color.FromArgb(item.rgbCoeff.A, item.rgbCoeff.R, item.rgbCoeff.G, item.rgbCoeff.B);

                    try
                    {
                        //if (ImageBank.realGraphicMode < 4 && Settings.Build < 289 && !Settings.Android && CTFAKCore.parameters.Contains("-badblend"))
                        if (!game.header.OtherFlags["Direct3D8or11"] && !game.header.OtherFlags["Direct3D9or11"])
                        {
                            shdrData.Blend = (byte)(255 - item.blend);
                            shdrData.RGBCoeff = Color.FromArgb(item.rgbCoeff.A, 255 - item.rgbCoeff.R, 255 - item.rgbCoeff.G, 255 - item.rgbCoeff.B);
                        }
                    }
                    catch { }

                    if (item.shaderData.hasShader)
                    {
                        var newShader = new ObjectShaderSettings.MFAShader();
                        newShader.Name = item.shaderData.name;
                        //Logger.Log("Writing shader " + newShader.Name + " on object " + item.name);
                        foreach (var param in item.shaderData.parameters)
                        {
                            var newParam = new ObjectShaderSettings.ShaderParameter();
                            newParam.Name = param.Name;
                            newParam.Value = param.Value;
                            newParam.ValueType = param.ValueType;
                            newShader.Parameters.Add(newParam);
                        }
                        shdrData.Shaders.Add(newShader);
                    }
                }

                if (item.ObjectType == (int)Constants.ObjectType.QuickBackdrop)
                {
                    var backdropLoader = item.properties as Quickbackdrop;
                    var backdrop = new MFAQuickBackdrop();
                    backdrop.ObstacleType = (uint)backdropLoader.ObstacleType;
                    backdrop.CollisionType = (uint)backdropLoader.CollisionType;
                    backdrop.Width = backdropLoader.Width;
                    backdrop.Height = backdropLoader.Height;
                    backdrop.Shape = backdropLoader.Shape.ShapeType;
                    backdrop.BorderSize = backdropLoader.Shape.BorderSize;
                    backdrop.FillType = backdropLoader.Shape.FillType;
                    backdrop.Color1 = backdropLoader.Shape.Color1;
                    backdrop.Color2 = backdropLoader.Shape.Color2;
                    backdrop.Flags = backdropLoader.Shape.GradFlags;
                    if (!CTFAKCore.parameters.Contains("-noimg"))
                        backdrop.Image = backdropLoader.Shape.Image;
                    newItem.Loader = backdrop;
                }
                else if (item.ObjectType == (int)Constants.ObjectType.Backdrop)
                {
                    var backdropLoader = item.properties as Backdrop;
                    var backdrop = new MFABackdrop();
                    backdrop.ObstacleType = (uint)backdropLoader.ObstacleType;
                    backdrop.CollisionType = (uint)backdropLoader.CollisionType;
                    if (!CTFAKCore.parameters.Contains("-noimg"))
                        backdrop.Handle = backdropLoader.Image;
                    newItem.Loader = backdrop;
                }
                else
                {
                    var itemLoader = item?.properties as ObjectCommon;
                    if (itemLoader == null) throw new NotImplementedException("Null loader");
                    //CommonSection
                    var newObject = new ObjectLoader();
                    newObject.ObjectFlags = (int)(itemLoader.Flags.flag);
                    newObject.NewObjectFlags = (int)(itemLoader.NewFlags.flag);
                    newObject.BackgroundColor = itemLoader.BackColor;
                    newObject.Qualifiers = itemLoader._qualifiers;

                    newObject.Strings = new MFAValueList();//ConvertStrings(itemLoader.);
                    newObject.Values = new MFAValueList();//ConvertValue(itemLoader.Values
                    newObject.Movements = new MFAMovements();
                    newItem.FlagWriter = new MFAObjectFlags();

                    if (itemLoader.Values != null)
                    {
                        for (int j = 0; j < itemLoader.Values.Items.Count; j++)
                        {
                            var newVal = new ValueItem();
                            newVal.Value = itemLoader.Values.Items[j];
                            newObject.Values.Items.Add(newVal);
                        }
                        for (int j = 0; j < 32; j++)
                        {
                            var newFlag = new ObjectFlag();
                            newFlag.Value = ByteFlag.GetFlag((uint)itemLoader.Values.Flags, j);
                            newItem.FlagWriter.Items.Add(newFlag);
                        }
                        for (int j = 31; j >= 0; j--)
                            if (newItem.FlagWriter.Items[j].Value == false)
                                newItem.FlagWriter.Items.Remove(newItem.FlagWriter.Items[j]);
                            else
                                break;
                    }

                    if (itemLoader.Strings != null)
                    {
                        for (int j = 0; j < itemLoader.Strings.Items.Count; j++)
                        {
                            var newStr = new ValueItem();
                            newStr.Value = itemLoader.Strings.Items[j];
                            newObject.Strings.Items.Add(newStr);
                        }
                    }

                    if (itemLoader.Movements == null)
                    {
                        var newMov = new MFAMovement();
                        newMov.Extension = "";
                        newMov.Type = 0;
                        newMov.Identifier = (uint)0;
                        newMov.Loader = null;
                        newMov.Player = 0;
                        newMov.MovingAtStart = 1;
                        newMov.DirectionAtStart = 0;
                        newObject.Movements.Items.Add(newMov);
                    }
                    else
                    {
                        for (int j = 0; j < itemLoader.Movements.Items.Count; j++)
                        {
                            var mov = itemLoader.Movements.Items[j];
                            var newMov = new MFAMovement();
                            newMov.Extension = "";
                            newMov.Type = mov.Type;
                            newMov.Identifier = (uint)mov.Type;
                            newMov.Loader = mov.Loader;
                            newMov.Player = mov.Player;
                            newMov.MovingAtStart = mov.MovingAtStart;
                            newMov.DirectionAtStart = mov.DirectionAtStart;
                            newObject.Movements.Items.Add(newMov);
                        }
                    }

                    newObject.Behaviours = new Behaviours();

                    if (item.ObjectType == (int)Constants.ObjectType.Active)
                    {
                        var active = new MFAActive();
                        //Shit Section
                        {
                            active.ObjectFlags = newObject.ObjectFlags;
                            active.NewObjectFlags = newObject.NewObjectFlags;
                            active.BackgroundColor = newObject.BackgroundColor;
                            active.Strings = newObject.Strings;
                            active.Values = newObject.Values;
                            active.Movements = newObject.Movements;
                            active.Behaviours = newObject.Behaviours;
                            active.Qualifiers = newObject.Qualifiers;
                        }

                        //TODO: Transitions
                        if (itemLoader.Animations != null)
                        {
                            var animHeader = itemLoader.Animations;
                            for (int j = 0; j < animHeader.AnimationDict.Count; j++)
                            {
                                if (CTFAKCore.parameters.Contains("-noimg"))
                                    break;
                                var origAnim = animHeader.AnimationDict.ToArray()[j];
                                var newAnimation = new MFAAnimation();
                                var newDirections = new List<MFAAnimationDirection>();
                                Animation animation = null;
                                if (animHeader.AnimationDict.ContainsKey(origAnim.Key))
                                {
                                    animation = animHeader?.AnimationDict[origAnim.Key];
                                }
                                else break;

                                if (animation != null)
                                {
                                    if (animation.DirectionDict != null)
                                    {
                                        for (int n = 0; n < animation.DirectionDict.Count; n++)
                                        {
                                            var direction = animation.DirectionDict.ToArray()[n].Value;
                                            var newDirection = new MFAAnimationDirection();
                                            newDirection.MinSpeed = direction.MinSpeed;
                                            newDirection.MaxSpeed = direction.MaxSpeed;
                                            newDirection.Index = n;
                                            newDirection.Repeat = direction.Repeat;
                                            newDirection.BackTo = direction.BackTo;
                                            if (CTFAKCore.parameters.Contains("-noimg"))
                                                newDirection.Frames = new List<int>();
                                            else
                                                newDirection.Frames = direction.Frames;
                                            newDirections.Add(newDirection);
                                        }
                                    }
                                    else
                                    {

                                    }

                                    newAnimation.Directions = newDirections;
                                }
                                active.Items.Add(j, newAnimation);
                            }
                        }
                        newItem.Loader = active;
                    }

                    if ((int)item.ObjectType >= 32)
                    {
                        var newExt = new MFAExtensionObject();
                        {
                            newExt.ObjectFlags = newObject.ObjectFlags;
                            newExt.NewObjectFlags = newObject.NewObjectFlags;
                            newExt.BackgroundColor = newObject.BackgroundColor;
                            newExt.Strings = newObject.Strings;
                            newExt.Values = newObject.Values;
                            newExt.Movements = newObject.Movements;
                            newExt.Behaviours = newObject.Behaviours;
                            newExt.Qualifiers = newObject.Qualifiers;
                        }
                        // if (Settings.GameType != GameType.OnePointFive)
                        {
                            Extensions exts = game.extensions;
                            CTFAK.CCN.Chunks.Extension ext = null;
                            foreach (var testExt in exts.Items)
                            {
                                if (testExt.Handle == (int)item.ObjectType - 32) ext = testExt;
                            }

                            newExt.ExtensionType = -1;
                            newExt.ExtensionName = "";
                            newExt.Filename = $"{ext.Name}.mfx";
                            newExt.Magic = (uint)ext.MagicNumber;
                            newExt.SubType = ext.SubType;
                            newExt.ExtensionVersion = itemLoader.ExtensionVersion;
                            newExt.ExtensionId = itemLoader.ExtensionId;
                            newExt.ExtensionPrivate = itemLoader.ExtensionPrivate;
                            newExt.ExtensionData = itemLoader.ExtensionData;

                            newItem.Loader = newExt;
                            var tuple = new Tuple<int, string, string, int, string>(ext.Handle, ext.Name, "",
                                ext.MagicNumber, ext.SubType);
                            // mfa.Extensions.Add(tuple);
                        }
                    }
                    else if (item.ObjectType == (int)Constants.ObjectType.Text)
                    {
                        var text = itemLoader.Text;
                        var newText = new MFAText();
                        //Shit Section
                        {
                            newText.ObjectFlags = newObject.ObjectFlags;
                            newText.NewObjectFlags = newObject.NewObjectFlags;
                            newText.BackgroundColor = newObject.BackgroundColor;
                            newText.Strings = newObject.Strings;
                            newText.Values = newObject.Values;
                            newText.Movements = newObject.Movements;
                            newText.Behaviours = newObject.Behaviours;
                            newText.Qualifiers = newObject.Qualifiers;

                            newItem.FlagWriter = new MFAObjectFlags();
                            newItem.FlagWriter = newItem.FlagWriter;

                        }
                        if (text == null)
                        {
                            newText.Width = 10;
                            newText.Height = 10;
                            newText.Font = 0;
                            newText.Color = Color.Black;
                            newText.Flags = 0;
                            newText.Items = new List<MFAParagraph>(){new MFAParagraph()
                            {
                                Value="ERROR"
                            }};
                        }
                        else
                        {
                            newText.Width = (uint)text.Width;
                            newText.Height = (uint)text.Height;
                            var paragraph = text.Items[0];
                            newText.Font = paragraph.FontHandle;
                            newText.Color = paragraph.Color;
                            newText.Flags = paragraph.Flags.flag;
                            newText.Items = new List<MFAParagraph>();
                            foreach (Paragraph exePar in text.Items)
                            {
                                var newPar = new MFAParagraph();
                                newPar.Value = exePar.Value;
                                newPar.Flags = exePar.Flags.flag;
                                newText.Items.Add(newPar);
                            }
                        }

                        newItem.Loader = newText;
                    }
                    else if (item.ObjectType == (int)Constants.ObjectType.Lives || item.ObjectType == (int)Constants.ObjectType.Score)
                    {
                        var counter = itemLoader.Counters;
                        var lives = new MFALives();
                        {
                            lives.ObjectFlags = newObject.ObjectFlags;
                            lives.NewObjectFlags = newObject.NewObjectFlags;
                            lives.BackgroundColor = newObject.BackgroundColor;
                            lives.Strings = newObject.Strings;
                            lives.Values = newObject.Values;
                            lives.Movements = newObject.Movements;
                            lives.Behaviours = newObject.Behaviours;
                            lives.Qualifiers = newObject.Qualifiers;
                        }
                        lives.Player = counter?.Player ?? 0;
                        if (!CTFAKCore.parameters.Contains("-noimg"))
                            lives.Images = counter?.Frames ?? new List<int>() { 0 };
                        lives.DisplayType = counter?.DisplayType ?? 0;
                        lives.Flags = counter?.Flags ?? 0;
                        lives.Font = counter?.Font ?? 0;
                        lives.Width = (int)(counter?.Width ?? 0);
                        lives.Height = (int)(counter?.Height ?? 0);
                        newItem.Loader = lives;
                    }
                    else if (item.ObjectType == (int)Constants.ObjectType.Counter)
                    {
                        var counter = itemLoader.Counters;
                        var newCount = new MFACounter();
                        {
                            newCount.ObjectFlags = newObject.ObjectFlags;
                            newCount.NewObjectFlags = newObject.NewObjectFlags;
                            newCount.BackgroundColor = newObject.BackgroundColor;
                            newCount.Strings = newObject.Strings;
                            newCount.Values = newObject.Values;
                            newCount.Movements = newObject.Movements;
                            newCount.Behaviours = newObject.Behaviours;
                            newCount.Qualifiers = newObject.Qualifiers;
                        }
                        if (itemLoader.Counter == null)
                        {
                            newCount.Value = 0;
                            newCount.Minimum = 0;
                            newCount.Maximum = 0;
                        }
                        else
                        {
                            newCount.Value = itemLoader.Counter.Initial;
                            newCount.Maximum = itemLoader.Counter.Maximum;
                            newCount.Minimum = itemLoader.Counter.Minimum;
                        }

                        var shape = counter?.Shape;
                        if (counter == null)
                        {
                            newCount.DisplayType = 0;
                            newCount.CountType = 0;
                            newCount.Width = 0;
                            newCount.Height = 0;
                            newCount.Images = new List<int>() { 0 };
                            newCount.Font = 0;
                        }
                        else
                        {
                            newCount.DisplayType = counter.DisplayType;
                            newCount.CountType = counter.Inverse ? 1 : 0;
                            newCount.Width = (int)counter.Width;
                            newCount.Height = (int)counter.Height;
                            if (CTFAKCore.parameters.Contains("-noimg"))
                                newCount.Images = new List<int>();
                            else
                                newCount.Images = counter.Frames;
                            newCount.Font = counter.Font;
                        }

                        if (shape == null)
                        {
                            newCount.Color1 = Color.Black;
                            newCount.Color2 = Color.Black;
                            newCount.VerticalGradient = 0;
                            newCount.CountFlags = 0;
                        }
                        else
                        {
                            newCount.Color1 = shape.Color1;
                            newCount.Color2 = shape.Color2;
                            newCount.VerticalGradient = (uint)shape.GradFlags;
                            newCount.CountFlags = (uint)shape.FillType;
                        }
                        newItem.Loader = newCount;
                    }

                    else if (item.ObjectType == 9)
                    {
                        var newSubApp = new MFASubApplication();
                        newSubApp.ObjectFlags = newObject.ObjectFlags;
                        newSubApp.NewObjectFlags = newObject.NewObjectFlags;
                        newSubApp.BackgroundColor = newObject.BackgroundColor;
                        newSubApp.Strings = newObject.Strings;
                        newSubApp.Values = newObject.Values;
                        newSubApp.Movements = newObject.Movements;
                        newSubApp.Behaviours = newObject.Behaviours;
                        newSubApp.Qualifiers = newObject.Qualifiers;
                        try
                        {
                            newSubApp.fileName = itemLoader.SubApplication.odName;
                            newSubApp.width = itemLoader.SubApplication.odCx;
                            newSubApp.height = itemLoader.SubApplication.odCy;
                            newSubApp.flaggyflag = itemLoader.SubApplication.odOptions;
                            newSubApp.frameNum = itemLoader.SubApplication.odNStartFrame;
                        }
                        catch (Exception)
                        {
                            newSubApp.fileName = "";
                            newSubApp.width = 128;
                            newSubApp.height = 128;
                            newSubApp.flaggyflag = 0;
                            newSubApp.frameNum = 3;
                        }
                        newItem.Loader = newSubApp;
                    }
                }
                //Logger.Log("Name: " + newItem.Name + ", Object type: " + newItem.ObjectType);
                return newItem;
            }
            game.Images.Items = imgs;
        }
    }
}