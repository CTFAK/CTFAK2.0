using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.MFA.MFAObjectLoaders;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAObjectInfo : ChunkLoader
    {
        public int ObjectType;
        public int Handle;
        public string Name;
        public int Transparent;
        public int InkEffect;
        public uint InkEffectParameter;
        public int AntiAliasing;
        public int Flags;
        public int IconType;
        public int IconHandle;
        public MFAChunks Chunks;
        public ChunkLoader Loader;
        public MFAObjectFlags FlagWriter;

        public override void Write(ByteWriter Writer)
        {
            //Debug.Assert(ObjectType==2);
            Writer.WriteInt32(this.ObjectType);
            Writer.WriteInt32(Handle);
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(Transparent);
            Writer.WriteInt32(InkEffect);
            Writer.WriteUInt32(InkEffectParameter);
            Writer.WriteInt32(AntiAliasing);
            Writer.WriteInt32(Flags);
            Writer.WriteInt32(1);
            Writer.WriteInt32(IconHandle);

            if (FlagWriter != null)
                FlagWriter.Write(Writer);

            Chunks.Write(Writer);
            Loader.Write(Writer);
        }



        public override void Read(ByteReader reader)
        {

            ObjectType = reader.ReadInt32();
            Handle = reader.ReadInt32();
            Name = reader.AutoReadUnicode();
            Transparent = reader.ReadInt32();

            InkEffect = reader.ReadInt32();
            InkEffectParameter = reader.ReadUInt32();
            AntiAliasing = reader.ReadInt32();

            Flags = reader.ReadInt32();

            IconType = reader.ReadInt32();
            if (IconType == 1)
            {
                IconHandle = reader.ReadInt32();
            }
            else throw new InvalidDataException("invalid icon");
            Chunks = new MFAChunks();
            Chunks.Log = true;
            Chunks.Read(reader);

            if (ObjectType >= 32)//extension base
            {
                Loader = new MFAExtensionObject();
            }
            else if (ObjectType == 0)
            {
                Loader = new MFAQuickBackdrop();
            }
            else if (ObjectType == 1)
            {
                Loader = new MFABackdrop();
            }
            else if (ObjectType == 2)
            {
                Loader = new MFAActive();
            }
            else if(ObjectType == 3)
            {
                Loader = new MFAText();
            }
            else if (ObjectType == 7)
            {
                Loader = new MFACounter();
            }
            else throw new NotImplementedException("Unsupported object: " + ObjectType);
            Loader.Read(reader);



        }

    }
}
