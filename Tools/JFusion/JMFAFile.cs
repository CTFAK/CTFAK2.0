using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using CTFAK.MFA;
using Newtonsoft.Json;

namespace JFusion
{
    public class JMFAFile
    {
        public string name;
        public string description;
        public string author;
        public string copyright;
        public string company;
        public string version;
        public string helpFile;
        public string aboutBox;

        public int windowWidth;
        public int windowsHeight;

        public uint displayFlags;
        public uint graphicFlags;
        public int frameRate;
        public int buildType;
        
        
        
        
        public int mfaBuild;
        public int product;
        public int buildVersion;
        public int langId;

        [JsonIgnore] public List<JMFAFrame> frames=new List<JMFAFrame>();
        
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
            fProj.windowsHeight = mfa.WindowY;

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
            }

            return fProj;
        }



        public static JMFAFile Open(string filePath)
        {
            return JsonConvert.DeserializeObject<JMFAFile>(File.ReadAllText(filePath));
        }

        public void Write(string filePath)
        {
            Directory.CreateDirectory(filePath);
            File.WriteAllText($"{filePath}\\{name}.json",JsonConvert.SerializeObject(this,Formatting.Indented));
            foreach (var frame in frames)
            {
                frame.Write(filePath);
            }
        }
        
    }
}