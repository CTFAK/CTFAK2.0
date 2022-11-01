using System;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class ParameterCommon : ChunkLoader
    {
        

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException("Unexcepted parameter: "+this.GetType().Name);
           
        }



        public override void Read(ByteReader reader)
        {

            
        }
    }
}
