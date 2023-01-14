using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MFA;

namespace CTFAK.MFA.MFAObjectLoaders;

public class ObjectLoader : ChunkLoader
{
    public Color BackgroundColor;
    public Behaviours Behaviours;
    public MFAMovements Movements;
    public int NewObjectFlags;
    public int ObjectFlags;
    public short[] Qualifiers = new short[8];
    public MFAValueList Strings;
    public MFAValueList Values;

    public override void Write(ByteWriter Writer)
    {
        // if(Qualifiers==null) throw new NullReferenceException("QUALIFIERS NULL");
        Writer.WriteInt32(ObjectFlags);
        Writer.WriteInt32(NewObjectFlags);
        // var col = Color.FromArgb(255,BackgroundColor.R,BackgroundColor.G,BackgroundColor.B);
        Writer.WriteColor(BackgroundColor);

        for (var i = 0; i < 8; i++) Writer.WriteInt16(Qualifiers[i]);
        Writer.WriteInt16(-1);
        Values.Write(Writer);
        Strings.Write(Writer);
        Movements.Write(Writer);
        Behaviours.Write(Writer);

        Writer.WriteInt8(0); //FadeIn
        Writer.WriteInt8(0); //FadeOut
    }


    public override void Read(ByteReader reader)
    {
        ObjectFlags = reader.ReadInt32();
        NewObjectFlags = reader.ReadInt32();
        BackgroundColor = reader.ReadColor();
        var end = reader.Tell() + 2 * (8 + 1);
        for (var i = 0; i < 8; i++)
        {
            var value = reader.ReadInt16();
            // if(value==-1)
            // {
            // break;
            // }
            Qualifiers[i] = value;
        }

        reader.Seek(end);

        Values = new MFAValueList();
        Values.Read(reader);
        Strings = new MFAValueList();
        Strings.Read(reader);
        Movements = new MFAMovements();
        Movements.Read(reader);
        Behaviours = new Behaviours();
        Behaviours.Read(reader);
        reader.Skip(2); //Transitions
    }
}