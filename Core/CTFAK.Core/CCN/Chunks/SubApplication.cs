using System.Collections.Generic;
using System.Drawing;
using System.Xml.Schema;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public class SubApplication : ChunkLoader
    {
        public int FrameNumber;





        public override void Read(ByteReader reader)
        {
            if (Settings.Old)
            {
                FrameNumber = reader.ReadInt32();
            }
            else
            {
                FrameNumber = reader.ReadInt32();
            }



        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(FrameNumber);
        }


    }
}