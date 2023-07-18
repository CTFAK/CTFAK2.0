using CTFAK;
using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Tools;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dumper
{
    class SortedImageDumper : IFusionTool
    {
        // Patched by Yunivers :3
        // Broken multiple times by Yunivers ;3
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Sorted Image Dumper";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Execute(IFileReader reader)
        {
            var outPath = reader.getGameData().name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            var images = reader.getGameData().Images.Items;
            var frames = reader.getGameData().frames;
            var objects = reader.getGameData().frameitems;
            List<int> LostandFound = new();
            int imageNumber = 1;
            float curframe = 0;
            float maxdone = 0;
            int objectsdone = 0;

            if (Settings.TwoFivePlus)
                Logger.Log($"2.5+\n");
            if (Settings.F3 || Settings.Fusion3Seed)
                Logger.Log($"Fusion 3" + (Settings.Fusion3Seed ? " Seeded" : "") + "\n");
            if (Settings.Android)
                Logger.Log($"Android\n");
            if (Settings.isMFA)
                Logger.Log($"MFA\n");
            if (Settings.iOS)
                Logger.Log($"iOS\n");
            if (Settings.Old)
                Logger.Log($"Old\n");
            Console.WriteLine("");

            foreach (var frame in frames)
                foreach (var instance in frame.objects)
                    maxdone++;

            foreach (var frame in frames)
            {
                string frameFolder = $"Dumps\\{outPath}\\Sorted Images\\[{curframe}] {Utils.ClearName(frame.name)}\\";
                int retry = 0;
                Task[] tasks = new Task[frame.objects.Count];
                int i = 0;
                foreach (var instance in frame.objects)
                {
                    Directory.CreateDirectory($"{frameFolder}[UNSORTED]");
                    var oi = objects[instance.objectInfo];
                    var newTask = new Task(() =>
                    {
                        var objectFolder = frameFolder + Utils.ClearName(oi.name) + "\\";
                        int retrysave = 0;
                    RETRY_SAVE:
                        try
                        {
                            if (oi.properties is Backdrop bg)
                            {
                                Directory.CreateDirectory(objectFolder);
                                while (retry < 5)
                                {
                                    try
                                    {
                                        if (!LostandFound.Contains(bg.Image))
                                            LostandFound.Add(bg.Image);
                                        images[bg.Image].bitmap.Save($"{objectFolder}{oi.name}.png");
                                        images[bg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}.png");
                                        retry = 5;
                                    }
                                    catch
                                    {
                                        if (CTFAKCore.parameters.Contains("-log"))
                                            Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).", false);
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
                                        if (!LostandFound.Contains(qbg.Image))
                                            LostandFound.Add(qbg.Image);
                                        images[qbg.Image].bitmap.Save($"{objectFolder}{oi.name}.png");
                                        images[qbg.Image].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}.png");
                                        retry = 5;
                                    }
                                    catch
                                    {
                                        if (CTFAK.CTFAKCore.parameters.Contains("-log"))
                                            Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).", false);
                                        retry++;
                                    }
                                }
                                retry = 0;
                                imageNumber++;
                            }
                            else if (oi.properties is ObjectCommon common)
                            {
                                if ((Settings.TwoFivePlus || Settings.F3 || Settings.Fusion3Seed) && common.Identifier == "SPRI" || !Settings.TwoFivePlus && common.Parent.ObjectType == 2)
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
                                                        if (!LostandFound.Contains(frm))
                                                            LostandFound.Add(frm);
                                                        images[frm].bitmap.Save($"{directionFolder}_{i}.png");
                                                        images[frm].bitmap.Save($"{frameFolder}[UNSORTED]\\{oi.name}_{anim.Key}-{dir.Key}_{i}.png");
                                                        retry = 5;
                                                    }
                                                    catch
                                                    {
                                                        if (CTFAK.CTFAKCore.parameters.Contains("-log"))
                                                            Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).", false);
                                                        retry++;
                                                    }
                                                }
                                                retry = 0;
                                                imageNumber++;
                                            }
                                        }
                                    }
                                }
                                else if ((Settings.TwoFivePlus || Settings.F3 || Settings.Fusion3Seed) && common.Identifier == "CNTR" && common.Counters != null ||
                                         !Settings.TwoFivePlus && common.Parent.ObjectType == 7 && common.Counters != null)
                                {
                                    var counter = common.Counters;
                                    if (!(counter.DisplayType != 1 && counter.DisplayType != 4 && counter.DisplayType != 50))
                                    {
                                        foreach (var cntrFrm in counter.Frames)
                                        {
                                            Bitmap bmp = images[cntrFrm].bitmap;

                                            Directory.CreateDirectory(objectFolder);
                                            while (retry < 5)
                                            {
                                                try
                                                {
                                                    if (!LostandFound.Contains(cntrFrm))
                                                        LostandFound.Add(cntrFrm);
                                                    bmp.Save($"{objectFolder}{cntrFrm}.png");
                                                    bmp.Save($"{frameFolder}[UNSORTED]\\{oi.name}_{cntrFrm}.png");
                                                    retry = 5;
                                                }
                                                catch
                                                {
                                                    if (CTFAK.CTFAKCore.parameters.Contains("-log"))
                                                        Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).", false);
                                                    retry++;
                                                }
                                            }

                                            retry = 0;
                                            imageNumber++;
                                        }
                                    }
                                }
                                else if (((Settings.TwoFivePlus || Settings.F3 || Settings.Fusion3Seed) && common.Identifier == "TEXT" || !Settings.TwoFivePlus && common.Parent.ObjectType == 3) && common.Text != null && CTFAKCore.parameters.Contains("-sorteddumpstrings"))
                                {
                                    Directory.CreateDirectory(objectFolder);
                                    var paragraphs = common.Text;
                                    while (retry < 5)
                                    {
                                        try
                                        {
                                            List<string> output = new List<string>();
                                            foreach (var text in paragraphs.Items)
                                                output.Add(text.Value);

                                            File.WriteAllLines($"{objectFolder}Paragraphs.txt", output);
                                            File.WriteAllLines($"{frameFolder}[UNSORTED]\\{oi.name}.txt", output);
                                            retry = 5;
                                        }
                                        catch (Exception ex)
                                        {
                                            if (CTFAK.CTFAKCore.parameters.Contains("-log"))
                                                Logger.Log($"Failed to save \"{oi.name}\", retrying {5 - retry} more time(s).", false);
                                            retry++;
                                        }
                                    }
                                    retry = 0;
                                }
                            }

                            if (oi.properties is ObjectCommon loggercommon)
                                Console.WriteLine($"{frame.name} | {loggercommon.Identifier} {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");
                            else if (oi.properties is Backdrop)
                                Console.WriteLine($"{frame.name} | BD {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");
                            else if (oi.properties is Quickbackdrop)
                                Console.WriteLine($"{frame.name} | QBD {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");

                            objectsdone++;
                            Progress = new int[2] { objectsdone, (int)maxdone };
                        }
                        catch (Exception exc)
                        {
                            retrysave++;
                            if (retrysave <= 10)
                                goto RETRY_SAVE;
                            else
                            {
                                if (oi.properties is ObjectCommon loggercommon)
                                    Console.WriteLine($"{frame.name} | {loggercommon.Identifier} {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");
                                else if (oi.properties is Backdrop)
                                    Console.WriteLine($"{frame.name} | BD {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");
                                else if (oi.properties is Quickbackdrop)
                                    Console.WriteLine($"{frame.name} | QBD {oi.name}\n{(int)(objectsdone / maxdone * 100.0)}%\n");

                                objectsdone++;
                                Progress = new int[2] { objectsdone, (int)maxdone };
                            }
                        }
                    });

                    tasks[i] = newTask;
                    newTask.Start();
                    i++;
                }
                curframe++;
                foreach (var item in tasks)
                {
                    item.Wait();
                }
            }

            int retrysave2 = 0;
            int savedvar = 0;
            RETRY_SAVE2:
            try
            {
                string lafFolder = $"Dumps\\{outPath}\\Sorted Images\\[~Lost and Found~]\\";
                foreach (var img in images)
                {
                    savedvar = img.Key;
                    if (LostandFound.Contains(savedvar)) continue;
                    Directory.CreateDirectory(lafFolder);
                    img.Value.bitmap.Save($"{lafFolder}{savedvar}.png");
                    LostandFound.Add(savedvar);

                    Console.WriteLine($"Lost and Found | {img.Key}");
                }
            }
            catch (Exception exc)
            {
                retrysave2++;
                if (retrysave2 <= 10)
                    goto RETRY_SAVE2;
            }
        }
    }
}