using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Frame
{
    public class Transition : ChunkLoader
    {
        public string Module;
        public string Name;
        public int Duration;
        public int Flags;
        public Color Color;
        public string ModuleFile;
        public byte[] ParameterData;




        public override void Read(ByteReader reader)
        {
            var currentPos = reader.Tell();
            Module = reader.ReadAscii(4);
            Name = reader.ReadAscii(4);
            Duration = reader.ReadInt32();
            Flags = reader.ReadInt32();
            Color = reader.ReadColor();
            var nameOffset = reader.ReadInt32();
            var parameterOffset = reader.ReadInt32();
            var parameterSize = reader.ReadInt32();
            reader.Seek(currentPos + nameOffset);
            ModuleFile = reader.ReadYuniversal();
            reader.Seek(currentPos + parameterOffset);
            ParameterData = reader.ReadBytes(parameterSize);



        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }
    
    }
}
