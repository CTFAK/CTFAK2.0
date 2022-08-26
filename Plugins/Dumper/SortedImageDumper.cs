using CTFAK;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.FileReaders;
using CTFAK.Tools;
using CTFAK.Utils;
using System;
using System.Drawing;
using System.IO;

namespace Dumper
{
    class SortedImageDumper : IFusionTool
    {
        //Patched by Yunivers :3
        public string Name => "Sorted Image Dumper";
        int imageNumber = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Execute(IFileReader reader)
        {
            var outPath = reader.getGameData().name ?? "Unknown Game";
            var images = reader.getGameData().Images.Items;
            var frames = reader.getGameData().frames;
            var objects = reader.getGameData().frameitems;
            float curframe = 0;
            float maxdone = 0;
            int objectsdone = 0;

            foreach (var frame in frames)
                foreach (var instance in frame.objects)
                    maxdone++;

            foreach (var frame in frames)
            {
                string frameFolder = $"Dumps\\{outPath}\\Sorted Images\\[{curframe}] {Utils.ClearName(frame.name)}\\";
                int retry = 0;
                Directory.CreateDirectory($"{frameFolder}[UNSORTED]");
                foreach (var instance in frame.objects)
                {
                    objectsdone++;
                    var oi = objects[instance.objectInfo];
                    Console.WriteLine("\n");
                    if (oi.properties is ObjectCommon loggercommon)
                        Logger.Log($"{loggercommon.Identifier} {oi.name}");
                    else if (oi.properties is Backdrop)
                        Logger.Log($"BD {oi.name}");
                    else if (oi.properties is Quickbackdrop)
                        Logger.Log($"QBD {oi.name}");

                    Console.WriteLine($"{(int)(objectsdone / maxdone * 100.0)}%");
                    var objectFolder = frameFolder + Utils.ClearName(oi.name) + "\\";
                    if (oi.properties is Backdrop bg)
                    {
                        Directory.CreateDirectory(objectFolder);
                        while (retry < 5)
                        {

                            try
                            {
                                images[bg.Image].bitmap.Save($"{objectFolder}{oi.name}.png");
                                images[bg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}.png");
                                retry = 5;
                            }
                            catch
                            {
                                if (Core.parameters.Contains("-log"))
                                    Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).");
                                retry++;
                            }
                        }
                        retry = 0;
                        imageNumber++;
                    }
                    else if (oi.properties is Quickbackdrop qbg)
                    {
                        Directory.CreateDirectory(objectFolder);
                        while (retry < 5)
                        {

                            try
                            {
                                images[qbg.Image].bitmap.Save($"{objectFolder}{oi.name}.png");
                                images[qbg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}.png");
                                retry = 5;
                            }
                            catch
                            {
                                if (Core.parameters.Contains("-log"))
                                    Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).");
                                retry++;
                            }
                        }
                        retry = 0;
                        imageNumber++;
                    }
                    else if (oi.properties is ObjectCommon common)
                    {
                        if (Settings.twofiveplus && common.Identifier == "SPRI" || !Settings.twofiveplus && common.Parent.ObjectType == 2)
                        {
                            int cntrAnims = 0;
                            foreach (var anim in common.Animations?.AnimationDict)
                            {
                                if (anim.Value.DirectionDict?.Count > 0) cntrAnims++;
                            }
                            foreach (var anim in common.Animations?.AnimationDict)
                            {
                                string animationFolder = "";
                                if (cntrAnims > 0) animationFolder = objectFolder + $"Animation {anim.Key}\\";
                                else animationFolder = objectFolder;

                                int cntrDirs = 0;




                                if (anim.Value.DirectionDict == null) continue;
                                foreach (var dir in anim.Value?.DirectionDict)
                                {
                                    if (dir.Value.Frames.Count > 0) cntrDirs++;
                                }
                                foreach (var dir in anim.Value?.DirectionDict)
                                {
                                    string directionFolder = "";


                                    if (cntrDirs > 1) directionFolder = objectFolder + $"Direction {dir.Key}\\";
                                    else if (cntrAnims > 1) directionFolder = animationFolder;
                                    else directionFolder = objectFolder;
                                    var frms = dir.Value.Frames;
                                    for (int i = 0; i < frms.Count; i++)
                                    {
                                        var frm = frms[i];
                                        Directory.CreateDirectory(directionFolder);
                                        while (retry < 5)
                                        {

                                            try
                                            {
                                                images[frm].bitmap.Save($"{directionFolder}{frm}.png");
                                                images[frm].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}_{frm}.png");
                                                retry = 5;
                                            }
                                            catch
                                            {
                                                if (Core.parameters.Contains("-log"))
                                                    Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).");
                                                retry++;
                                            }
                                        }
                                        retry = 0;
                                        imageNumber++;
                                    }
                                }
                            }
                        }
                        else if (Settings.twofiveplus && common.Identifier == "CNTR" || !Settings.twofiveplus && common.Parent.ObjectType == 7)
                        {
                            var counter = common.Counters;
                            if (counter == null) break;
                            if (!(counter.DisplayType == 1 || counter.DisplayType == 4 || counter.DisplayType == 50)) continue;
                            foreach (var cntrFrm in counter.Frames)
                            {
                                Bitmap bmp = images[cntrFrm].bitmap;
                                var resultImage = new Bitmap(bmp.Width, bmp.Height);
                                Color TransparencyRGB = bmp.GetPixel(0, 0);
                                for (int w = 0; w < bmp.Width; w++)
                                    for (int h = 0; h < bmp.Height; h++)
                                    {
                                        var bm2Color = bmp.GetPixel(w, h);
                                        if (bm2Color != TransparencyRGB)
                                            bm2Color = System.Drawing.Color.FromArgb(255, bm2Color.R, bm2Color.G, bm2Color.B);
                                        resultImage.SetPixel(w, h, bm2Color);
                                    }
                                
                                Directory.CreateDirectory(objectFolder);
                                while (retry < 5)
                                {
                                    try
                                    {
                                        resultImage.Save($"{objectFolder}{cntrFrm}.png");
                                        resultImage.Save($"{frameFolder}[UNSORTED]\\{oi.name}_{cntrFrm}.png");
                                        retry = 5;
                                    }
                                    catch
                                    {
                                        if (Core.parameters.Contains("-log"))
                                            Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).");
                                        retry++;
                                    }
                                }
                                resultImage.Dispose();

                                retry = 0;
                                imageNumber++;
                            }
                        }
                    }
                }
                curframe++;
            }
        }
    }
}