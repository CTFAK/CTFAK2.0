using System;
using System.Drawing;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA
{
    public class MFALayer : ChunkLoader
    {
        public string Name = "Ass";
        public float XCoefficient;
        public float YCoefficient;

        public BitDict Flags = new BitDict(new string[]
            {
                "Visible",
                "Locked",
                "Obsolete",
                "HideAtStart",
                "NoBackground",
                "WrapHorizontally",
                "WrapVertically",
                "PreviousEffect"
            }
        );

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32((int)Flags.flag);
            Writer.WriteSingle(XCoefficient);
            Writer.WriteSingle(YCoefficient);
        }



        public override void Read(ByteReader reader)
        {
            Name = reader.AutoReadUnicode();
            Flags.flag = (uint)reader.ReadInt32();
            XCoefficient = reader.ReadSingle();
            YCoefficient = reader.ReadSingle();





        }
 
    }
}
