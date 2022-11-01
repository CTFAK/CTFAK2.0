using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFATransition : ChunkLoader
    {
        public string Module;
        public string Name;
        public string Id;
        public string TransitionId;
        public int Duration;
        public int Flags;
        public Color Color;
        public byte[] ParameterData;



  

        public override void Read(ByteReader reader)
        {
            Module = reader.AutoReadUnicode();
            Name = reader.AutoReadUnicode();
            Id = reader.ReadAscii(4);
            TransitionId = reader.ReadAscii(4);
            Duration = reader.ReadInt32();
            Flags = reader.ReadInt32();
            Color = reader.ReadColor();
            ParameterData = reader.ReadBytes(reader.ReadInt32());

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Module);
            Writer.AutoWriteUnicode(Name);
            Writer.WriteAscii(Id);
            Writer.WriteAscii(TransitionId);
            Writer.WriteInt32(Duration);
            Writer.WriteInt32(Flags);
            Writer.WriteColor(Color);
            Writer.WriteInt32(ParameterData.Length);
            Writer.WriteBytes(ParameterData);
        }


    }
}
