using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.MFA;
using Newtonsoft.Json;

namespace JFusion
{
    public class JMFAFrame
    {
        public string name;

        public int width;
        public int height;
        public Color backgroundColor;
        public int maxObjects;

        [System.Text.Json.Serialization.JsonIgnore] public List<JMFAObject> objects = new List<JMFAObject>();

        public int lastViewedX;
        public int lastViewedY;

        public uint flags;
        public string password;
        public List<Color> palette;

        public int activeLayer;
        public List<JMFALayer> layers = new List<JMFALayer>();
        public JMFAEvents events;
        public JMFATransition fadeIn;
        public JMFATransition fadeOut;
        
        public static JMFAFrame FromMFA(MFAFrame mfaFrame)
        {
            var newFrame = new JMFAFrame();

            newFrame.name = mfaFrame.Name;
            newFrame.width = mfaFrame.SizeX;
            newFrame.height = mfaFrame.SizeY;
            newFrame.backgroundColor = mfaFrame.Background;
            newFrame.maxObjects = mfaFrame.MaxObjects;

            newFrame.lastViewedX = mfaFrame.LastViewedX;
            newFrame.lastViewedY = mfaFrame.LastViewedY;
            newFrame.flags = mfaFrame.Flags.flag;
            newFrame.password = mfaFrame.Password;
            newFrame.activeLayer = mfaFrame.ActiveLayer;
            foreach (var mfaLayer in mfaFrame.Layers)
            {
                var newLayer = new JMFALayer();
                newLayer.name = mfaLayer.Name;
                newLayer.xCoeff = mfaLayer.XCoefficient;
                newLayer.yCoeff = mfaLayer.YCoefficient;
                newLayer.flags = mfaLayer.Flags.flag;
            }

            Dictionary<int, MFAObjectInfo> objectInfos = new Dictionary<int, MFAObjectInfo>();
            foreach (var oi in mfaFrame.Items)    
            {
                    objectInfos.Add(oi.Handle,oi);
            }
            foreach (var mfaInst in mfaFrame.Instances)
            {
                try
                {
                    var newObj = JMFAObject.FromMFA(mfaInst, objectInfos[(int)mfaInst.ItemHandle]);
                    newFrame.objects.Add(newObj); 
                }
                catch{Console.WriteLine("Failed to create object");}

            }
            
            
            
            
            return newFrame;
        }

        public string ClearName(string og)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                og = og.Replace(c.ToString(), "");
            }

            return og;
        }

        public void Write(string filePath)
        {
            Directory.CreateDirectory($"{filePath}\\Frames\\{name}");
            File.WriteAllText($"{filePath}\\Frames\\{name}\\frameData.json",JsonConvert.SerializeObject(this,Formatting.Indented));
            Directory.CreateDirectory($"{filePath}\\Frames\\{name}\\Objects");
            foreach (var obj in objects)
            {
                File.WriteAllText($"{filePath}\\Frames\\{name}\\Objects\\{ClearName(obj.name)}.json",JsonConvert.SerializeObject(obj,Formatting.Indented));
            }
        }
    }
}