using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using CTFAK;
using CTFAK.FileReaders;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.Tools;
using CTFAK.Utils;

namespace Dumper;

internal class SortedImageDumper : IFusionTool
{
    private int imageNumber = 1;

    //Patched by Yunivers :3
    //Broken multiple times by Yunivers ;3
    public int[] Progress = { };
    int[] IFusionTool.Progress => Progress;
    public string Name => "Sorted Image Dumper";

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public void Execute(IFileReader reader)
    {
        var outPath = reader.GetGameData().Name ?? "Unknown Game";
        var rgx = new Regex("[^a-zA-Z0-9 -]");
        outPath = rgx.Replace(outPath, "").Trim(' ');
        var images = reader.GetGameData().Images.Items;
        var frames = reader.GetGameData().Frames;
        var objects = reader.GetGameData().FrameItems;
        float curframe = 0;
        float maxdone = 0;
        var objectsdone = 0;

        Logger.Log($"2.5+?: {Settings.TwoFivePlus}");

        foreach (var frame in frames)
        foreach (var instance in frame.Objects)
            maxdone++;

        foreach (var frame in frames)
        {
            var frameFolder = $"Dumps\\{outPath}\\Sorted Images\\[{curframe}] {Utils.ClearName(frame.Name)}\\";
            var retry = 0;
            Directory.CreateDirectory($"{frameFolder}[UNSORTED]");
            foreach (var instance in frame.Objects)
            {
                objectsdone++;
                var oi = objects[instance.ObjectInfo];
                Console.WriteLine("\n");
                if (oi.Properties is ObjectCommon loggercommon)
                    Logger.Log($"{frame.Name} | {loggercommon.Identifier} {oi.Name}");
                else if (oi.Properties is Backdrop)
                    Logger.Log($"{frame.Name} | BD {oi.Name}");
                else if (oi.Properties is Quickbackdrop)
                    Logger.Log($"{frame.Name} | QBD {oi.Name}");

                Console.WriteLine($"{(int)(objectsdone / maxdone * 100.0)}%");
                var objectFolder = frameFolder + Utils.ClearName(oi.Name) + "\\";
                if (oi.Properties is Backdrop bg)
                {
                    Directory.CreateDirectory(objectFolder);
                    while (retry < 5)
                        try
                        {
                            images[bg.Image].bitmap.Save($"{objectFolder}{oi.Name}.png");
                            images[bg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.Name}.png");
                            retry = 5;
                        }
                        catch
                        {
                            if (CTFAKCore.Parameters.Contains("-log"))
                                Logger.Log($"Failed to save \"{oi.Name}\", retrying {5 - retry} more time(s).");
                            retry++;
                        }

                    retry = 0;
                    imageNumber++;
                }
                else if (oi.Properties is Quickbackdrop qbg)
                {
                    Directory.CreateDirectory(objectFolder);
                    while (retry < 5)
                        try
                        {
                            images[qbg.Image].bitmap.Save($"{objectFolder}{oi.Name}.png");
                            images[qbg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.Name}.png");
                            retry = 5;
                        }
                        catch
                        {
                            if (CTFAKCore.Parameters.Contains("-log"))
                                Logger.Log($"Failed to save \"{oi.Name}\", retrying {5 - retry} more time(s).");
                            retry++;
                        }

                    retry = 0;
                    imageNumber++;
                }
                else if (oi.Properties is ObjectCommon common)
                {
                    if ((Settings.TwoFivePlus && common.Identifier == "SPRI") ||
                        (!Settings.TwoFivePlus && common.Parent.ObjectType == 2))
                    {
                        var cntrAnims = 0;
                        foreach (var anim in common.Animations?.AnimationDict)
                            if (anim.Value.DirectionDict?.Count > 0)
                                cntrAnims++;
                        foreach (var anim in common.Animations?.AnimationDict)
                        {
                            var animationFolder = "";
                            if (cntrAnims > 0) animationFolder = objectFolder + $"Animation {anim.Key}\\";
                            else animationFolder = objectFolder;

                            var cntrDirs = 0;
                            if (anim.Value.DirectionDict == null) continue;
                            foreach (var dir in anim.Value?.DirectionDict)
                                if (dir.Value.Frames.Count > 0)
                                    cntrDirs++;
                            foreach (var dir in anim.Value?.DirectionDict)
                            {
                                var directionFolder = "";

                                if (cntrDirs > 1) directionFolder = objectFolder + $"Direction {dir.Key}\\";
                                else if (cntrAnims > 1) directionFolder = animationFolder;
                                else directionFolder = objectFolder;
                                var frms = dir.Value.Frames;
                                for (var i = 0; i < frms.Count; i++)
                                {
                                    var frm = frms[i];
                                    Directory.CreateDirectory(directionFolder);
                                    while (retry < 5)
                                        try
                                        {
                                            images[frm].bitmap.Save($"{directionFolder}_{i}.png");
                                            images[frm].bitmap
                                                .Save(
                                                    $"{frameFolder}[UNSORTED]\\{oi.Name}_{anim.Key}-{dir.Key}_{i}.png");
                                            retry = 5;
                                        }
                                        catch
                                        {
                                            if (CTFAKCore.Parameters.Contains("-log"))
                                                Logger.Log(
                                                    $"Failed to save \"{oi.Name}\", retrying {5 - retry} more time(s).");
                                            retry++;
                                        }

                                    retry = 0;
                                    imageNumber++;
                                }
                            }
                        }
                    }
                    else if ((Settings.TwoFivePlus && common.Identifier == "CNTR") ||
                             (!Settings.TwoFivePlus && common.Parent.ObjectType == 7))
                    {
                        var counter = common.Counters;
                        if (counter == null) continue;
                        if (!(counter.DisplayType == 1 || counter.DisplayType == 4 || counter.DisplayType == 50))
                            continue;
                        foreach (var cntrFrm in counter.Frames)
                        {
                            var bmp = images[cntrFrm].bitmap;

                            Directory.CreateDirectory(objectFolder);
                            while (retry < 5)
                                try
                                {
                                    bmp.Save($"{objectFolder}{cntrFrm}.png");
                                    bmp.Save($"{frameFolder}[UNSORTED]\\{oi.Name}_{cntrFrm}.png");
                                    retry = 5;
                                }
                                catch
                                {
                                    if (CTFAKCore.Parameters.Contains("-log"))
                                        Logger.Log($"Failed to save \"{oi.Name}\", retrying {5 - retry} more time(s).");
                                    retry++;
                                }

                            retry = 0;
                            imageNumber++;
                        }
                    }
                }

                Progress = new int[2] { objectsdone, (int)maxdone };
            }

            curframe++;
        }
    }
}