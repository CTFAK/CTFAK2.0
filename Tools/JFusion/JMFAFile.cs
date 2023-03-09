using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.MFA;
using CTFAK.MMFParser.MFA;
using CTFAK.MMFParser.Shared.Banks;
using CTFAK.Shared.Banks.ImageBank;
using Newtonsoft.Json;

namespace JFusion;

public class JMFAFile
{
    public string aboutBox;
    public string author;
    public int buildType;
    public int buildVersion;
    public string company;
    public string copyright;
    public string description;

    public uint displayFlags;

    [JsonIgnore] public Dictionary<string, JMFAFrameData> frameData = new();
    public int frameRate;

    [JsonIgnore] public List<JMFAFrame> frames = new();
    public uint graphicFlags;
    public string helpFile;
    [JsonIgnore] public List<JMfAImage> icons = new();
    [JsonIgnore] public List<JMfAImage> images = new();
    public int langId;

    public int mfaBuild;
    public string name;
    public int product;
    public string version;
    public int windowHeight;

    public int windowWidth;

    public static JMFAFile FromMFA(MFAData mfa)
    {
        var fProj = new JMFAFile();
        fProj.name = mfa.Name;
        fProj.description = mfa.Description;
        fProj.author = mfa.Author;
        fProj.copyright = mfa.Copyright;
        fProj.company = mfa.Company;
        fProj.version = mfa.Version;
        fProj.helpFile = mfa.HelpFile;
        fProj.aboutBox = mfa.Aboutbox;
        fProj.windowWidth = mfa.WindowX;
        fProj.windowHeight = mfa.WindowY;
        fProj.displayFlags = mfa.DisplayFlags.flag;
        fProj.graphicFlags = mfa.GraphicFlags.flag;
        fProj.frameRate = mfa.FrameRate;
        fProj.buildType = mfa.BuildType;
        fProj.mfaBuild = mfa.MfaBuild;
        fProj.product = mfa.Product;
        fProj.buildVersion = mfa.BuildType;
        fProj.langId = mfa.LangId;
        foreach (var mfaFrame in mfa.Frames)
        {
            fProj.frames.Add(JMFAFrame.FromMFA(mfaFrame));
            fProj.frameData.Add(mfaFrame.Name,
                new JMFAFrameData { handle = mfaFrame.Handle, index = mfa.Frames.IndexOf(mfaFrame) });
        }

        foreach (var imagePair in mfa.Images.Items)
            fProj.images.Add(new JMfAImage
            {
                Handle = imagePair.Value.Handle,
                ActionX = imagePair.Value.ActionX,
                ActionY = imagePair.Value.ActionY,
                HotspotX = imagePair.Value.HotspotX,
                HotspotY = imagePair.Value.HotspotY,
                Flags = imagePair.Value.Flags.flag,
                bmp = imagePair.Value.bitmap
            });
        foreach (var imagePair in mfa.Icons.Items)
            fProj.icons.Add(new JMfAImage
            {
                Handle = imagePair.Value.Handle,
                ActionX = imagePair.Value.ActionX,
                ActionY = imagePair.Value.ActionY,
                HotspotX = imagePair.Value.HotspotX,
                HotspotY = imagePair.Value.HotspotY,
                Flags = imagePair.Value.Flags.flag,
                bmp = imagePair.Value.bitmap
            });
        return fProj;
    }

    public MFAData ToMFA()
    {
        //var mfaReader = new ByteReader("test.mfa", FileMode.Open);
        var mfa = new MFAData();

        //mfa.Read(mfaReader);
        mfa.Name = name;
        mfa.Description = description;
        mfa.Author = author;
        mfa.Copyright = copyright;
        mfa.Company = company;
        mfa.Version = version;
        mfa.HelpFile = helpFile;
        mfa.Aboutbox = aboutBox;
        mfa.WindowX = windowWidth;
        mfa.WindowY = windowHeight;

        mfa.DisplayFlags.flag = displayFlags;
        mfa.GraphicFlags.flag = 2181249; //don't ask
        mfa.FrameRate = frameRate;
        mfa.BuildType = buildType;

        mfa.MfaBuild = 6;
        mfa.Product = 2;
        mfa.BuildVersion = 292;
        mfa.LangId = 1033;
        mfa.InitialLifes = 3;
        mfa.GraphicMode = 4;
        //mfa.Menu = null;
        //mfa.Path = @"D:\ClickteamStuff\CTFAK2.0\CTFAK\Tools\JFusion\bin\Debug\net6.0-windows\test.mfa";

        //mfa.Stamp = new byte[0];

        //EVERYTHING ABOVE THIS COMMENT IS UNFINISHED

        mfa.Fonts = new FontBank();
        mfa.Images = new AGMIBank();
        mfa.Icons = new AGMIBank();
        mfa.Sounds = new SoundBank();
        mfa.Music = new MusicBank();
        mfa.Stamp = new byte[0];
        foreach (var jmfaIcon in icons)
        {
            var newIcon = new FusionImage();
            newIcon.FromBitmap(jmfaIcon.bmp);
            newIcon.Handle = jmfaIcon.Handle;
            newIcon.ActionX = jmfaIcon.ActionX;
            newIcon.ActionY = jmfaIcon.ActionY;
            newIcon.HotspotX = jmfaIcon.HotspotX;
            newIcon.HotspotY = jmfaIcon.HotspotY;
            newIcon.Flags.flag = jmfaIcon.Flags;
            mfa.Icons.Items.Add(newIcon.Handle, newIcon);
        }

        foreach (var jMfAImage in images)
        {
            var image = new FusionImage();
            image.FromBitmap(jMfAImage.bmp);
            image.Handle = jMfAImage.Handle;
            image.ActionX = jMfAImage.ActionX;
            image.ActionY = jMfAImage.ActionY;
            image.HotspotX = jMfAImage.HotspotX;
            image.HotspotY = jMfAImage.HotspotY;
            image.Flags.flag = jMfAImage.Flags;
            mfa.Images.Items.Add(image.Handle, image);
        }

        mfa.Icons.Palette = frames[0].palette;
        mfa.Images.Palette = frames[0].palette;

        mfa.Controls = new MFAControls();
        mfa.menuImages = new Dictionary<int, int>();
        mfa.GlobalValues = new MFAValueList();
        mfa.GlobalStrings = new MFAValueList();
        mfa.GlobalEvents = new byte[0];

        mfa.IconImages = new List<int>();
        mfa.CustomQuals = new List<Tuple<string, int>>();
        mfa.Extensions = new List<Tuple<int, string, string, int, string>>();
        mfa.Frames = new List<MFAFrame>();
        var tempFrames = new JMFAFrame[frames.Count];
        foreach (var jmfaFrame in frames)
        {
            var currentFrameData = frameData[jmfaFrame.name];
            jmfaFrame.handle = currentFrameData.handle;

            tempFrames[currentFrameData.index] = jmfaFrame;
        }

        foreach (var tempFrame in tempFrames) mfa.Frames.Add(tempFrame.ToMFA());
        mfa.Chunks = new MFAChunkList();

        return mfa;
    }

    public static JMFAFile Open(string filePath)
    {
        var jmfa = JsonConvert.DeserializeObject<JMFAFile>(File.ReadAllText(filePath));
        var projectDir = Path.GetDirectoryName(filePath);
        foreach (var frmDir in Directory.GetDirectories($"{projectDir}\\Frames"))
        {
            var newFrame = JMFAFrame.Open(frmDir);
            jmfa.frames.Add(newFrame);
        }

        foreach (var iconFile in Directory.GetFiles($"{projectDir}\\Icons", "*.png"))
        {
            var newBmp = (Bitmap)System.Drawing.Image.FromFile(iconFile);
            var newImg =
                JsonConvert.DeserializeObject<JMfAImage>(File.ReadAllText($"{iconFile.Replace(".png", ".json")}"));
            newImg.bmp = newBmp;
            jmfa.icons.Add(newImg);
        }

        foreach (var imgFile in Directory.GetFiles($"{projectDir}\\Images", "*.png"))
        {
            var newBmp = (Bitmap)System.Drawing.Image.FromFile(imgFile);
            var newImg =
                JsonConvert.DeserializeObject<JMfAImage>(File.ReadAllText($"{imgFile.Replace(".png", ".json")}"));
            newImg.bmp = newBmp;
            jmfa.images.Add(newImg);
        }

        jmfa.frameData =
            JsonConvert.DeserializeObject<Dictionary<string, JMFAFrameData>>(
                File.ReadAllText($"{projectDir}\\Frames\\frameData.json"));

        return jmfa;
    }

    public void Write(string filePath)
    {
        Directory.CreateDirectory(filePath);
        File.WriteAllText($"{filePath}\\{name}.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        foreach (var frame in frames) frame.Write(filePath);

        Directory.CreateDirectory($"{filePath}\\Images");
        foreach (var img in images)
        {
            img.bmp.Save($"{filePath}\\Images\\{img.Handle}.png");
            File.WriteAllText($"{filePath}\\Images\\{img.Handle}.json",
                JsonConvert.SerializeObject(img, Formatting.Indented));
        }

        Directory.CreateDirectory($"{filePath}\\Icons");
        foreach (var img in icons)
        {
            img.bmp.Save($"{filePath}\\Icons\\{img.Handle}.png");
            File.WriteAllText($"{filePath}\\Icons\\{img.Handle}.json",
                JsonConvert.SerializeObject(img, Formatting.Indented));
        }

        File.WriteAllText($"{filePath}\\Frames\\frameData.json",
            JsonConvert.SerializeObject(frameData, Formatting.Indented));
    }

    public class JMFAFrameData
    {
        public int handle;
        public int index;
    }
}