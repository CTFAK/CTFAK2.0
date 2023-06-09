namespace CTFAK.MMFParser;

public class Constants
{
    public enum ObjectType
    {
        Player = -7,
        Keyboard = -6,
        Create = -5,
        Timer = -4,
        Game = -3,
        Speaker = -2,
        System = -1,
        QuickBackdrop = 0,
        Backdrop = 1,
        Active = 2,
        Text = 3,
        Question = 4,
        Score = 5,
        Lives = 6,
        Counter = 7,
        Rtf = 8,
        SubApplication = 9,
        Extension = 32
    }

    // Do we need this? I don't really think we need this, but I'll leave this for now in case we need it in future
    public enum Products
    {
        MMF1 = 1,
        STD = 2,
        DEV = 3,
        CNC1 = 0
    }

    public enum ValueType
    {
        Long = 0,
        Int = 0,
        String = 1,
        Float = 2,
        Double = 2
    }
}