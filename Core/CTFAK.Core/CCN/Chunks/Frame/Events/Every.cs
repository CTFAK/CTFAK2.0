using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class Every : ParameterCommon
    {
        public int Delay;
        public int Compteur;
        
        public Every(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            Delay = reader.ReadInt32();
            Compteur = reader.ReadInt32();
            
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Delay);
            Writer.WriteInt32(Compteur);
        }

        public override string ToString()
        {
            return $"Every {Delay/1000} sec";
        }
    }
}
