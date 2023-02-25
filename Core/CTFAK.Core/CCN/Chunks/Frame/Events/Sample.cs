﻿using CTFAK.CCN.Chunks.Banks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class Sample : ParameterCommon
    {
        public int Handle;
        public string Name;
        public int Flags;

        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt16();
            Flags = reader.ReadUInt16();
            Name = reader.ReadUniversal();

            if (Settings.Android && Settings.Build < 289 &&
                !AndroidSoundBank.oldAndroidNames.ContainsKey(Handle))
                AndroidSoundBank.oldAndroidNames.Add(Handle, Name);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short) Handle);
            Writer.WriteUInt16((ushort) Flags);
            Name = Name.Trim((char)0);
            Writer.WriteUnicode(Name);
            Writer.Skip(120);
            Writer.WriteInt16(0);
        }

        public override string ToString()
        {
            return $"Sample '{Name}' handle: {Handle}";
        }
    }
}
