using System.Collections.Generic;
using System.Drawing;
using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.CCN.Chunks.Frame;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Common.Banks;
using CTFAK.MMFParser.Common.Events;
using CTFAK.MMFParser.MFA;
using CTFAK.MMFParser.MFA.MFAObjectLoaders;
using CTFAK.Utils;
using ShaderParameter = CTFAK.MMFParser.CCN.Chunks.Objects.ShaderParameter;

namespace CTFAK.MMFParser.MMFUtils;

public class Mfa2Pame
{
    public static GameData Convert(MFAData mfa)
    {
        Logger.Log("Converting");
        var game = new GameData();
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
        var mfaHeader = new AppHeader();
        mfaHeader.WindowWidth = mfa.WindowX;
        mfaHeader.WindowHeight = mfa.WindowY;
        mfaHeader.InitialScore = mfa.InitialScore;
        mfaHeader.InitialLives = mfa.InitialLifes;
        mfaHeader.NumberOfFrames = mfa.Frames.Count;
        mfaHeader.Flags["Maximize"] = mfa.DisplayFlags["MaximizedOnBoot"];
        mfaHeader.Flags["MDI"] = mfa.DisplayFlags["ResizeDisplay"];
        mfaHeader.Flags["FullscreenAtStart"] = mfa.DisplayFlags["FullscreenAtStart"];
        mfaHeader.Flags["FullscreenSwitch"] = mfa.DisplayFlags["AllowFullscreen"];
        mfaHeader.Flags["NoHeading"] = !mfa.DisplayFlags["Heading"];
        mfaHeader.Flags["MenuBar"] = mfa.DisplayFlags["MenuBar"];
        mfaHeader.Flags["MenuHidden"] = !mfa.DisplayFlags["MenuOnBoot"];
        mfaHeader.NewFlags["NoMinimizeBox"] = mfa.DisplayFlags["NoMinimize"];
        mfaHeader.NewFlags["NoMaximizeBox"] = mfa.DisplayFlags["NoMaximize"];
        mfaHeader.NewFlags["NoThickFrame"] = mfa.DisplayFlags["NoThickFrame"];
        mfaHeader.NewFlags["DisableClose"] = mfa.DisplayFlags["DisableClose"];
        mfaHeader.NewFlags["HiddenAtStart"] = mfa.DisplayFlags["HiddenAtStart"];
        mfaHeader.NewFlags["MDI"] = mfa.DisplayFlags["MDI"];
        mfaHeader.BorderColor = mfa.BorderColor;
        mfaHeader.FrameRate = mfa.FrameRate;
        mfaHeader.GraphicsMode = (short)mfa.GraphicMode;

        var mfaControls = new Controls();
        mfaControls.Items = new List<PlayerControl>();
        for (var i = 0; i < mfa.Controls.Items.Count; i++)
            mfaControls.Items.Add(new PlayerControl(new ByteReader(new byte[] { })));

        mfaHeader.Controls = mfaControls;
        game.Header = mfaHeader;

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
        var mfaFrames = new List<Frame>();
        var mfaFrameItems = new Dictionary<int, ObjectInfo>();
        foreach (var frame in mfa.Frames)
        {
            Logger.Log($"Frame Found: {frame.Name}, {frame.SizeX}x{frame.SizeY}, {frame.Items.Count} objects.");

            var newFrame = new Frame();
            newFrame.Name = frame.Name;
            newFrame.Width = frame.SizeX;
            newFrame.Height = frame.SizeY;
            newFrame.Background = frame.Background;
            var mfaEvents = new Events();
            newFrame.Flags = frame.Flags;
            var mfaObjects = new List<ObjectInstance>();
            var mfaLayers = new Layers();
            mfaLayers.Items = new List<Layer>();
            newFrame.Palette = frame.Palette;
            var mfaFadeIn = new Transition();
            var mfaFadeOut = new Transition();
            var mfaVirtualRect = new VirtualRect();

            mfaEvents.Items = frame.Events.Items;
            mfaEvents.MaxObjects = frame.MaxObjects;
            foreach (var evnt in frame.Events.Items)
                mfaEvents.NumberOfConditions.Add(evnt.NumberOfConditions);

            newFrame.Events = mfaEvents;

            foreach (var item in frame.Items)
            {
                Logger.Log($"Object Found: {item.Name} on Frame {frame.Name} with loader {item.Loader}");
                var newItem = new ObjectInfo();
                newItem.Handle = item.Handle;
                if (mfaFrameItems.ContainsKey(newItem.Handle)) continue;
                newItem.Name = item.Name;

                if (item.Loader is MFABackdrop)
                {
                    newItem.Properties = new Backdrop();
                }
                else if (item.Loader is MFAQuickBackdrop)
                {
                    var oi = newItem.Properties as MFAQuickBackdrop;
                    var width = oi.Width;
                    var height = oi.Height;
                    var shape = oi.Shape;
                    var bs = oi.BorderSize;
                    var bc = oi.BorderColor;
                    var ft = oi.FillType;
                    var c1 = oi.Color1;
                    var c2 = oi.Color2;
                    var flags = oi.Flags;
                    var img = oi.Image;

                    newItem.Properties = new Quickbackdrop();

                    var newShape = new Shape();
                    newShape.BorderSize = (short)bs;
                    newShape.BorderColor = bc;
                    newShape.ShapeType = (short)shape;
                    newShape.FillType = (short)ft;
                    newShape.Color1 = c1;
                    newShape.Color2 = c2;
                    newShape.GradFlags = (short)flags;
                    newShape.Image = (short)img;

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
                            var newParam = new ShaderParameter();
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

                mfaFrameItems.Add(newItem.Handle, newItem);
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
                mfaObjects.Add(newItem);
            }

            newFrame.Objects = mfaObjects;

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
                mfaLayers.Items.Add(newLayer);
            }

            newFrame.Layers = mfaLayers;

            if (frame.FadeIn != null)
            {
                mfaFadeIn.Module = frame.FadeIn.Id;
                mfaFadeIn.Name = frame.FadeIn.Name;
                mfaFadeIn.Duration = frame.FadeIn.Duration;
                mfaFadeIn.Flags = frame.FadeIn.Flags;
                mfaFadeIn.Color = frame.FadeIn.Color;
                mfaFadeIn.ModuleFile = frame.FadeIn.Module;
                mfaFadeIn.ParameterData = frame.FadeIn.ParameterData;
            }

            newFrame.FadeIn = mfaFadeIn;

            if (frame.FadeOut != null)
            {
                mfaFadeOut.Module = frame.FadeOut.Id;
                mfaFadeOut.Name = frame.FadeOut.Name;
                mfaFadeOut.Duration = frame.FadeOut.Duration;
                mfaFadeOut.Flags = frame.FadeOut.Flags;
                mfaFadeOut.Color = frame.FadeOut.Color;
                mfaFadeOut.ModuleFile = frame.FadeOut.Module;
                mfaFadeOut.ParameterData = frame.FadeOut.ParameterData;
            }

            newFrame.FadeOut = mfaFadeOut;

            var virtualRect = frame.Chunks.GetOrCreateChunk<FrameVirtualRect>();
            mfaVirtualRect.Left = virtualRect.Left;
            mfaVirtualRect.Right = virtualRect.Right;
            mfaVirtualRect.Top = virtualRect.Top;
            mfaVirtualRect.Bottom = virtualRect.Bottom;

            mfaFrames.Add(newFrame);
        }

        game.Frames = mfaFrames;
        game.FrameItems = mfaFrameItems;
        game.FrameHandles = new FrameHandles();

        Logger.Log("Converting Extensions");
        var mfaExtensions = new Extensions();
        mfaExtensions.Items = new List<Extension>();
        foreach (var ext in mfa.Extensions)
        {
            var newExt = new Extension();
            newExt.Handle = (short)ext.Item1;
            newExt.Name = ext.Item2;
            newExt.MagicNumber = ext.Item4;
            newExt.SubType = ext.Item5;
            mfaExtensions.Items.Add(newExt);
        }

        game.Extensions = mfaExtensions;

        game.PackData = new PackData();
        game.Shaders = new Shaders();
        game.GlobalStrings = new GlobalStrings();
        game.GlobalValues = new GlobalValues();
        game.ExtData = new ExtData();
        game.BinaryFiles = mfa.BinaryFiles;

        Logger.Log("Converting Global Values");
        foreach (var str in mfa.GlobalStrings.Items)
            game.GlobalStrings.Items.Add(str.Value.ToString());

        foreach (var str in mfa.GlobalValues.Items)
            game.GlobalValues.Items.Add(str.Value);

        return game;
    }
}