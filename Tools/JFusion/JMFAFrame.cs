using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;
using JFusion.ObjectTypes;
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
        [JsonIgnore] public int handle;

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

        public MFAFrame ToMFA()
        {
            var mfaFrame = new MFAFrame(null);

            mfaFrame.Name = name;
            mfaFrame.Handle = handle;
            mfaFrame.SizeX = width;
            mfaFrame.SizeY = height;
            mfaFrame.Background = backgroundColor;
            mfaFrame.MaxObjects = maxObjects;
            mfaFrame.LastViewedX = lastViewedX;
            mfaFrame.LastViewedY = lastViewedY;
            mfaFrame.Flags.flag = flags;
            mfaFrame.Password = password;
            mfaFrame.ActiveLayer = activeLayer;
            mfaFrame.Events = new MFAEvents(null);
            mfaFrame.Chunks = new MFAChunkList(null);

            for (int i = 0; i < objects.Count; i++)
            {
                var jmfaobj = objects[i];

                var newOi = new MFAObjectInfo(null);
                var newInst = new MFAObjectInstance(null);
                var newFolder = new MFAItemFolder(null);
               


                    /*if (jmfaobj.objectType == 2)
                    {
                        Console.WriteLine("Translating object: " + jmfaobj.GetType());
                        JMFAActive active = (JMFAActive)jmfaobj;

                        newOi.Name = jmfaobj.name;
                        newOi.ObjectType = 2;
                        newOi.Handle = i;
                        newInst.ItemHandle = (uint)i;
                        newInst.X = jmfaobj.xPos;
                        newInst.Y = jmfaobj.yPos;

                        var activeLoader = new MFAActive(null);
                        activeLoader.Items.Add(0, new MFAAnimation(null)
                        {
                            Directions = new List<MFAAnimationDirection>()
                            {
                                new MFAAnimationDirection(null)
                                {
                                    Name = null,
                                    BackTo = active.animations[0]
                                        .directions[0]
                                        .backTo,
                                    Frames = active.animations[0]
                                        .directions[0]
                                        .frames,
                                    Index = 0,
                                    MinSpeed = active.animations[0].directions[0].minSpeed,
                                    MaxSpeed = active.animations[0].directions[0].maxSpeed,
                                    Repeat = active.animations[0].directions[0].repeat,


                                }
                            }
                        });
                        newOi.Loader = activeLoader;
                        newFolder.isRetard = true;
                        newFolder.Items.Add((uint)newOi.Handle);
                        mfaFrame.Instances.Add(newInst);
                        mfaFrame.Items.Add(newOi);
                    }*/
              

            }


            return mfaFrame;
        }

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

        public static JMFAFrame Open(string filePath)
        {
            var jframe = JsonConvert.DeserializeObject<JMFAFrame>(File.ReadAllText($"{filePath}\\frameData.json"));
            foreach (var objPath in Directory.GetFiles($"{filePath}\\Objects","*.json"))
            {
                var newObj = JsonConvert.DeserializeObject<JMFAObject>(File.ReadAllText(objPath));
                switch (newObj.objectType)
                {
                    case 0://Quick Backdrop
                        break;
                    case 1://Backdrop
                        break;
                    case 2://Active
                        JMFAActive active = new JMFAActive();//JsonConvert.DeserializeObject<JMFAActive>(File.ReadAllText(objPath));
                        jframe.objects.Add((JMFAActive)active);
                        continue;
                        break;
                    case 3://Text
                        break;
                    case 4://Question
                        break;
                    case 5://Score
                        break;
                    case 6://Lives
                        break;
                    case 7://Counter
                        break;
                    case 8://RTF
                        break;
                    case 9://SubApp
                        break;
                    default://Extension probably
                        break;
                }
                jframe.objects.Add(newObj);
            }
            


            return jframe;
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