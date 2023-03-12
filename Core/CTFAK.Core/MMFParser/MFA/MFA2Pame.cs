using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.CCN.Chunks.Frame;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Shared.Events;
using CTFAK.Shared.Banks.ImageBank;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;

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

            game.Name = mfa.Name;
            game.Author = mfa.Author;
            game.Copyright = mfa.Copyright;
            game.AboutText = mfa.Aboutbox;
            game.Doc = mfa.Description;
            game.EditorFilename = mfa.Path;
            game.TargetFilename = mfa.BuildPath;
            game.Menu = mfa.Menu;

            Logger.Log("Converting Header");
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
                MFAControls.Items.Add(new PlayerControl(new ByteReader(new byte[] { })));

            MFAHeader.Controls = MFAControls;
            game.Header = MFAHeader;

            Logger.Log("Converting Fonts");
            game.Fonts = mfa.Fonts;
            Logger.Log("Converting Sounds");
            game.Sounds = mfa.Sounds;
            Logger.Log("Converting Music");
            game.Music = mfa.Music;
            Logger.Log("Converting Images");
            game.Images = new ImageBank();
            game.Images.Items = mfa.Images.Items;

            Logger.Log("Converting Frames");
            var MFAFrames = new List<Frame>();
            var MFAFrameItems = new Dictionary<int, ObjectInfo>();
            foreach (var frame in mfa.Frames)
            {
                Logger.Log($"Frame Found: {frame.Name}, {frame.SizeX}x{frame.SizeY}, {frame.Items.Count} objects.");

                var newFrame = new Frame();
                newFrame.Name = frame.Name;
                newFrame.Width = frame.SizeX;
                newFrame.Height = frame.SizeY;
                newFrame.Background = frame.Background;
                var MFAEvnts = new Events();
                newFrame.Flags = frame.Flags;
                var MFAObjects = new List<ObjectInstance>();
                var MFALayers = new Layers();
                MFALayers.Items = new List<Layer>();
                newFrame.Palette = frame.Palette;
                var MFAFadeIn = new Transition();
                var MFAFadeOut = new Transition();
                var MFAVirtualRect = new VirtualRect();

                MFAEvnts.Items = frame.Events.Items;
                MFAEvnts.MaxObjects = frame.MaxObjects;
                foreach (var evnt in frame.Events.Items)
                    MFAEvnts.NumberOfConditions.Add(evnt.NumberOfConditions);

                newFrame.Events = MFAEvnts;

                foreach (var item in frame.Items)
                {
                    Logger.Log($"Object Found: {item.Name} on Frame {frame.Name} with loader {item.Loader}");
                    var newItem = new ObjectInfo();
                    newItem.Handle = item.Handle;
                    if (MFAFrameItems.ContainsKey(newItem.Handle)) continue;
                    newItem.Name = item.Name;

                    if (item.Loader is MFABackdrop)
                        newItem.Properties = new Backdrop();
                    else if (item.Loader is MFAQuickBackdrop)
                    {
                        var oi = newItem.Properties as MFAQuickBackdrop;
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

                        newItem.Properties = new Quickbackdrop();

                        var newShape = new Shape();
                        newShape.BorderSize = (short)BS;
                        newShape.BorderColor = BC;
                        newShape.ShapeType = (short)Shape;
                        newShape.FillType = (short)FT;
                        newShape.Color1 = C1;
                        newShape.Color2 = C2;
                        newShape.GradFlags = (short)Flags;
                        newShape.Image = (short)Img;

                        (newItem.Properties as Quickbackdrop).Shape = newShape;
                    }
                    else
                    {
                        if (item.Loader is MFAActive)
                        {

                        }

                        newItem.Properties = new ObjectCommon(newItem);
                    }
                    newItem.ObjectType = item.ObjectType;
                    newItem.Flags = item.Flags;
                    newItem.InkEffect = item.InkEffect;
                    newItem.InkEffectValue = (int)item.InkEffectParameter;
                    try
                    {
                        var shdrData = item.Chunks.GetOrCreateChunk<ShaderSettings>();
                        newItem.RgbCoeff = shdrData.RGBCoeff;
                        newItem.Blend = shdrData.Blend;

                        if (shdrData.Shaders != null)
                        {
                            newItem.ShaderData = new ShaderData();
                            newItem.ShaderData.Name = shdrData.Shaders[0].Name;
                            foreach (var param in shdrData.Shaders[0].Parameters)
                            {
                                var newParam = new MMFParser.CCN.Chunks.Objects.ShaderParameter();
                                newParam.Name = param.Name;
                                newParam.Value = param.Value;
                                newParam.ValueType = param.ValueType;
                                newItem.ShaderData.Parameters.Add(newParam);
                            }
                        }
                    }
                    catch
                    {
                        newItem.RgbCoeff = Color.FromArgb(0, 255, 255, 255);
                        newItem.Blend = 255;
                    }

                    MFAFrameItems.Add(newItem.Handle, newItem);
                }

                foreach (var item in frame.Instances)
                {
                    var newItem = new ObjectInstance();
                    newItem.Handle = (ushort)item.Handle;
                    newItem.ObjectInfo = (ushort)item.ItemHandle;
                    newItem.X = item.X;
                    newItem.Y = item.Y;
                    newItem.ParentType = (short)item.ParentType;
                    newItem.Layer = (short)item.Layer;
                    newItem.Flags = (short)item.Flags;
                    newItem.ParentHandle = (short)item.ParentHandle;
                    MFAObjects.Add(newItem);
                }

                newFrame.Objects = MFAObjects;

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

                newFrame.Layers = MFALayers;

                if (frame.FadeIn != null)
                {
                    MFAFadeIn.Module = frame.FadeIn.Id;
                    MFAFadeIn.Name = frame.FadeIn.Name;
                    MFAFadeIn.Duration = frame.FadeIn.Duration;
                    MFAFadeIn.Flags = frame.FadeIn.Flags;
                    MFAFadeIn.Color = frame.FadeIn.Color;
                    MFAFadeIn.ModuleFile = frame.FadeIn.Module;
                    MFAFadeIn.ParameterData = frame.FadeIn.ParameterData;
                }

                newFrame.FadeIn = MFAFadeIn;

                if (frame.FadeOut != null)
                {
                    MFAFadeOut.Module = frame.FadeOut.Id;
                    MFAFadeOut.Name = frame.FadeOut.Name;
                    MFAFadeOut.Duration = frame.FadeOut.Duration;
                    MFAFadeOut.Flags = frame.FadeOut.Flags;
                    MFAFadeOut.Color = frame.FadeOut.Color;
                    MFAFadeOut.ModuleFile = frame.FadeOut.Module;
                    MFAFadeOut.ParameterData = frame.FadeOut.ParameterData;
                }

                newFrame.FadeOut = MFAFadeOut;

                var VirtualRect = frame.Chunks.GetOrCreateChunk<FrameVirtualRect>();
                MFAVirtualRect.Left = VirtualRect.Left;
                MFAVirtualRect.Right = VirtualRect.Right;
                MFAVirtualRect.Top = VirtualRect.Top;
                MFAVirtualRect.Bottom = VirtualRect.Bottom;

                MFAFrames.Add(newFrame);
            }

            game.Frames = MFAFrames;
            game.FrameItems = MFAFrameItems;
            game.FrameHandles = new FrameHandles();

            Logger.Log("Converting Extensions");
            var MFAExtensions = new Extensions();
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

            game.Extensions = MFAExtensions;

            game.PackData = new PackData();
            game.Shaders = new Shaders();
            game.GlobalStrings = new GlobalStrings();
            game.GlobalValues = new GlobalValues();
            game.ExtData = new ExtData();
            game.BinaryFiles = mfa.binaryFiles;

            Logger.Log("Converting Global Values");
            foreach (var str in mfa.GlobalStrings.Items)
                game.GlobalStrings.Items.Add(str.Value.ToString());

            foreach (var str in mfa.GlobalValues.Items)
                game.GlobalValues.Items.Add(str.Value);

            return game;
        }
    }
}