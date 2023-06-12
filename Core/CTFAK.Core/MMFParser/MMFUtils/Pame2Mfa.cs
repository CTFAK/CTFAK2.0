using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.CCN.Chunks.Frame;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Common.Banks;
using CTFAK.MMFParser.Common.Events;
using CTFAK.MMFParser.MFA;
using CTFAK.MMFParser.MFA.MFAObjectLoaders;
using CTFAK.Properties;
using CTFAK.Tools;
using CTFAK.Utils;

namespace CTFAK.MMFParser.MMFUtils;

public class Pame2Mfa
{
    public static int lastAllocatedHandleImg = 15;

    public static Dictionary<int, MFAObjectInfo> FrameItems;
    public int[] Progress = { };

    public static MFAData Convert(GameData game,Dictionary<int,Bitmap> icons=null)
    {
        var mfa = new MFAData();
        mfa.Read(new ByteReader("template.mfa",FileMode.Open));
        mfa.Chunks.Items.Clear();
        mfa.Name = game.Name;
        mfa.LangId = 0; //8192;
        mfa.Description = "";
        mfa.Path = game.EditorFilename;
        mfa.Menu = game.Menu;


        //if (game.Fonts != null) mfa.Fonts = game.Fonts;
        // mfa.Sounds.Items.Clear();
        if (game.Sounds != null && game.Sounds.Items != null)
        {
            foreach (var item in game.Sounds.Items) mfa.Sounds.Items.Add(item);
            if (CTFAKCore.Parameters.Contains("-nosound"))
                mfa.Sounds.Items.Clear();
        }

        mfa.Fonts.Items.Clear();
        if (game.Fonts?.Items != null)
            foreach (var item in game.Fonts.Items)
            {
                item.Compressed = false;
                mfa.Fonts.Items.Add(item);
            }

        mfa.Music = game.Music;
        mfa.Images.Items = game.Images.Items;
        foreach (var key in mfa.Images.Items.Keys) mfa.Images.Items[key].IsMFA = true;
        mfa.GraphicMode = 4;
        
        
        foreach (var item in mfa.Icons.Items)
            try
            {
                switch (item.Key)
                {
                    case 2:
                    case 5:
                    case 8:
                        item.Value.FromBitmap(icons[16]);
                        break;
                    case 1:
                    case 4:
                    case 7:
                        item.Value.FromBitmap(icons[32]);
                        break;
                    case 0:
                    case 3:
                    case 6:
                        item.Value.FromBitmap(icons[48]);
                        break;
                    case 9:
                        item.Value.FromBitmap(icons[128]);
                        break;
                    case 10:
                        item.Value.FromBitmap(icons[256]);
                        break;
                }
            }
            catch
            {
                Logger.LogWarning($"Requested icon is not found: {item.Key} - {item.Value.Width}");
            }
        var imageNull = new FusionImage();
        imageNull.Handle = 14;
        imageNull.Transparent = Color.Brown;
        imageNull.FromBitmap(Resources.EmptyIcon);
        mfa.Icons.Items.Add(14, imageNull);
        // game.Images.Images.Clea r();

        mfa.Author = game.Author;
        mfa.Copyright = game.Copyright;
        mfa.Company = "";
        mfa.Version = "";
        //mfa.binaryFiles = game.BinaryFiles;
        var displaySettings = mfa.DisplayFlags;
        var graphicSettings = mfa.GraphicFlags;
        var flags = game.Header.Flags;
        var newFlags = game.Header.NewFlags;
        mfa.Extensions.Clear();

        displaySettings["MaximizedOnBoot"] = flags["Maximize"];
        displaySettings["ResizeDisplay"] = flags["MDI"];
        displaySettings["FullscreenAtStart"] = flags["FullscreenAtStart"];
        displaySettings["AllowFullscreen"] = flags["FullscreenSwitch"];
        // displaySettings["Heading"] = !flags["NoHeading"];
        // displaySettings["HeadingWhenMaximized"] = true;
        displaySettings["MenuBar"] = flags["MenuBar"];
        displaySettings["MenuOnBoot"] = !flags["MenuHidden"];
        displaySettings["NoMinimize"] = newFlags["NoMinimizeBox"];
        displaySettings["NoMaximize"] = newFlags["NoMaximizeBox"];
        displaySettings["NoThickFrame"] = newFlags["NoThickFrame"];
        // displaySettings["NoCenter"] = flags["MDI"];
        displaySettings["DisableClose"] = newFlags["DisableClose"];
        displaySettings["HiddenAtStart"] = newFlags["HiddenAtStart"];
        displaySettings["MDI"] = newFlags["MDI"];

        /*for (int i = 0; i < game.globalValues.Items.Count; i++)
        {
            var globalValue = game.globalValues.Items[i];


            mfa.GlobalValues.Items.Add(new ValueItem(null)
            {
                Value = (globalValue is float) ? (float)globalValue:(int)globalValue,
                Name = $"Global Value "+i

            });
        }
        for (int i = 0; i < game.globalStrings.Items.Count; i++)
        {
            var globalString = game.globalStrings.Items[i];


            mfa.GlobalStrings.Items.Add(new ValueItem(null)
            {
                Value = globalString,
                Name = $"Global Value "+i

            });
        }*/
        //mfa.GraphicFlags = graphicSettings;
        //mfa.DisplayFlags = displaySettings;
        mfa.WindowX = game.Header.WindowWidth;
        mfa.WindowY = game.Header.WindowHeight;
        mfa.BorderColor = game.Header.BorderColor;
        mfa.HelpFile = "";
        mfa.InitialScore = game.Header.InitialScore;
        mfa.InitialLifes = game.Header.InitialLives;
        mfa.FrameRate = game.Header.FrameRate;
        mfa.BuildType = 0;
        mfa.BuildPath = game.TargetFilename;
        mfa.CommandLine = "";
        mfa.Aboutbox = game.AboutText ?? "Decompiled with CTFAK 2.0";
        
        mfa.Controls = new MFAControls();
        mfa.MenuImages = new Dictionary<int, int>();
        mfa.GlobalValues = new MFAValueList();
        mfa.GlobalStrings = new MFAValueList();
        mfa.GlobalEvents = new byte[0];
        mfa.IconImages = new List<int>();
        mfa.MfaVersion = 6;
        mfa.Product = 2;
        mfa.BuildVersion = 292;
        mfa.LangId = 1033;
        mfa.InitialLifes = 3;

        //TODO: Controls

        //Object Section
        FrameItems = new Dictionary<int, MFAObjectInfo>();
        for (var i = 0; i < game.FrameItems.Keys.Count; i++)
        {
            var key = game.FrameItems.Keys.ToArray()[i];
            var item = game.FrameItems[key];
            var newItem = new MFAObjectInfo();
            if (item.ObjectType >= 32)
            {
                //Logger.Log(item.ObjectType + ", " + item.name);
                if ((item.ObjectType == 36 && item.Name == "iOS Plus Object") ||
                    (item.ObjectType == 45 && item.Name.Contains("KYSO")))
                    continue; //DIE YOU UNDEAD FLESH MAGGOT! 

                newItem = TranslateObject(mfa, game, item, true);
            }
            else
            {
                //Logger.Log(item.ObjectType);
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

        var indexHandles = new Dictionary<int, int>();
        if (game.FrameHandles != null)
            foreach (var pair in game.FrameHandles.Items)
            {
                var key = pair.Key;
                var handle = pair.Value;
                if (!indexHandles.ContainsKey(handle)) indexHandles.Add(handle, key);
                else indexHandles[handle] = key;
            }

        Logger.Log($"Prepating to translate {game.Frames.Count} frames");
        for (var a = 0; a < game.Frames.Count; a++)
            if (CTFAKCore.Parameters.Contains(a.ToString()))
            {
            }
            else
            {
                var frame = game.Frames[a];

                if (frame.Name == "") continue;
                //if(frame.Palette==null|| frame.Events==null|| frame.Objects==null) continue;
                var newFrame = new MFAFrame();
                newFrame.Chunks = new MFAChunkList(); //MFA.MFA.emptyFrameChunks;
                newFrame.Handle = a;
                if (!indexHandles.TryGetValue(a, out newFrame.Handle)) Logger.Log("Error while getting frame handle");

                newFrame.Name = frame.Name;
                newFrame.SizeX = frame.Width;
                newFrame.SizeY = frame.Height;

                newFrame.Background = frame.Background;
                newFrame.FadeIn = frame.FadeIn != null ? ConvertTransition(frame.FadeIn) : null;
                newFrame.FadeOut = frame.FadeOut != null ? ConvertTransition(frame.FadeOut) : null;
                var mfaFlags = newFrame.Flags;
                var originalFlags = frame.Flags;

                mfaFlags["GrabDesktop"] = originalFlags["GrabDesktop"];
                mfaFlags["KeepDisplay"] = originalFlags["KeepDisplay"];
                mfaFlags["BackgroundCollisions"] = originalFlags["TotalCollisionMask"];
                mfaFlags["ResizeToScreen"] = originalFlags["ResizeAtStart"];
                mfaFlags["ForceLoadOnCall"] = originalFlags["ForceLoadOnCall"];
                mfaFlags["NoDisplaySurface"] = false;
                mfaFlags["TimerBasedMovements"] = originalFlags["TimedMovements"];
                newFrame.Flags = mfaFlags;
                newFrame.MaxObjects = frame.Events?.MaxObjects ?? 10000;
                newFrame.Password = new byte[0];
                newFrame.LastViewedX = 320;
                newFrame.LastViewedY = 240;
                // if (frame.Palette == null) continue; // this shouldn't be here. i have no idea how it got here
                newFrame.Palette = frame.Palette ?? new List<Color>();
                newFrame.StampHandle = 13;
                newFrame.ActiveLayer = 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Left = frame.VirtualRect?.Left ?? 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Top = frame.VirtualRect?.Top ?? 0;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Right = frame.VirtualRect?.Right ?? frame.Width;
                newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Bottom = frame.VirtualRect?.Bottom ?? frame.Height;
                if (frame.ShaderData.HasShader)
                {
                    var shdrData = newFrame.Chunks.GetOrCreateChunk<ShaderSettings>();
                    if (frame.InkEffect != 1 && !CTFAKCore.Parameters.Contains("-notrans"))
                    {
                        shdrData.Blend = frame.Blend;
                        shdrData.RGBCoeff = Color.FromArgb(frame.RgbCoeff.A, frame.RgbCoeff.R, frame.RgbCoeff.G,
                            frame.RgbCoeff.B);
                    }

                    if (ImageBank.realGraphicMode < 4 && Settings.Build < 289 && !Settings.Android)
                    {
                        shdrData.Blend = (byte)(255 - frame.Blend);
                        shdrData.RGBCoeff = Color.FromArgb(frame.RgbCoeff.A, 255 - frame.RgbCoeff.R,
                            255 - frame.RgbCoeff.G, 255 - frame.RgbCoeff.B);
                    }

                    var newShader = new ShaderSettings.MFAShader();
                    newShader.Name = frame.ShaderData.Name;
                    foreach (var param in frame.ShaderData.Parameters)
                    {
                        var newParam = new ShaderSettings.ShaderParameter();
                        newParam.Name = param.Name;
                        newParam.Value = param.Value;
                        newParam.ValueType = param.ValueType;
                        newShader.Parameters.Add(newParam);
                    }

                    shdrData.Shaders.Add(newShader);
                }

                //LayerInfo
                var layerCount = frame.Layers?.Items.Count;
                if (layerCount==0)
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
                    
                    for (var i = 0; i < layerCount; i++)
                    {
                        var layer = frame.Layers.Items[i];
                        var newLayer = new MFALayer();
                        newLayer.Name = layer.Name;
                        newLayer.Flags["HideAtStart"] = layer.Flags["ToHide"];
                        newLayer.Flags["Visible"] = true; //layer.Flags["Visible"];;
                        newLayer.Flags["NoBackground"] = layer.Flags["DoNotSaveBackground"];
                        newLayer.Flags["WrapHorizontally"] = layer.Flags["WrapHorizontally"];

                        newLayer.XCoefficient = layer.XCoeff;
                        newLayer.YCoefficient = layer.YCoeff;

                        newFrame.Layers.Add(newLayer);
                    }
                }

                var newFrameItems = new List<MFAObjectInfo>();
                var newInstances = new List<MFAObjectInstance>();
                if (frame.Objects != null)
                    //if (false)
                    for (var i = 0; i < frame.Objects.Count; i++)
                    {
                        var instance = frame.Objects[i];
                        MFAObjectInfo frameItem;

                        if (FrameItems.ContainsKey(instance.ObjectInfo))
                        {
                            frameItem = FrameItems[instance.ObjectInfo];
                            if (!newFrameItems.Contains(frameItem)) newFrameItems.Add(frameItem);
                            var newInstance = new MFAObjectInstance();
                            newInstance.X = instance.X;
                            newInstance.Y = instance.Y;
                            newInstance.Handle = i; //instance.handle;
                            if (instance.ParentType != 0) newInstance.Flags = 8;
                            else newInstance.Flags = 0;

                            // newInstance.Flags = ((instance.FrameItem.Properties.Loader as ObjectCommon)?.Preferences?.flag ?? (uint)instance.FrameItem.Flags);
                            //newInstance.Flags = (uint)instance.flags;
                            newInstance.ParentType = instance.ParentType;
                            newInstance.ItemHandle = instance.ObjectInfo;
                            newInstance.ParentHandle = instance.ParentHandle;
                            newInstance.Layer = instance.Layer;
                            newInstances.Add(newInstance);
                        }
                        else
                        {
                            Logger.Log($"WARNING: OBJECT NOT FOUND - {instance.ObjectInfo}");
                        }
                    }

                newFrame.Items = newFrameItems;
                newFrame.Instances = newInstances;
                newFrame.Folders = new List<MFAItemFolder>();
                foreach (var newFrameItem in newFrame.Items)
                {
                    var newFolder = new MFAItemFolder();
                    newFolder.IsHiddenFolder = true;
                    newFolder.Items = new List<uint> { (uint)newFrameItem.Handle };
                    newFrame.Folders.Add(newFolder);
                }

                //if(false)
                {
                    newFrame.Events = new MFAEvents();
                    newFrame.Events.Items = new List<EventGroup>();
                    newFrame.Events.Objects = new List<EventObject>();
                    newFrame.Events.Version = 1028;
                    //if(false)
                    if (frame.Events != null)
                        if (!CTFAKCore.Parameters.Contains("-noevnt"))
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

                            newFrame.Events.Items = frame.Events.Items;

                            var qualifiers = new Dictionary<int, Quailifer>();
                            foreach (var quailifer in frame.Events.QualifiersList)
                            {
                                var newHandle = 0;
                                while (true)
                                {
                                    if (!newFrame.Items.Any(item => item.Handle == newHandle) &&
                                        !qualifiers.Keys.Any(item => item == newHandle)) break;
                                    newHandle++;
                                }

                                //newHandle = quailifer.ObjectInfo;
                                qualifiers.Add(newHandle, quailifer);
                                var qualItem = new EventObject();
                                qualItem.Handle = (uint)newHandle;
                                qualItem.SystemQualifier = (ushort)quailifer.Qualifier;
                                qualItem.Name = "";
                                qualItem.TypeName = "";
                                qualItem.ItemType = (ushort)quailifer.Type;
                                qualItem.ObjectType = 3;
                                newFrame.Events.Objects.Add(qualItem);
                            }

                            for (var eg = 0;
                                 eg < newFrame.Events.Items.Count;
                                 eg++) //foreach (EventGroup eventGroup in newFrame.Events.Items)
                            {
                                var eventGroup = newFrame.Events.Items[eg];
                                foreach (var action in eventGroup.Actions)
                                {
                                    if (action.ObjectType == -5 && action.Num == 0)
                                        continue;
                                    foreach (var quailifer in qualifiers)
                                    {

                                        if (quailifer.Value.ObjectInfo == action.ObjectInfo &&
                                            quailifer.Value.Type == action.ObjectType)
                                        {
                                            action.ObjectInfo = quailifer.Key;
                                            action.ObjectType = quailifer.Value.Type;
                                        }

                                        foreach (var param in action.Items)
                                        {
                                            if (param.Loader is ExpressionParameter expr)
                                            {
                                                foreach (var actualExpr in expr.Items)
                                                {
                                                    if (quailifer.Value.ObjectInfo == actualExpr.ObjectInfo)
                                                    {
                                                        actualExpr.ObjectInfo = quailifer.Key;
                                                        actualExpr.ObjectType = quailifer.Value.Type;
                                                    }

                                                }
                                            }
                                            else if (param.Loader is ParamObject obj)
                                            {
                                                if (quailifer.Value.ObjectInfo == obj.ObjectInfo)
                                                {
                                                    obj.ObjectInfo = quailifer.Key;
                                                    obj.ObjectType = quailifer.Value.Type;
                                                }
                                            }
                                            else if (param.Loader is Position pos)
                                            {
                                                if (quailifer.Value.ObjectInfo == -pos.ObjectInfoParent)
                                                {
                                                    pos.ObjectInfoParent = quailifer.Key;
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (var cond in eventGroup.Conditions)
                                foreach (var quailifer in qualifiers)
                                {
                                    if (quailifer.Value.ObjectInfo == cond.ObjectInfo &&
                                        quailifer.Value.Type == cond.ObjectType)
                                    {
                                        cond.ObjectInfo = quailifer.Key;
                                        cond.ObjectType = quailifer.Value.Type;
                                    }

                                    foreach (var param in cond.Items)
                                    {
                                        if (param.Loader is ExpressionParameter expr)
                                        {
                                            foreach (var actualExpr in expr.Items)
                                            {
                                                if (quailifer.Value.ObjectInfo == actualExpr.ObjectInfo)
                                                {
                                                    actualExpr.ObjectInfo = quailifer.Key;
                                                    actualExpr.ObjectType = quailifer.Value.Type;
                                                }
                                            }
                                        }
                                        else if (param.Loader is ParamObject obj)
                                        {
                                            if (quailifer.Value.ObjectInfo == obj.ObjectInfo)
                                            {
                                                obj.ObjectInfo = quailifer.Key;
                                                obj.ObjectType = quailifer.Value.Type;
                                            }
                                        }
                                        else if (param.Loader is Position pos)
                                        {
                                            if (quailifer.Value.ObjectInfo == -pos.ObjectInfoParent)
                                            {
                                                pos.ObjectInfoParent = quailifer.Key;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                }
                if (CTFAKCore.Parameters.Contains(a.ToString()) == false)
                {
                    Logger.Log($"Translating frame {frame.Name} - {a}");
                    mfa.Frames.Add(newFrame);
                }
            }
        
        return mfa;
    }
    static MFATransition ConvertTransition(Transition gameTrans)
        {
            var newName = "";
            newName = gameTrans.Name;
            newName = newName.ToLower();
            var mfaTrans = new MFATransition
            {
                Module = "cctrans.dll", //gameTrans.ModuleFile,
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
            newItem.Chunks = new MFAChunkList();
            newItem.Name = item.Name;
            newItem.ObjectType = item.ObjectType;
            newItem.Handle = item.Handle;
            newItem.Transparent = 1;
            newItem.InkEffect = item.InkEffect;
            newItem.InkEffectParameter = (uint)item.InkEffectValue;
            newItem.AntiAliasing = 0;
            newItem.Flags = item.Flags;
            var noicon = false;
            Bitmap iconBmp = null;
            if (newItem.ObjectType >= 32)
            {
                Extension ext = null;

                foreach (var testExt in game.Extensions.Items)
                    if (testExt.Handle == item.ObjectType - 32)
                        ext = testExt;
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
                            var bmp = game.Images.Items[((Quickbackdrop)item.Properties).Image].bitmap;
                            if (bmp.Width > bmp.Height)
                                iconBmp = bmp.ResizeImage(new Size(32,
                                    (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                            else
                                iconBmp = bmp.ResizeImage(
                                    new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                        }
                        catch
                        {
                            iconBmp = Resources.Backdrop;
                        }

                        break;
                    case 1: //Backdrop
                        try
                        {
                            var bmp = game.Images.Items[((Backdrop)item.Properties).Image].bitmap;
                            if (bmp.Width > bmp.Height)
                                iconBmp = bmp.ResizeImage(new Size(32,
                                    (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                            else
                                iconBmp = bmp.ResizeImage(
                                    new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
                        }
                        catch
                        {
                            iconBmp = Resources.Backdrop;
                        }

                        break;
                    case 2: //Active
                        try
                        {
                            var bmp = game.Images
                                .Items[
                                    ((ObjectCommon)item.Properties).Animations.AnimationDict.First().Value.DirectionDict
                                    .First().Value.Frames.First()].bitmap;
                            if (bmp.Width > bmp.Height)
                                iconBmp = bmp.ResizeImage(new Size(32,
                                    (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                            else
                                iconBmp = bmp.ResizeImage(
                                    new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
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
                            var bmp = game.Images.Items[((ObjectCommon)item.Properties).Counters.Frames.First()].bitmap;
                            if (bmp.Width > bmp.Height)
                                iconBmp = bmp.ResizeImage(new Size(32,
                                    (int)Math.Round((float)bmp.Height / bmp.Width * 32.0)));
                            else
                                iconBmp = bmp.ResizeImage(
                                    new Size((int)Math.Round((float)bmp.Width / bmp.Height * 32.0), 32));
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
            if (CTFAKCore.Parameters.Contains("-noicons"))
            {
                noicon = false;
                iconBmp = Resources.Active;
            }

            if (!noicon)
            {
                lastAllocatedHandleImg++;
                var newIconImage = new FusionImage();
                newIconImage.Handle = lastAllocatedHandleImg;
                newIconImage.FromBitmap(iconBmp);
                mfa.Icons.Items.Add(lastAllocatedHandleImg, newIconImage);
            }

            newItem.IconHandle = noicon ? 14 : lastAllocatedHandleImg;

            var shdrData = newItem.Chunks.GetOrCreateChunk<ShaderSettings>();
            shdrData.Blend = item.Blend;
            shdrData.RGBCoeff = Color.FromArgb(item.RgbCoeff.A, item.RgbCoeff.B, item.RgbCoeff.G, item.RgbCoeff.R);
            if (item.ShaderData.HasShader)
            {
                var newShader = new ShaderSettings.MFAShader();
                newShader.Name = item.ShaderData.Name;
                foreach (var param in item.ShaderData.Parameters)
                {
                    var newParam = new ShaderSettings.ShaderParameter();
                    newParam.Name = param.Name;
                    newParam.Value = param.Value;
                    newParam.ValueType = param.ValueType;
                    newShader.Parameters.Add(newParam);
                }

                shdrData.Shaders.Add(newShader);
            }


            try
            {
                for (var i = 0; i < game.GlobalValues.Items.Count; i++)
                {
                    var globalValue = game.GlobalValues.Items[i];


                    mfa.GlobalValues.Items.Add(new ValueItem
                    {
                        Value = globalValue,
                        Name = "Global Value " + i
                    });
                }

                for (var i = 0; i < game.GlobalStrings.Items.Count; i++)
                {
                    var globalString = game.GlobalStrings.Items[i];


                    mfa.GlobalStrings.Items.Add(new ValueItem
                    {
                        Value = globalString,
                        Name = "Global String " + i
                    });
                }
            }
            catch
            {
            }

            if (item.ObjectType == (int)Constants.ObjectType.QuickBackdrop)
            {
                var backdropLoader = item.Properties as Quickbackdrop;
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
                if (!CTFAKCore.Parameters.Contains("-noimg"))
                    backdrop.Image = backdropLoader.Shape.Image;
                newItem.Loader = backdrop;
            }
            else if (item.ObjectType == (int)Constants.ObjectType.Backdrop)
            {
                var backdropLoader = item.Properties as Backdrop;
                var backdrop = new MFABackdrop();
                backdrop.ObstacleType = (uint)backdropLoader.ObstacleType;
                backdrop.CollisionType = (uint)backdropLoader.CollisionType;
                if (!CTFAKCore.Parameters.Contains("-noimg"))
                    backdrop.Handle = backdropLoader.Image;
                newItem.Loader = backdrop;
            }
            else
            {
                var itemLoader = item?.Properties as ObjectCommon;
                if (itemLoader == null) throw new NotImplementedException("Null loader");
                //CommonSection
                var newObject = new ObjectLoader();
                newObject.ObjectFlags = (int)itemLoader.Flags.Flag;
                newObject.NewObjectFlags = (int)itemLoader.NewFlags.Flag;
                newObject.BackgroundColor = itemLoader.BackColor;
                newObject.Qualifiers = itemLoader.Qualifiers;

                newObject.Strings = new MFAValueList(); //ConvertStrings(itemLoader.);
                newObject.Values = new MFAValueList(); //ConvertValue(itemLoader.Values);
                newObject.Movements = new MFAMovements();

                if (itemLoader.Values != null)
                    for (var j = 0; j < itemLoader.Values.Items.Count; j++)
                    {
                        var ch = "A";
                        if (j >= 26)
                            ch = (char)('A' + ((j - j % 26) / 26 - 1)) + ((char)('A' + j % 26)).ToString();
                        else
                            ch = ((char)('A' + j)).ToString();

                        var newVal = new ValueItem();
                        newVal.Name = $"Alterable Value {ch}";
                        newVal.Value = itemLoader.Values.Items[j];
                        newObject.Values.Items.Add(newVal);
                    }

                if (itemLoader.Strings != null)
                    for (var j = 0; j < itemLoader.Strings.Items.Count; j++)
                    {
                        var ch = "A";
                        if (j >= 26)
                            ch = (char)('A' + ((j - j % 26) / 26 - 1)) + ((char)('A' + j % 26)).ToString();
                        else
                            ch = ((char)('A' + j)).ToString();

                        var newStr = new ValueItem();
                        newStr.Name = $"Alterable String {ch}";
                        newStr.Value = itemLoader.Strings.Items[j];
                        newObject.Strings.Items.Add(newStr);
                    }

                if (itemLoader.Movements == null)
                {
                    var newMov = new MFAMovement();
                    newMov.Name = $"Movement #{0}";
                    newMov.Extension = "";
                    newMov.Type = 0;
                    newMov.Identifier = 0;
                    newMov.Loader = null;
                    newMov.Player = 0;
                    newMov.MovingAtStart = 1;
                    newMov.DirectionAtStart = 0;
                    newObject.Movements.Items.Add(newMov);
                }
                else
                {
                    for (var j = 0; j < itemLoader.Movements.Items.Count; j++)
                    {
                        var mov = itemLoader.Movements.Items[j];
                        var newMov = new MFAMovement();
                        newMov.Name = $"Movement #{j}";
                        newMov.Extension = "";
                        newMov.Type = mov.Type;
                        newMov.Identifier = mov.Type;
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
                        for (var j = 0; j < animHeader.AnimationDict.Count; j++)
                        {
                            if (CTFAKCore.Parameters.Contains("-noimg"))
                                break;
                            var origAnim = animHeader.AnimationDict.ToArray()[j];
                            var newAnimation = new MFAAnimation();
                            newAnimation.Name = $"User Defined {j}";
                            var newDirections = new List<MFAAnimationDirection>();
                            Animation animation = null;
                            if (animHeader.AnimationDict.ContainsKey(origAnim.Key))
                                animation = animHeader?.AnimationDict[origAnim.Key];
                            else break;

                            if (animation != null)
                            {
                                if (animation.DirectionDict != null)
                                    for (var n = 0; n < animation.DirectionDict.Count; n++)
                                    {
                                        var direction = animation.DirectionDict.ToArray()[n].Value;
                                        var newDirection = new MFAAnimationDirection();
                                        newDirection.MinSpeed = direction.MinSpeed;
                                        newDirection.MaxSpeed = direction.MaxSpeed;
                                        newDirection.Index = n;
                                        newDirection.Repeat = direction.Repeat;
                                        newDirection.BackTo = direction.BackTo;
                                        if (CTFAKCore.Parameters.Contains("-noimg"))
                                            newDirection.Frames = new List<int>();
                                        else
                                            newDirection.Frames = direction.Frames;
                                        newDirections.Add(newDirection);
                                    }

                                newAnimation.Directions = newDirections;
                            }

                            active.Items.Add(j, newAnimation);
                        }
                    }

                    newItem.Loader = active;
                }

                if (item.ObjectType >= 32)
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
                        var exts = game.Extensions;
                        Extension ext = null;
                        foreach (var testExt in exts.Items)
                            if (testExt.Handle == item.ObjectType - 32)
                                ext = testExt;

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
                    }
                    if (text == null)
                    {
                        newText.Width = 10;
                        newText.Height = 10;
                        newText.Font = 0;
                        newText.Color = Color.Black;
                        newText.Flags = 0;
                        newText.Items = new List<MFAParagraph>
                        {
                            new()
                            {
                                Value = "ERROR"
                            }
                        };
                    }
                    else
                    {
                        newText.Width = (uint)text.Width;
                        newText.Height = (uint)text.Height;
                        var paragraph = text.Items[0];
                        newText.Font = paragraph.FontHandle;
                        newText.Color = paragraph.Color;
                        newText.Flags = paragraph.Flags.Flag;
                        newText.Items = new List<MFAParagraph>();
                        foreach (var exePar in text.Items)
                        {
                            var newPar = new MFAParagraph();
                            newPar.Value = exePar.Value;
                            newPar.Flags = exePar.Flags.Flag;
                            newText.Items.Add(newPar);
                        }
                    }

                    newItem.Loader = newText;
                }
                else if (item.ObjectType == (int)Constants.ObjectType.Lives ||
                         item.ObjectType == (int)Constants.ObjectType.Score)
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
                    if (!CTFAKCore.Parameters.Contains("-noimg"))
                        lives.Images = counter?.Frames ?? new List<int> { 0 };
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
                        newCount.Images = new List<int> { 0 };
                        newCount.Font = 0;
                    }
                    else
                    {
                        newCount.DisplayType = counter.DisplayType;
                        newCount.CountType = counter.Inverse ? 1 : 0;
                        newCount.Width = (int)counter.Width;
                        newCount.Height = (int)counter.Height;
                        if (CTFAKCore.Parameters.Contains("-noimg"))
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
                        newSubApp.FileName = itemLoader.SubApplication.OdName;
                        newSubApp.Width = itemLoader.SubApplication.OdCx;
                        newSubApp.Height = itemLoader.SubApplication.OdCy;
                        newSubApp.Flaggyflag = itemLoader.SubApplication.OdOptions;
                        newSubApp.FrameNum = itemLoader.SubApplication.OdNStartFrame;
                    }
                    catch (Exception)
                    {
                        newSubApp.FileName = "";
                        newSubApp.Width = 128;
                        newSubApp.Height = 128;
                        newSubApp.Flaggyflag = 0;
                        newSubApp.FrameNum = 3;
                    }

                    newItem.Loader = newSubApp;
                }
            }

            //Logger.Log("Name: " + newItem.Name + ", Object type: " + newItem.ObjectType);
            return newItem;
        }
}