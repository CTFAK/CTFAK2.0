using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Events;

public class Events : ChunkLoader
{
    public static int IdentifierCounter;
    public readonly string End = "<<ER";
    public readonly string EventCount = "ERes";
    public readonly string EventgroupData = "ERev";
    public readonly string Header = "ER>>";
    public List<EventGroup> Items = new();
    public int MaxObjectInfo;

    public int MaxObjects;
    public List<int> NumberOfConditions = new();
    public int NumberOfPlayers;
    public List<Quailifer> QualifiersList = new();

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Read(ByteReader reader)
    {
        IdentifierCounter = 0;
        // if (Settings.GameType == GameType.OnePointFive) return;
        while (true)
        {
            var identifier = reader.ReadAscii(4);
            if (identifier == Header)
            {
                MaxObjects = reader.ReadInt16();
                MaxObjectInfo = reader.ReadInt16();
                NumberOfPlayers = reader.ReadInt16();
                for (var i = 0; i < 17; i++) NumberOfConditions.Add(reader.ReadInt16());

                var qualifierCount = reader.ReadInt16(); //should be 0, so i dont care
                for (var i = 0; i < qualifierCount; i++)
                {
                    var newQualifier = new Quailifer();
                    newQualifier.Read(reader);
                    QualifiersList.Add(newQualifier);
                }
            }
            else if (identifier == EventCount)
            {
                if (Settings.Android) reader.ReadInt32(); //TODO: figure out what it is
                var size = reader.ReadInt32();
            }
            else if (identifier == EventgroupData)
            {
                var size = reader.ReadInt32();
                if (Settings.Android) size += 4;

                var endPosition = reader.Tell() + size;
                if (Settings.Android) reader.ReadInt32();
                var i = 0;
                while (true)
                {
                    i++;
                    var eg = new EventGroup();
                    eg.Read(reader);
                    Items.Add(eg);

                    if (reader.Tell() >= endPosition) break;
                }
            }
            else if (identifier == End)
            {
                break;
            }
        }
    }
}

public class Quailifer : ChunkLoader
{
    private List<int> _objects = new();
    public int ObjectInfo;
    public int Qualifier;
    public int Type;

    public override void Read(ByteReader reader)
    {
        ObjectInfo = reader.ReadUInt16();
        Type = reader.ReadInt16();
        Qualifier = ObjectInfo & 0b11111111111;
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt16((ushort)ObjectInfo);
        writer.WriteInt16((short)Type);
    }
}

public class EventGroup : ChunkLoader
{
    public List<Action> Actions = new();
    public List<Condition> Conditions = new();
    public ushort Flags;
    public int Identifier;
    public bool isMFA = false;
    public int IsRestricted;
    public byte NumberOfActions;
    public byte NumberOfConditions;
    public int RestrictCpt;
    public int Size;
    public int Undo;

    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        Size = reader.ReadInt16() * -1;
        NumberOfConditions = reader.ReadByte();
        NumberOfActions = reader.ReadByte();
        Flags = reader.ReadUInt16();
        if (Settings.Old || Settings.CBM)
        {
            IsRestricted = reader.ReadInt16(); //For MFA
            RestrictCpt = reader.ReadInt16();
            Identifier = reader.ReadInt16();
            Undo = reader.ReadInt16();
        }
        else
        {
            if (Settings.Build >= 284)
            {
                if (isMFA)
                {
                    IsRestricted = reader.ReadInt16(); //For MFA
                    RestrictCpt = reader.ReadInt16();
                    Identifier = reader.ReadInt16();
                    Undo = reader.ReadInt16();
                }
                else
                {
                    var nop = reader.ReadInt16();
                    IsRestricted = reader.ReadInt32();
                    RestrictCpt = reader.ReadInt32();
                }
            }
            else
            {
                IsRestricted = reader.ReadInt16();
                RestrictCpt = reader.ReadInt16();
                Identifier = reader.ReadInt16();
                Undo = reader.ReadInt16();
            }
        }

        // Logger.Log($"Cond: {NumberOfConditions},Act: {NumberOfActions}");
        for (var i = 0; i < NumberOfConditions; i++)
        {
            //Child Event fix by -liz
            var item = new Condition();
            item.Read(reader);
            item.Identifier += Events.IdentifierCounter;
            Fixer.FixConditions(ref item);
            if ((item.Num == -27 && item.ObjectType == -1) ||
                (item.Num == -43 && item.ObjectType == -1))
            {
                //this is the most retarded thing i have ever seen and it breaks mfa reading. fuck that one moron who added that
            }

            else if (item.Num == -25 || item.Num == -41)
            {
                if (item.Items.Count > 0)
                {
                    foreach (var param in item.Items)
                        if (param.Loader is MultipleVariables multivar)
                        {
                            Logger.Log(multivar.flags);

                            //To the no-lifer who decided that it was a good idea to do that kind of shit:
                            //All that bit logic bullshit is probably slower than the normal way of value comparsion
                            //And if it was done to prevent decompilers from working with it - you have failed
                            //I mean, I do respect people who actually develop Fusion (Yves and Francois), but whoever decided to do this thing is a fucking retard
                            //2.01.2023 I should probably rewrite this part, because fixing and translating it there is kind of dumb if you ask me
                            //11.03.2023 Yuni forced me to fix flags, so I'm back here again. I hate this fucking condition and I don't want to ever revisit it again anytime soon
                            //12.03.2023 Turns out my fix didn't really work, so I'm back here again
                            var cnt = 0;
                            var mask = 1;
                            while (true)
                            {
                                if (mask == 0) break;
                                if ((mask & multivar.flagMasks) == 0)
                                {
                                    mask <<= 1;
                                    cnt++;
                                    continue;
                                }

                                var newCondition = new Condition();
                                newCondition.DefType = item.DefType;
                                newCondition.Identifier = item.Identifier + cnt;
                                newCondition.ObjectInfo = item.ObjectInfo;
                                newCondition.Flags = item.Flags;
                                newCondition.OtherFlags = item.OtherFlags;
                                newCondition.ObjectType = item.ObjectType;
                                newCondition.Num = (mask & multivar.flagValues) == 0 ? -24 : -25;
                                var exp = new ExpressionParameter { Comparsion = 0 };
                                exp.Items.Add(new Expression { Loader = new LongExp { Value = cnt }, ObjectType = -1 });
                                newCondition.Items.Add(new Parameter { Code = 22, Loader = exp });
                                Conditions.Add(newCondition);
                                mask <<= 1;
                                cnt++;
                                Events.IdentifierCounter++;
                            }

                            //Alterable Flags
                            //Alterable Values
                            for (var j = 0; j < multivar.values.Length; j++)
                            {
                                var val = multivar.values[j];
                                var newCondition = new Condition();
                                newCondition.DefType = item.DefType;
                                newCondition.Identifier = item.Identifier + j;
                                newCondition.ObjectInfo = item.ObjectInfo;
                                newCondition.Flags = item.Flags;
                                newCondition.OtherFlags = item.OtherFlags;
                                newCondition.ObjectType = item.ObjectType;

                                //Alterable Values
                                newCondition.Num = -27;
                                var newParam = new AlterableValue();
                                newParam.Value = (short)val.index;
                                newCondition.Items.Add(new Parameter { Code = 50, Loader = newParam });
                                var exp = new ExpressionParameter { Comparsion = (short)val.op };
                                exp.Items.Add(new Expression
                                    { Loader = new LongExp { Value = (int)val.value }, ObjectType = -1 });
                                newCondition.Items.Add(new Parameter { Code = 23, Loader = exp });
                                Conditions.Add(newCondition);
                                Events.IdentifierCounter++;
                            }
                        }
                        else
                        {
                            Conditions.Add(item);
                        }
                }
                else
                {
                    Conditions.Add(item);
                }
            }
            else
            {
                Conditions.Add(item);
            }
        }

        for (var i = 0; i < NumberOfActions; i++)
        {
            var item = new Action();
            item.Read(reader);
            Fixer.FixActions(ref item);
            if (item.Num == 43 && item.ObjectType == -1)
            {
            }
            else if (item.Num == 2 && item.Items.Count == 2)
            {
                var xAct = new Action();
                xAct.DefType = item.DefType;
                xAct.ObjectInfo = item.ObjectInfo;
                xAct.Flags = item.Flags;
                xAct.OtherFlags = item.OtherFlags;
                xAct.ObjectType = item.ObjectType;
                xAct.Num = 2;
                xAct.Items.Add(item.Items[0]);
                var yAct = new Action();
                yAct.DefType = item.DefType;
                yAct.ObjectInfo = item.ObjectInfo;
                yAct.Flags = item.Flags;
                yAct.OtherFlags = item.OtherFlags;
                yAct.ObjectType = item.ObjectType;
                yAct.Num = 3;
                yAct.Items.Add(item.Items[1]);
                Actions.Add(xAct);
                Actions.Add(yAct);
            }
            else
            {
                Actions.Add(item);
            }
        }

        reader.Seek(currentPosition + Size);
        // Logger.Log($"COND:{NumberOfConditions}, ACT: {NumberOfActions}");
    }

    public override void Write(ByteWriter writer)
    {
        var newWriter = new ByteWriter(new MemoryStream());
        newWriter.WriteInt8((byte)Conditions.Count);
        newWriter.WriteInt8((byte)Actions.Count);
        newWriter.WriteUInt16(Flags);
        if (Settings.Build >= 284)
        {
            if (isMFA) //For MFA
            {
                newWriter.WriteInt16((short)IsRestricted);
                newWriter.WriteInt16((short)RestrictCpt);
                newWriter.WriteInt16((short)Identifier);
                newWriter.WriteInt16((short)Undo);
            }
            else
            {
                newWriter.WriteInt16(0);
                newWriter.WriteInt32(IsRestricted);
                newWriter.WriteInt32(RestrictCpt);
            }
        }
        else
        {
            newWriter.WriteInt16((short)IsRestricted);
            newWriter.WriteInt16((short)RestrictCpt);
            newWriter.WriteInt16((short)Identifier);
            newWriter.WriteInt16((short)Undo);
        }

        foreach (var condition in Conditions)
        {
            var cond = condition;
            condition.Write(newWriter);
        }

        foreach (var action in Actions)
        {
            var act = action;
            act.Write(newWriter);
        }

        writer.WriteInt16((short)((newWriter.Size() + 2) * -1));

        writer.WriteWriter(newWriter);
    }
}

public static class Fixer
{
    public static void FixConditions(ref Condition cond)
    {
        var num = cond.Num;
        //Alterable Values:
        if (num == -42 && cond.ObjectType != -1) num = -27;
        //Global Values
        if (cond.ObjectType == -1)
            if (num == -28 || num == -29 || num == -30 || num == -31 || num == -32 || num == -33)
                num = -8;
        cond.Num = num;
    }

    public static void FixActions(ref Action act)
    {
        var num = act.Num;
        var type = act.ObjectType;
        if (type == -1)
        {
            if (num == 27 || num == 28 || num == 29 || num == 30)
                num = 3;
            if (num == 31 || num == 32 || num == 33 || num == 34)
                num = 4;
            if (num == 35 || num == 36 || num == 37 || num == 38)
                num = 5;
        }

        act.Num = num;
    }
}

public class Condition : ChunkLoader
{
    public int DefType;
    public int Flags;
    public int Identifier;
    public List<Parameter> Items = new();
    public int Num;
    public int NumberOfParameters;
    public int ObjectInfo;
    public int ObjectInfoList;
    public int ObjectType;
    public int OtherFlags;

    public override void Write(ByteWriter writer)
    {
        var newWriter = new ByteWriter(new MemoryStream());
        newWriter.WriteInt16((short)ObjectType);
        newWriter.WriteInt16((short)Num);
        newWriter.WriteUInt16((ushort)ObjectInfo);
        newWriter.WriteInt16((short)ObjectInfoList);
        newWriter.WriteUInt8((sbyte)Flags);
        newWriter.WriteUInt8((sbyte)OtherFlags);
        newWriter.WriteUInt8((sbyte)Items.Count);
        newWriter.WriteInt8((byte)DefType);
        newWriter.WriteUInt16((ushort)Identifier);
        foreach (var parameter in Items) parameter.Write(newWriter);
        writer.WriteInt16((short)(newWriter.BaseStream.Position + 2));
        writer.WriteWriter(newWriter);
    }

    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        var size = reader.ReadUInt16();

        ObjectType = Settings.Old ? reader.ReadByte() : reader.ReadInt16();
        Num = Settings.Old ? reader.ReadByte() : reader.ReadInt16();
        if (ObjectType >= 2 && Num >= 48)
            if (Settings.Old)
                Num -= 32;
        ObjectInfo = reader.ReadUInt16();
        ObjectInfoList = reader.ReadInt16();
        Flags = reader.ReadSByte();
        OtherFlags = reader.ReadSByte();
        NumberOfParameters = reader.ReadByte();
        DefType = reader.ReadByte();
        Identifier = reader.ReadUInt16();
        if (CTFAKCore.Parameters.Contains("-noevnt")) return;
        for (var i = 0; i < NumberOfParameters; i++)
        {
            var item = new Parameter();
            item.Read(reader);
            Items.Add(item);
        }

        if (CTFAKCore.Parameters.Contains("-debug"))
            Logger.Log(this);
        //Console.ReadKey();
    }

    public override string ToString()
    {
        //return Preprocessor.ProcessCondition(this);
        //return $"Condition {(Constants.ObjectType)ObjectType}=={Names.ConditionNames[ObjectType][Num]}{(Items.Count > 0 ? "-"+Items[0].ToString() : " ")}";
        return
            $"Condition {(Constants.ObjectType)ObjectType}=={Num}{(Items.Count > 0 ? "-" + Items[0].Loader : " ")} Params: {Items.Count}. Object: ({ObjectInfo})-({ObjectType})";
    }
}

public class Action : ChunkLoader
{
    public int DefType;
    public int Flags;
    public List<Parameter> Items = new();
    public int Num;
    public byte NumberOfParameters;
    public int ObjectInfo;
    public int ObjectInfoList;
    public int ObjectType;
    public int OtherFlags;

    public override void Write(ByteWriter writer)
    {
        var newWriter = new ByteWriter(new MemoryStream());
        newWriter.WriteInt16((short)ObjectType);
        newWriter.WriteInt16((short)Num);
        newWriter.WriteUInt16((ushort)ObjectInfo);
        newWriter.WriteInt16((short)ObjectInfoList);
        newWriter.WriteUInt8((sbyte)Flags);
        newWriter.WriteUInt8((sbyte)OtherFlags);
        newWriter.WriteUInt8((sbyte)Items.Count);
        newWriter.WriteInt8((byte)DefType);

        foreach (var parameter in Items) parameter.Write(newWriter);

        writer.WriteUInt16((ushort)(newWriter.BaseStream.Position + 2));
        writer.WriteWriter(newWriter);
    }

    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        var size = reader.ReadUInt16();

        ObjectType = Settings.Old ? reader.ReadSByte() : reader.ReadInt16();
        Num = Settings.Old ? reader.ReadSByte() : reader.ReadInt16();
        if (ObjectType >= 2 && Num >= 48)
            if (Settings.Old)
                Num += 32;
        ObjectInfo = reader.ReadUInt16();
        ObjectInfoList = reader.ReadInt16();
        Flags = reader.ReadSByte();
        OtherFlags = reader.ReadSByte();
        NumberOfParameters = reader.ReadByte();
        DefType = reader.ReadByte();
        for (var i = 0; i < NumberOfParameters; i++)
        {
            var item = new Parameter();
            item.Read(reader);
            Items.Add(item);
        }

        if (CTFAKCore.Parameters.Contains("-debug"))
            Logger.Log(this);
    }

    public override string ToString()
    {
        return $"Action {ObjectType}-{Num}{(Items.Count > 0 ? "-" + Items[0].Loader : " ")} Params: {Items.Count}";
    }
}

public class Parameter : ChunkLoader
{
    public int Code;
    public ChunkLoader Loader;

    public object Value
    {
        get
        {
            if (Loader != null)
            {
                if (Loader.GetType().GetField("value") != null)
                    return Loader.GetType().GetField("value").GetValue(Loader);
                return null;
            }

            return null;
        }
    }

    public override void Write(ByteWriter writer)
    {
        var newWriter = new ByteWriter(new MemoryStream());
        newWriter.WriteInt16((short)Code);
        Loader.Write(newWriter);
        writer.WriteUInt16((ushort)(newWriter.BaseStream.Position + 2));
        writer.WriteWriter(newWriter);
    }

    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        var size = reader.ReadInt16();
        Code = reader.ReadInt16();

        var actualLoader = LoadParameter(Code, reader);
        Loader = actualLoader;
        if (Loader != null)
        {
            Loader.Read(reader);
        }
        else
        {
            Logger.LogWarning("Loader is null: " + Code);
            return;
        }

        reader.Seek(currentPosition + size);
    }

    public static ChunkLoader LoadParameter(int code, ByteReader reader)
    {
        ChunkLoader item = null;
        if (code == 1)
            item = new ParamObject();

        if (code == 2 || code == 42)
            item = new Time();

        if (code == 3 || code == 4 || code == 10 || code == 11 || code == 12 || code == 17 || code == 26 ||
            code == 31 ||
            code == 43 || code == 57 || code == 58 || code == 60 || code == 61)
            item = new Short();

        if (code == 5 || code == 25 || code == 29 || code == 34 || code == 48 || code == 56)
            item = new Int();

        if (code == 6 || code == 7 || code == 35 || code == 36)
            item = new Sample();

        if (code == 9 || code == 21)
            item = new Create();

        if (code == 13)
            item = new Every();

        if (code == 14 || code == 44)
            item = new KeyParameter();

        if (code == 15 || code == 22 || code == 23 || code == 27 || code == 28 || code == 45 || code == 46 ||
            code == 52 || code == 53 || code == 54 || code == 59 || code == 62)
            item = new ExpressionParameter();

        if (code == 16)
            item = new Position();

        if (code == 18)
            item = new Shoot();

        if (code == 19)
            item = new Zone();

        if (code == 24)
            item = new Colour();

        if (code == 40)
            item = new Filename();

        if (code == 50)
            item = new AlterableValue();

        if (code == 32)
            item = new Click();

        if (code == 33)
            item = new Program();

        if (code == 55)
            item = new Extension();

        if (code == 38)
            item = new Group();

        if (code == 39)
            item = new GroupPointer();

        if (code == 49)
            item = new GlobalValue();

        if (code == 41 || code == 64)
            item = new StringParam();

        if (code == 47 || code == 51)
            item = new TwoShorts();

        if (code == 67)
            item = new Int();

        if (code == 68)
            item = new MultipleVariables();

        if (code == 69)
            item = new ChildEvent();

        if (code == 70)
            item = new Int();

        return item;
    }
}