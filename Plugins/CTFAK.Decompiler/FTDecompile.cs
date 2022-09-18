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
using CTFAKCore.Properties;
using Ionic.Zlib;
using Microsoft.VisualBasic;
using Action = CTFAK.CCN.Chunks.Frame.Action;
using Constants = CTFAK.CCN.Constants;

namespace CTFAK.Tools
{
    class FTDecompile : IFusionTool
    {
        public string Name => "Decompiler";
        public static int lastAllocatedHandleImg = 15;

        public static Dictionary<int, MFAObjectInfo> FrameItems;
        public void Execute(IFileReader reader)
        {
            var game = reader.getGameData();
            var mfa = new MFAData();
            bool myAss = false;
            Settings.gameType = Settings.GameType.NORMAL;
            if (Settings.Old)
            {
                myAss = true;
                Settings.gameType = Settings.GameType.NORMAL;
            }
            mfa.Read(new ByteReader("template.mfa", FileMode.Open));
            if (myAss)
            {
                Settings.gameType = Settings.GameType.MMF15;
            }

            mfa.Name = game.name;
            mfa.LangId = 0;//8192;
            mfa.Description = "";
            mfa.Path = game.editorFilename;

            //if (game.Fonts != null) mfa.Fonts = game.Fonts;
            // mfa.Sounds.Items.Clear();
            if (game.Sounds != null && game.Sounds.Items != null)
            {

                foreach (var item in game.Sounds.Items)
                {
                    mfa.Sounds.Items.Add(item);
                }
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

            mfa.Music = game.Music;
            mfa.Images.Items = game.Images?.Items;
            foreach (var key in mfa.Images.Items.Keys)
            {
                mfa.Images.Items[key].IsMFA = true;
            }
            foreach (var item in mfa.Icons.Items)
            {
                try
                {


                    switch (item.Key)
                    {
                        case 2:
                        case 5:
                        case 8:
                            item.Value.FromBitmap(reader.getIcons()[16]);
                            break;
                        case 1:
                        case 4:
                        case 7:
                            item.Value.FromBitmap(reader.getIcons()[32]);
                            break;
                        case 0:
                        case 3:
                        case 6:
                            item.Value.FromBitmap(reader.getIcons()[48]);
                            break;
                        case 9:
                            item.Value.FromBitmap(reader.getIcons()[128]);
                            break;
                        case 10:
                            item.Value.FromBitmap(reader.getIcons()[256]);
                            break;



                        default:
                            break;
                    }
                }
                catch
                {
                    Logger.LogWarning($"Requested icon is not found: {item.Key} - {item.Value.width}");
                }

            }
            var imageNull = new CCN.Chunks.Banks.Image(null);
            imageNull.Handle = 14;
            imageNull.transparent = 0x3aebca;
            imageNull.FromBitmap((Bitmap)Resources.EmptyIcon);
            mfa.Icons.Items.Add(14, imageNull);
            // game.Images.Images.Clea r();

            mfa.Author = game.author;
            mfa.Copyright = game.copyright;
            mfa.Company = "";
            mfa.Version = "";
            //TODO:Binary Files
            var displaySettings = mfa.DisplayFlags;
            var graphicSettings = mfa.GraphicFlags;
            var flags = game.header.Flags;
            var newFlags = game.header.NewFlags;
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
            for (int i = 0; i < game.frameitems.Keys.Count; i++)
            {
                var key = game.frameitems.Keys.ToArray()[i];
                var item = game.frameitems[key];
                var newItem = new MFAObjectInfo(null);
                if (item.ObjectType >= 32)
                {
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
                else
                {
                    FrameItems.Add(newItem.Handle, newItem);
                }
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


            Logger.Log($"Prepating to translate {game.frames.Count} frames");
            for (int a = 0; a < game.frames.Count; a++)
            {
                if (Core.parameters.Contains(a.ToString()))
                {

                }
                else
                {
                    var frame = game.frames[a];

                    if (frame.name == "") continue;
                    //if(frame.Palette==null|| frame.Events==null|| frame.Objects==null) continue;
                    var newFrame = new MFAFrame(null);
                    newFrame.Chunks = new MFAChunkList(null);//MFA.MFA.emptyFrameChunks;
                    newFrame.Handle = a;
                    if (!indexHandles.TryGetValue(a, out newFrame.Handle)) Logger.Log("Error while getting frame handle");

                    newFrame.Name = frame.name;
                    newFrame.SizeX = frame.width;
                    newFrame.SizeY = frame.height;

                    newFrame.Background = frame.background;
                    newFrame.FadeIn = frame.fadeIn != null ? ConvertTransition(frame.fadeIn) : null;
                    newFrame.FadeOut = frame.fadeOut != null ? ConvertTransition(frame.fadeOut) : null;
                    var mfaFlags = newFrame.Flags;
                    var originalFlags = frame.flags;

                    mfaFlags["GrabDesktop"] = originalFlags["GrabDesktop"];
                    mfaFlags["KeepDisplay"] = originalFlags["KeepDisplay"];
                    mfaFlags["BackgroundCollisions"] = originalFlags["TotalCollisionMask"];
                    mfaFlags["ResizeToScreen"] = originalFlags["ResizeAtStart"];
                    mfaFlags["ForceLoadOnCall"] = originalFlags["ForceLoadOnCall"];
                    mfaFlags["NoDisplaySurface"] = false;
                    mfaFlags["TimerBasedMovements"] = originalFlags["TimedMovements"];
                    newFrame.Flags = mfaFlags;
                    newFrame.MaxObjects = frame.events?.MaxObjects ?? 10000;
                    newFrame.Password = "";
                    newFrame.LastViewedX = 320;
                    newFrame.LastViewedY = 240;
                    if (frame.palette == null) continue;
                    newFrame.Palette = frame.palette ?? new List<Color>();
                    newFrame.StampHandle = 13;
                    newFrame.ActiveLayer = 0;
                    newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Left = frame.virtualRect?.left ?? 0;
                    newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Top = frame.virtualRect?.top ?? 0;
                    newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Right = frame.virtualRect?.right ?? frame.width;
                    newFrame.Chunks.GetOrCreateChunk<FrameVirtualRect>().Bottom = frame.virtualRect?.bottom ?? frame.height;
                    //LayerInfo
                    if (Settings.Old)
                    {

                        var tempLayer = new MFALayer(null);

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
                            var newLayer = new MFALayer(null);
                            newLayer.Name = layer.Name;
                            newLayer.Flags["HideAtStart"] = layer.Flags["ToHide"];
                            newLayer.Flags["Visible"] = true;
                            newLayer.Flags["NoBackground"] = layer.Flags["DoNotSaveBackground"];
                            newLayer.Flags["WrapHorizontally"] = layer.Flags["WrapHorizontally"];
                            newLayer.XCoefficient = layer.XCoeff;
                            newLayer.YCoefficient = layer.YCoeff;

                            newFrame.Layers.Add(newLayer);
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
                                var newInstance = new MFAObjectInstance((ByteReader)null);
                                newInstance.X = instance.x;
                                newInstance.Y = instance.y;
                                newInstance.Handle = i;//instance.handle;
                                if (instance.parentType != 0) newInstance.Flags = 8;
                                else newInstance.Flags = 0;
                                // newInstance.Flags = ((instance.FrameItem.Properties.Loader as ObjectCommon)?.Preferences?.flag ?? (uint)instance.FrameItem.Flags);
                                //newInstance.Flags = (uint)instance.flags;

                                newInstance.ParentType = (uint)instance.parentType;
                                newInstance.ItemHandle = (uint)(instance.objectInfo);
                                newInstance.ParentHandle = (uint)instance.parentHandle;
                                newInstance.Layer = (uint)(instance.layer);
                                newInstances.Add(newInstance);
                            }
                            else
                            {
                                Logger.Log("WARNING: OBJECT NOT FOUND");
                                break;
                            }
                        }
                    }


                    newFrame.Items = newFrameItems;
                    newFrame.Instances = newInstances;
                    newFrame.Folders = new List<MFAItemFolder>();
                    foreach (MFAObjectInfo newFrameItem in newFrame.Items)
                    {
                        var newFolder = new MFAItemFolder((ByteReader)null);
                        newFolder.isRetard = true;
                        newFolder.Items = new List<uint>() { (uint)newFrameItem.Handle };
                        newFrame.Folders.Add(newFolder);
                    }
                    //if(false)
                    {
                        newFrame.Events = new MFAEvents((ByteReader)null);
                        newFrame.Events.Items = new List<EventGroup>();
                        newFrame.Events.Objects = new List<EventObject>();
                        newFrame.Events._ifMFA = true;
                        newFrame.Events.Version = 1028;
                        //if(false)
                        if (frame.events != null)
                        {
                            foreach (var item in newFrame.Items)
                            {
                                var newObject = new EventObject((ByteReader)null);

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
                            foreach (Quailifer quailifer in frame.events.QualifiersList.Values)
                            {
                                break;
                                int newHandle = 0;
                                while (true)
                                {
                                    if (!newFrame.Items.Any(item => item.Handle == newHandle) &&
                                        !qualifiers.Keys.Any(item => item == newHandle)) break;
                                    newHandle++;

                                }
                                qualifiers.Add(newHandle, quailifer);
                                var qualItem = new EventObject(null as ByteReader);
                                qualItem.Handle = (uint)newHandle;
                                qualItem.SystemQualifier = (ushort)quailifer.Qualifier;
                                qualItem.Name = "";
                                qualItem.TypeName = "";
                                qualItem.ItemType = (ushort)quailifer.Type;
                                qualItem.ObjectType = 3;
                                newFrame.Events.Objects.Add(qualItem);

                            }
                            for (int eg = 0; eg < newFrame.Events.Items.Count; eg++)//foreach (EventGroup eventGroup in newFrame.Events.Items)
                            {
                                var eventGroup = newFrame.Events.Items[eg];
                                foreach (Action action in eventGroup.Actions)
                                {
                                    foreach (var quailifer in qualifiers)
                                    {
                                        if (quailifer.Value.ObjectInfo == action.ObjectInfo)
                                            action.ObjectInfo = quailifer.Key;
                                        foreach (var param in action.Items)
                                        {
                                            var objInfoFld = param?.Loader?.GetType()?.GetField("ObjectInfo");
                                            if (objInfoFld == null) continue;
                                            if ((int)objInfoFld?.GetValue(param?.Loader) ==
                                                quailifer.Value?.ObjectInfo)
                                                newFrame.Events.Items.Remove(eventGroup);
                                            param.Loader?.GetType().GetField("ObjectInfo")
                                                .SetValue(param.Loader, quailifer.Key);
                                        }
                                    }

                                }
                                foreach (Condition cond in eventGroup.Conditions)
                                {
                                    foreach (var quailifer in qualifiers)
                                    {
                                        if (quailifer.Value.ObjectInfo == cond.ObjectInfo)
                                            cond.ObjectInfo = quailifer.Key;
                                        foreach (var param in cond.Items)
                                        {
                                            var objInfoFld = param?.Loader?.GetType()?.GetField("ObjectInfo");
                                            if (objInfoFld == null) continue;
                                            if ((int)objInfoFld?.GetValue(param?.Loader) ==
                                                quailifer.Value?.ObjectInfo)
                                                param.Loader?.GetType().GetField("ObjectInfo")
                                                    .SetValue(param.Loader, quailifer.Key);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    if (Core.parameters.Contains(a.ToString()) == false)
                    {
                        Logger.Log($"Translating frame {frame.name} - {a}");
                        mfa.Frames.Add(newFrame);
                    }
                    else
                    {

                    }
                }
            }
            Settings.gameType = Settings.GameType.NORMAL;
            mfa.Write(new ByteWriter(new FileStream($"Dumps\\Decompiled.mfa", FileMode.Create)));

            static MFATransition ConvertTransition(Transition gameTrans)
            {
                var newName = "";
                newName = gameTrans.Name;
                newName = newName.ToLower();
                var mfaTrans = new MFATransition((ByteReader)null)
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
                var newItem = new MFAObjectInfo(null);
                newItem.Chunks = new MFAChunkList(null);
                newItem.Name = item.name;
                newItem.ObjectType = (int)item.ObjectType;
                newItem.Handle = item.handle;
                newItem.Transparent = 1;
                newItem.InkEffect = item.InkEffect;
                newItem.InkEffectParameter = (uint)item.InkEffectValue;
                newItem.AntiAliasing = 0;
                newItem.Flags = item.Flags;
                int type = 2;
                bool noicon = true;

                if (exyt == true)
                {
                    Extension ext = null;
                    foreach (var testExt in game.extensions.Items)
                    {
                        if (testExt.Handle == (int)item.ObjectType - 32) ext = testExt;
                    }
                    string extname = ext.Name;
                    switch (extname)
                    {
                        case "KcBoxA":
                            type = 1;
                            break;
                        case "kcpop":
                            type = 4;
                            break;
                        case "EasyScrollbar":
                            type = 9;
                            break;
                        case "InternalList":
                            type = 13;
                            break;
                        case "PopupMenu":
                            type = 7;
                            break;
                        case "RunInConsole":
                            type = 15;
                            break;
                        case "KcBoxB":
                            type = 10;
                            break;
                        case "TreeControl":
                            type = 5;
                            break;
                        case "kcinput":
                            type = 6;
                            break;
                        case "kcedit":
                            type = 2;
                            break;
                        case "kcriched":
                            type = 2;
                            break;
                        case "fontembed":
                            type = 18;
                            break;
                        case "kcfile":
                            type = 12;
                            break;
                        case "fcFolder":
                            type = 11;
                            break;
                        case "FileReadWrite":
                            //by default
                            break;
                        case "kcpica":
                            type = 19;
                            break;
                        case "kclist":
                            type = 20;
                            break;
                        case "kccombo":
                            type = 21;
                            break;
                        case "EditBoxSel":
                            type = 22;
                            break;
                        case "JSON_Object":
                            //by default
                            break;
                        case "CalcRect":
                            type = 23;
                            break;
                        case "IIF":
                            type = 24;
                            break;
                        case "StringReplace":
                            type = 25;
                            break;
                        case "ObjResize":
                            type = 26;
                            break;
                        case "xlua":
                            type = 3;
                            break;
                        case "kcini":
                            type = 16;
                            break;
                        case "INI++15":
                            type = 17;
                            break;
                        case "kcwctrl":
                            type = 14;
                            break;
                        case "KcButton":
                            type = 8;
                            break;
                        default:
                            type = 0;
                            Logger.Log(extname);
                            //System.Threading.Thread.Sleep(500);
                            break;

                    }
                        switch (type)
                        {
                            case 0:
                                noicon = true;
                                break;
                            case 1: //Active System Box
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageActiveSystemBox = new CCN.Chunks.Banks.Image(null);
                                imageActiveSystemBox.Handle = lastAllocatedHandleImg;
                                imageActiveSystemBox.FromBitmap(Resources.ActiveSystemBox);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageActiveSystemBox);
                                break;
                            case 2: //Editbox
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageEditbox = new CCN.Chunks.Banks.Image(null);
                                imageEditbox.Handle = lastAllocatedHandleImg;
                                imageEditbox.FromBitmap(Resources.EEEditbox);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageEditbox);
                                break;
                            case 3: //XLua
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageXLua = new CCN.Chunks.Banks.Image(null);
                                imageXLua.Handle = lastAllocatedHandleImg;
                                imageXLua.FromBitmap(Resources.XLua);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageXLua);
                                break;
                            case 4: //Popup Message object 2
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imagePopup = new CCN.Chunks.Banks.Image(null);
                                imagePopup.Handle = lastAllocatedHandleImg;
                                imagePopup.FromBitmap(Resources.PopupMessageobject2);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imagePopup);
                                break;
                            case 5: //Tree Control
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageTree = new CCN.Chunks.Banks.Image(null);
                                imageTree.Handle = lastAllocatedHandleImg;
                                imageTree.FromBitmap(Resources.TreeControl);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageTree);
                                break;
                            case 6: //Input object
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageInputObject = new CCN.Chunks.Banks.Image(null);
                                imageInputObject.Handle = lastAllocatedHandleImg;
                                imageInputObject.FromBitmap(Resources.InputObject);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageInputObject);
                                break;
                            case 7: //Popup Menu
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imagepm = new CCN.Chunks.Banks.Image(null);
                                imagepm.Handle = lastAllocatedHandleImg;
                                imagepm.FromBitmap(Resources.PopupMenu);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imagepm);
                                break;
                            case 8: //Button
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageBtn = new CCN.Chunks.Banks.Image(null);
                                imageBtn.Handle = lastAllocatedHandleImg;
                                imageBtn.FromBitmap(Resources.Button);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageBtn);
                                break;
                            case 9: //EasyScrollbar
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageEasyScrollbar = new CCN.Chunks.Banks.Image(null);
                                imageEasyScrollbar.Handle = lastAllocatedHandleImg;
                                imageEasyScrollbar.FromBitmap(Resources.EasyScrollbar);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageEasyScrollbar);
                                break;
                            case 10: //Background System Box
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageBActiveSystemBox = new CCN.Chunks.Banks.Image(null);
                                imageBActiveSystemBox.Handle = lastAllocatedHandleImg;
                                imageBActiveSystemBox.FromBitmap(Resources.ActiveSystemBox);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageBActiveSystemBox);
                                break;
                            case 11: //File-Folder object
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageFileF = new CCN.Chunks.Banks.Image(null);
                                imageFileF.Handle = lastAllocatedHandleImg;
                                imageFileF.FromBitmap(Resources.FileFolderObject);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageFileF);
                                break;
                            case 12: //File
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageFile = new CCN.Chunks.Banks.Image(null);
                                imageFile.Handle = lastAllocatedHandleImg;
                                imageFile.FromBitmap(Resources.File);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageFile);
                                break;
                            case 13: //Internal List Object
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageInternal = new CCN.Chunks.Banks.Image(null);
                                imageInternal.Handle = lastAllocatedHandleImg;
                                imageInternal.FromBitmap(Resources.InternalListObject);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageInternal);
                                break;
                            case 14: //Window Control
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageWindow = new CCN.Chunks.Banks.Image(null);
                                imageWindow.Handle = lastAllocatedHandleImg;
                                imageWindow.FromBitmap(Resources.WindowControl);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageWindow);
                                break;
                            case 15: //Execute in console
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageExecute = new CCN.Chunks.Banks.Image(null);
                                imageExecute.Handle = lastAllocatedHandleImg;
                                imageExecute.FromBitmap(Resources.ExecuteInConsole);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageExecute);
                                break;
                            case 16: //INI
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageINI = new CCN.Chunks.Banks.Image(null);
                                imageINI.Handle = lastAllocatedHandleImg;
                                imageINI.FromBitmap(Resources.Ini);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageINI);
                                break;
                            case 17: //INI++
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageINIP = new CCN.Chunks.Banks.Image(null);
                                imageINIP.Handle = lastAllocatedHandleImg;
                                imageINIP.FromBitmap(Resources.IniPLUS);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageINIP);
                                break;
                            case 18: //FontEmbedObject
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageFontEmbedObject = new CCN.Chunks.Banks.Image(null);
                                imageFontEmbedObject.Handle = lastAllocatedHandleImg;
                                imageFontEmbedObject.FromBitmap(Resources.FontEmbedObject);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageFontEmbedObject);
                                break;
                        case 19: //ActivePicture
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageActivePicture = new CCN.Chunks.Banks.Image(null);
                            imageActivePicture.Handle = lastAllocatedHandleImg;
                            imageActivePicture.FromBitmap(Resources.Active_Picture);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageActivePicture);
                            break;
                        case 20: //List
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageList = new CCN.Chunks.Banks.Image(null);
                            imageList.Handle = lastAllocatedHandleImg;
                            imageList.FromBitmap(Resources.List);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageList);
                            break;
                        case 21: //ComboBox
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageComboBox = new CCN.Chunks.Banks.Image(null);
                            imageComboBox.Handle = lastAllocatedHandleImg;
                            imageComboBox.FromBitmap(Resources.ComboBox);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageComboBox);
                            break;
                        case 22: //EditBoxSel
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageEditBoxSel = new CCN.Chunks.Banks.Image(null);
                            imageEditBoxSel.Handle = lastAllocatedHandleImg;
                            imageEditBoxSel.FromBitmap(Resources.EditBoxSel);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageEditBoxSel);
                            break;
                        case 23: //CalcRect
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageCalcRect = new CCN.Chunks.Banks.Image(null);
                            imageCalcRect.Handle = lastAllocatedHandleImg;
                            imageCalcRect.FromBitmap(Resources.CalcRect);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageCalcRect);
                            break;
                        case 24: //IIF
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageIIF = new CCN.Chunks.Banks.Image(null);
                            imageIIF.Handle = lastAllocatedHandleImg;
                            imageIIF.FromBitmap(Resources.IIF);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageIIF);
                            break;
                        case 25: //StringReplace
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageStringReplace = new CCN.Chunks.Banks.Image(null);
                            imageStringReplace.Handle = lastAllocatedHandleImg;
                            imageStringReplace.FromBitmap(Resources.StringReplace);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageStringReplace);
                            break;
                        case 26: //ObjResize
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                            var imageObjResize = new CCN.Chunks.Banks.Image(null);
                            imageObjResize.Handle = lastAllocatedHandleImg;
                            imageObjResize.FromBitmap(Resources.ObjResize);
                            mfa.Icons.Items.Add(lastAllocatedHandleImg, imageObjResize);
                            break;
                        default:
                            noicon = true;
                            break;
                        }
                }
                else if (exyt == false)
                {
                        switch (item.ObjectType)
                        {
                            case 0: //Quick backdrop
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageQB = new CCN.Chunks.Banks.Image(null);
                                imageQB.Handle = lastAllocatedHandleImg;
                                imageQB.FromBitmap(Resources.Backdrop);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageQB);
                                break;
                            case 1: //Backdrop
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageQBB = new CCN.Chunks.Banks.Image(null);
                                imageQBB.Handle = lastAllocatedHandleImg;
                                imageQBB.FromBitmap(Resources.Backdrop);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageQBB);
                                break;
                            case 2: //Active
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageAct = new CCN.Chunks.Banks.Image(null);
                                imageAct.Handle = lastAllocatedHandleImg;
                                //imageAct.transparent = game.images.Items[imgHandleAct].transparent;
                                imageAct.FromBitmap(Resources.Active);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageAct);
                                break;

                            case 3: //String
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageStr = new CCN.Chunks.Banks.Image(null);
                                imageStr.Handle = lastAllocatedHandleImg;
                                imageStr.FromBitmap(Resources.String);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageStr);
                                break;

                            case 4: //Question and Answer
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageQa = new CCN.Chunks.Banks.Image(null);
                                imageQa.Handle = lastAllocatedHandleImg;
                                imageQa.FromBitmap(Resources.QandA);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageQa);
                                break;

                            case 5: //Score
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageSc = new CCN.Chunks.Banks.Image(null);
                                imageSc.Handle = lastAllocatedHandleImg;
                                imageSc.FromBitmap(Resources.Score);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageSc);
                                break;

                            case 6: //Lives
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageLive = new CCN.Chunks.Banks.Image(null);
                                imageLive.Handle = lastAllocatedHandleImg;
                                imageLive.FromBitmap(Resources.Lives);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageLive);
                                break;

                            case 7: //Counter
                                FTDecompile.lastAllocatedHandleImg++;
                                var imageCntr = new CCN.Chunks.Banks.Image(null);
                                imageCntr.Handle = lastAllocatedHandleImg;
                                //imageCntr.transparent = game.images.Items[imgHandleCntr].transparent;
                                imageCntr.FromBitmap(Resources.Frrr);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageCntr);
                                break;

                            case 8: //Formatted Text
                            noicon = false;
                            FTDecompile.lastAllocatedHandleImg++;
                                var imageRTF = new CCN.Chunks.Banks.Image(null);
                                imageRTF.Handle = lastAllocatedHandleImg;
                                imageRTF.FromBitmap(Resources.Formatted_Text);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageRTF);
                                break;

                            case 9: //Sub-Application
                            noicon = false;
                                FTDecompile.lastAllocatedHandleImg++;
                                var imageSub = new CCN.Chunks.Banks.Image(null);
                                imageSub.Handle = lastAllocatedHandleImg;
                                imageSub.FromBitmap(Resources.SubApp);
                                mfa.Icons.Items.Add(lastAllocatedHandleImg, imageSub);
                                break;
                            default:
                                noicon = true;
                                break;
                        }
                }
                newItem.IconHandle = noicon ? 14 : lastAllocatedHandleImg;
                if (item.InkEffect != 1 && !Core.parameters.Contains("notrans"))
                {
                    newItem.Chunks.GetOrCreateChunk<Opacity>().Blend = item.blend;
                    newItem.Chunks.GetOrCreateChunk<Opacity>().RGBCoeff = item.rgbCoeff;
                }



                if (item.ObjectType == (int)Constants.ObjectType.QuickBackdrop)
                {
                    var backdropLoader = item.properties as Quickbackdrop;
                    var backdrop = new MFAQuickBackdrop((ByteReader)null);
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
                    backdrop.Image = backdropLoader.Shape.Image;
                    newItem.Loader = backdrop;

                }
                else if (item.ObjectType == (int)Constants.ObjectType.Backdrop)
                {
                    var backdropLoader = item.properties as Backdrop;
                    var backdrop = new MFABackdrop((ByteReader)null);
                    backdrop.ObstacleType = (uint)backdropLoader.ObstacleType;
                    backdrop.CollisionType = (uint)backdropLoader.CollisionType;
                    backdrop.Handle = backdropLoader.Image;
                    newItem.Loader = backdrop;
                }
                else
                {
                    var itemLoader = item?.properties as ObjectCommon;
                    if (itemLoader == null) throw new NotImplementedException("Null loader");
                    //CommonSection
                    var newObject = new ObjectLoader(null);
                    newObject.ObjectFlags = (int)(itemLoader.Flags.flag);
                    newObject.NewObjectFlags = (int)(itemLoader.NewFlags.flag);
                    newObject.BackgroundColor = itemLoader.BackColor;
                    newObject.Qualifiers = itemLoader._qualifiers;

                    newObject.Strings = new MFAValueList(null);//ConvertStrings(itemLoader.);
                    newObject.Values = new MFAValueList(null);//ConvertValue(itemLoader.Values);
                    newObject.Movements = new MFAMovements(null);
                    if (itemLoader.Movements == null)
                    {
                        var newMov = new MFAMovement(null);
                        newMov.Name = $"Movement #{0}";
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
                            var newMov = new MFAMovement(null);
                            newMov.Name = $"Movement #{j}";
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


                    newObject.Behaviours = new Behaviours(null);

                    if (item.ObjectType == (int)Constants.ObjectType.Active)
                    {
                        var active = new MFAActive(null);
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
                                var origAnim = animHeader.AnimationDict.ToArray()[j];
                                var newAnimation = new MFAAnimation(null);
                                newAnimation.Name = $"User Defined {j}";
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
                                            var newDirection = new MFAAnimationDirection(null);
                                            newDirection.MinSpeed = direction.MinSpeed;
                                            newDirection.MaxSpeed = direction.MaxSpeed;
                                            newDirection.Index = n;
                                            newDirection.Repeat = direction.Repeat;
                                            newDirection.BackTo = direction.BackTo;
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
                        var newExt = new MFAExtensionObject(null);
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
                            Extension ext = null;
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
                        var newText = new MFAText(null);
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
                            newText.Items = new List<MFAParagraph>(){new MFAParagraph(null)
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
                                var newPar = new MFAParagraph((ByteReader)null);
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
                        var lives = new MFALives(null);
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
                        var newCount = new MFACounter(null);
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
                        // if(counter==null) throw new NullReferenceException(nameof(counter));
                        // counter = null;
                        // shape = null;
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
                        /*if (Core.parameters.Contains("-nosubapp"))
                        {

                        }
                        else
                        {
                            var newSubApp = new MFASubApplication(null);
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
                                newSubApp.FrameNumber = itemLoader.SubApplication.FrameNumber;
                            }
                            catch (Exception)
                            {
                                newSubApp.FrameNumber = 1;
                            }
                            newItem.Loader = newSubApp;
                        }*/
                    } 
                    
                }
                //Logger.Log("Name: " + newItem.Name + ", Object type: " + newItem.ObjectType);
                return newItem;
            }
        }
    }
}