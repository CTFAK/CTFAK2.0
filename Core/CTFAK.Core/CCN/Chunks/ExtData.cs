using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    public class ExtData:ChunkLoader
    {
        public byte[] data;
        public string name;

        public override void Read(ByteReader reader)
        {
            var filename = reader.ReadAscii();
            var data = reader.ReadBytes();
            Logger.Log($"Found file {filename}, {data.Length}"); 
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}