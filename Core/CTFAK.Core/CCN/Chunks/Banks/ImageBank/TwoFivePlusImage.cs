using CTFAK.Memory;
using CTFAK.Utils;
using K4os.Compression.LZ4;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace CTFAK.Core.CCN.Chunks.Banks.ImageBank
{
    public class TwoFivePlusImage : FusionImage
    {
        public override void Read(ByteReader reader)
        {
            if (Settings.Fusion3Seed || CTFAKCore.parameters.Contains("-fuckimgs"))
            {
                var seek = reader.Tell();
                bool a1 = false;
                bool a2 = false;
                while (true)
                {
                    var newseek = reader.Tell();
                    if (reader.ReadByte() != 255 || a2)
                        if (reader.ReadInt32() == -1)
                            if (reader.ReadByte() != 255 || a1)
                            {
                                reader.Seek(reader.Tell() - 9);
                                break;
                            }
                    if (newseek > seek + 20)
                    {
                        if (a1) a2 = true;
                        else a1 = true;
                        reader.Seek(seek);
                    }
                    else
                        reader.Seek(newseek + 1);
                }
            }

            Handle = reader.ReadInt32() - 1;
            Checksum = reader.ReadInt32();
            references = reader.ReadInt32();
            reader.ReadInt32();
            var dataSize = reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            GraphicMode = reader.ReadByte();
            Flags.flag = reader.ReadByte();
            reader.ReadInt16();
            HotspotX = reader.ReadInt16();
            HotspotY = reader.ReadInt16();
            ActionX = reader.ReadInt16();
            ActionY = reader.ReadInt16();
            Transparent = reader.ReadColor();
            //Logger.Log("Image Handle '" + Handle + "'s Transparent color: [" + Transparent + "]");
            if (Settings.F3)
                Transparent = Color.Black;
            var decompSizePlus = reader.ReadInt32();
            var rawImg = reader.ReadBytes(Math.Max(0, dataSize - 4));
            var task = new Task(() =>
            {
                var target = new byte[decompSizePlus];
                LZ4Codec.Decode(rawImg, target);
                imageData = target;
            });
            task.RunSynchronously();
            ImageBank.imageReadingTasks.Add(task);
        }
    }
}
