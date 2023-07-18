using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.CCN.Chunks;
using CTFAK.EXE;
using CTFAK.MFA;
using CTFAK.MMFParser.EXE.Loaders;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using CTFAK.Memory;
using System.Security.Cryptography;
using CTFAK.Utils;
using static CTFAK.CCN.Chunks.Objects.ObjectInfo;
using System.Xml.Linq;
using System;
using CTFAK.MFA.MFAObjectLoaders;
using CTFAK.Core.CCN.Chunks;

namespace CTFAK.Core.MFA
{
    public class MFA2Pame
    {

        public static GameData ConvertMFA2Pame(MFAData mfa)
        {
            Logger.Log("Converting");
            GameData game = new GameData();
            game.runtimeVersion = mfa.MfaVersion;
            game.runtimeSubversion = mfa.MfaSubversion;
            game.productVersion = mfa.Product;
            game.productBuild = mfa.BuildVersion;

            game.name = mfa.Name;
            game.author = mfa.Author;
            game.copyright = mfa.Copyright;
            game.aboutText = mfa.Aboutbox;
            game.doc = mfa.Description;
            game.editorFilename = mfa.Path;
            game.targetFilename = mfa.BuildPath;
            game.menu = mfa.Menu;

            Logger.Log("Converting Headers");
            var MFAHeader = new AppHeader();
            MFAHeader.WindowWidth = mfa.WindowX;
            MFAHeader.WindowHeight = mfa.WindowY;
            MFAHeader.InitialScore = mfa.InitialScore;
            MFAHeader.InitialLives = mfa.InitialLifes;
            MFAHeader.NumberOfFrames = mfa.Frames.Count;
            MFAHeader.Flags["Maximize"] = mfa.DisplayFlags["MaximizedOnBoot"];
            MFAHeader.Flags["MDI"] = mfa.DisplayFlags["ResizeDisplay"];
            MFAHeader.Flags["FullscreenAtStart"] = mfa.DisplayFlags["FullscreenAtStart"];
            MFAHeader.Flags["FullscreenSwitch"] = mfa.DisplayFlags["AllowFullscreen"];
            MFAHeader.Flags["NoHeading"] = !mfa.DisplayFlags["Heading"];
            MFAHeader.Flags["MenuBar"] = mfa.DisplayFlags["MenuBar"];
            MFAHeader.Flags["MenuHidden"] = !mfa.DisplayFlags["MenuOnBoot"];
            MFAHeader.NewFlags["NoMinimizeBox"] = mfa.DisplayFlags["NoMinimize"];
            MFAHeader.NewFlags["NoMaximizeBox"] = mfa.DisplayFlags["NoMaximize"];
            MFAHeader.NewFlags["NoThickFrame"] = mfa.DisplayFlags["NoThickFrame"];
            MFAHeader.NewFlags["DisableClose"] = mfa.DisplayFlags["DisableClose"];
            MFAHeader.NewFlags["HiddenAtStart"] = mfa.DisplayFlags["HiddenAtStart"];
            MFAHeader.NewFlags["MDI"] = mfa.DisplayFlags["MDI"];
            MFAHeader.BorderColor = mfa.BorderColor;
            MFAHeader.FrameRate = mfa.FrameRate;
            MFAHeader.GraphicsMode = (short)mfa.GraphicMode;

            var MFAControls = new Controls();
            MFAControls.Items = new List<PlayerControl>();
            for (int i = 0; i < mfa.Controls.Items.Count; i++)
                MFAControls.Items.Add(new PlayerControl(new ByteReader(new byte[]{})));

            MFAHeader.Controls = MFAControls;
            game.header = MFAHeader;

            var MFAExtHeader = new ExtendedHeader();
            MFAExtHeader.Flags["DisableIME"] = mfa.GraphicFlags["DisableIME"];
            MFAExtHeader.Flags["ReduceCPUUsage"] = mfa.GraphicFlags["ReduceCPUUsage"];
            MFAExtHeader.Flags["PremultipliedAlpha"] = mfa.GraphicFlags["PremultipliedAlpha"];
            game.ExtHeader = MFAExtHeader;

            Logger.Log("Converting Fonts");
            if (mfa.Fonts != null)
                game.Fonts = mfa.Fonts;
            Logger.Log("Converting Sounds");
            if (mfa.Sounds != null)
                game.Sounds = mfa.Sounds;
            Logger.Log("Converting Music");
            if (mfa.Music != null)
                game.Music = mfa.Music;
            Logger.Log("Converting Images");
            if (mfa.Images != null)
                game.Images.Items = mfa.Images.Items;

            Logger.Log("Converting Frames");
            var MFAFrames = new List<Frame>();
            var MFAFrameItems = new Dictionary<int, ObjectInfo>();
            var MFAFrameHandles = new Dictionary<int, int>();
            int handle = 0;
            foreach (var frame in mfa.Frames)
            {
                Logger.Log($"Frame Found: {frame.Name}, {frame.SizeX}x{frame.SizeY}, {frame.Items.Count} objects.", true, ConsoleColor.Green);

                var newFrame = new Frame();
                newFrame.name = frame.Name;
                newFrame.width = frame.SizeX;
                newFrame.height = frame.SizeY;
                newFrame.background = frame.Background;
                var MFAEvnts = new Events();
                newFrame.flags = frame.Flags;
                var MFAObjects = new List<ObjectInstance>();
                var MFALayers = new Layers();
                MFALayers.Items = new List<Layer>();
                newFrame.palette = frame.Palette;
                var MFAFadeIn = new Transition();
                var MFAFadeOut = new Transition();
                var MFAVirtualRect = new VirtualRect();

                MFAEvnts.Items = frame.Events.Items;
                MFAEvnts.MaxObjects = frame.MaxObjects;
                foreach (var evnt in frame.Events.Items)
                    MFAEvnts.NumberOfConditions.Add(evnt.NumberOfConditions);

                newFrame.events = MFAEvnts;
                MFAFrameHandles.Add(handle, handle);
                handle++;

                foreach (var item in frame.Items)
                {
                    Logger.Log($"Object Found: {item.Name} on Frame {frame.Name} with loader {item.Loader}", true, ConsoleColor.DarkGreen);
                    var newItem = new ObjectInfo();
                    newItem.handle = item.Handle;
                    if (MFAFrameItems.ContainsKey(newItem.handle)) continue;
                    newItem.name = item.Name;

                    if (item.Loader is MFABackdrop)
                        newItem.properties = new Backdrop();
                    else if (item.Loader is MFAQuickBackdrop)
                    {
                        var oi = newItem.properties as MFAQuickBackdrop;
                        var Width = oi.Width;
                        var Height = oi.Height;
                        var Shape = oi.Shape;
                        var BS = oi.BorderSize;
                        var BC = oi.BorderColor;
                        var FT = oi.FillType;
                        var C1 = oi.Color1;
                        var C2 = oi.Color2;
                        var Flags = oi.Flags;
                        var Img = oi.Image;

                        newItem.properties = new Quickbackdrop();

                        var newShape = new Shape();
                        newShape.BorderSize = (short)BS;
                        newShape.BorderColor = BC;
                        newShape.ShapeType = (short)Shape;
                        newShape.FillType = (short)FT;
                        newShape.Color1 = C1;
                        newShape.Color2 = C2;
                        newShape.GradFlags = (short)Flags;
                        newShape.Image = (short)Img;

                        (newItem.properties as Quickbackdrop).Shape = newShape;
                    }
                    else
                    {
                        if (item.Loader is MFAActive)
                        {

                        }

                        newItem.properties = new ObjectCommon(newItem);
                    }
                    newItem.ObjectType = item.ObjectType;
                    newItem.Flags = item.Flags;
                    newItem.InkEffect = item.InkEffect;
                    newItem.InkEffectValue = (int)item.InkEffectParameter;
                    /*try
                    {
                        var shdrData = item.Chunks.GetOrCreateChunk<LayerShaderSettings>();
                        newItem.RGBCoeff = shdrData.RGBCoeff;
                        newItem.blend = shdrData.Blend;

                        if (shdrData.Shaders != null)
                        {
                            newItem.shaderData = new ObjectInfo.ShaderData();
                            newItem.shaderData.name = shdrData.Shaders[0].Name;
                            foreach (var param in shdrData.Shaders[0].Parameters)
                            {
                                var newParam = new ObjectInfo.ShaderParameter();
                                newParam.Name = param.Name;
                                newParam.Value = param.Value;
                                newParam.ValueType = param.ValueType;
                                newItem.shaderData.parameters.Add(newParam);
                            }
                        }
                    }
                    catch
                    {
                        newItem.RGBCoeff = Color.FromArgb(0, 255, 255, 255);
                        newItem.blend = 255;
                    }*/
                    
                    MFAFrameItems.Add(newItem.handle, newItem);
                }

                foreach (var item in frame.Instances)
                {
                    var newItem = new ObjectInstance();
                    newItem.handle = (ushort)item.Handle;
                    newItem.objectInfo = (ushort)item.ItemHandle;
                    newItem.x = item.X;
                    newItem.y = item.Y;
                    newItem.parentType = (short)item.ParentType;
                    newItem.layer = (short)item.Layer;
                    //newItem.flags = (short)item.Flags;
                    newItem.parentHandle = (short)item.ParentHandle;
                    MFAObjects.Add(newItem);
                }

                newFrame.objects = MFAObjects;

                foreach (var layer in frame.Layers)
                {
                    var newLayer = new Layer();
                    newLayer.Name = layer.Name;
                    newLayer.Flags["ToHide"] = layer.Flags["HideAtStart"];
                    newLayer.Flags["Visible"] = layer.Flags["Visible"];
                    newLayer.Flags["DoNotSaveBackground"] = layer.Flags["NoBackground"];
                    newLayer.Flags["WrapHorizontally"] = layer.Flags["WrapHorizontally"];
                    newLayer.XCoeff = layer.XCoefficient;
                    newLayer.YCoeff = layer.YCoefficient;
                    MFALayers.Items.Add(newLayer);
                }

                newFrame.layers = MFALayers;

                if (frame.FadeIn != null)
                {
                    MFAFadeIn.Module = frame.FadeIn.Id;
                    MFAFadeIn.Name = frame.FadeIn.Name;
                    MFAFadeIn.Duration = frame.FadeIn.Duration;
                    MFAFadeIn.Flags = frame.FadeIn.Flags;
                    MFAFadeIn.Color = frame.FadeIn.Color;
                    MFAFadeIn.ModuleFile = frame.FadeIn.Module;
                    MFAFadeIn.ParameterData = frame.FadeIn.ParameterData;
                    newFrame.fadeIn = MFAFadeIn;
                }


                if (frame.FadeOut != null)
                {
                    MFAFadeOut.Module = frame.FadeOut.Id;
                    MFAFadeOut.Name = frame.FadeOut.Name;
                    MFAFadeOut.Duration = frame.FadeOut.Duration;
                    MFAFadeOut.Flags = frame.FadeOut.Flags;
                    MFAFadeOut.Color = frame.FadeOut.Color;
                    MFAFadeOut.ModuleFile = frame.FadeOut.Module;
                    MFAFadeOut.ParameterData = frame.FadeOut.ParameterData;
                    newFrame.fadeOut = MFAFadeOut;
                }


                var VirtualRect = frame.Chunks.GetOrCreateChunk<FrameVirtualRect>();
                MFAVirtualRect.left = VirtualRect.Left;
                MFAVirtualRect.right = VirtualRect.Right;
                MFAVirtualRect.top = VirtualRect.Top;
                MFAVirtualRect.bottom = VirtualRect.Bottom;

                MFAFrames.Add(newFrame);
            }

            game.frames = MFAFrames;
            game.frameitems = MFAFrameItems;
            game.frameHandles = new FrameHandles();
            game.frameHandles.Items = MFAFrameHandles;

            Logger.Log("Converting Extensions");
            var MFAExtensions = new CTFAK.CCN.Chunks.Extensions();
            MFAExtensions.Items = new List<Extension>();
            foreach (var ext in mfa.Extensions)
            {
                var newExt = new Extension();
                newExt.Handle = (short)ext.Item1;
                newExt.Name = ext.Item2;
                newExt.MagicNumber = ext.Item4;
                newExt.SubType = ext.Item5;
                MFAExtensions.Items.Add(newExt);
            }

            game.extensions = MFAExtensions;

            game.packData = new PackData();
            game.shaders = new Shaders();
            game.globalStrings = new GlobalStrings();
            game.globalValues = new GlobalValues();
            game.extData = new ExtData();
            game.binaryFiles = mfa.binaryFiles;

            Logger.Log("Converting Global Values");
            foreach (var str in mfa.GlobalStrings.Items)
                game.globalStrings.Items.Add(str.Value.ToString());

            foreach (var str in mfa.GlobalValues.Items)
                game.globalValues.Items.Add(str.Value);

            return game;
        }
    }
}
