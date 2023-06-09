using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class ObjectLoader : ChunkLoader
{
    public MFAObjectFlags AltFlags;
    public Color BackgroundColor;
    public Behaviours Behaviours;
    public MFAMovements Movements;
    public int NewObjectFlags;
    public int ObjectFlags;
    public short[] Qualifiers = new short[8];
    public MFAValueList Strings;
    public MFAValueList Values;

    public override void Write(ByteWriter writer)
    {
        // if(Qualifiers==null) throw new NullReferenceException("QUALIFIERS NULL");
        writer.WriteInt32(ObjectFlags);
        writer.WriteInt32(NewObjectFlags);
        // var col = Color.FromArgb(255,BackgroundColor.R,BackgroundColor.G,BackgroundColor.B);
        writer.WriteColor(BackgroundColor);

        for (var i = 0; i < 8; i++) writer.WriteInt16(Qualifiers[i]);
        writer.WriteInt16(-1);
        Values.Write(writer);
        Strings.Write(writer);
        Movements.Write(writer);
        Behaviours.Write(writer);

        writer.WriteInt8(0); //FadeIn
        writer.WriteInt8(0); //FadeOut
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