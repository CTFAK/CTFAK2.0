using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;


namespace CTFAK.CCN.Chunks.Frame
{
    public class Events : ChunkLoader
    {
        public readonly string Header = "ER>>";
        public readonly string EventCount = "ERes";
        public readonly string EventgroupData = "ERev";
        public readonly string End = "<<ER";

        public int MaxObjects;
        public int MaxObjectInfo;
        public int NumberOfPlayers;
        public Dictionary<int, Quailifer> QualifiersList = new Dictionary<int, Quailifer>();
        public List<int> NumberOfConditions = new List<int>();
        public List<EventGroup> Items = new List<EventGroup>();




        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }




        public override void Read()
        {
            // if (Settings.GameType == GameType.OnePointFive) return;
            while (true)
            {
                var identifier = reader.ReadAscii(4);
                if (identifier == Header)
                {
                    MaxObjects = reader.ReadInt16();
                    MaxObjectInfo = reader.ReadInt16();
                    NumberOfPlayers = reader.ReadInt16();
                    for (int i = 0; i < 17; i++)
                    {
                        NumberOfConditions.Add(reader.ReadInt16());
                    }

                    var qualifierCount = reader.ReadInt16(); //should be 0, so i dont care
                    for (int i = 0; i < qualifierCount; i++)
                    {
                        var newQualifier = new Quailifer(reader);
                        newQualifier.Read();
                        if (!QualifiersList.ContainsKey(newQualifier.ObjectInfo)) QualifiersList.Add(newQualifier.ObjectInfo, newQualifier);
                    }
                }
                else if (identifier == EventCount)
                {
                    if (Settings.android) reader.ReadInt32();//TODO: figure out what it is
                    var size = reader.ReadInt32();
                }
                else if (identifier == EventgroupData)
                {
                    var size = reader.ReadInt32();
                    if (Settings.android) size += 4;
                    
                    var endPosition = reader.Tell() + size;
                    if(Settings.android) reader.ReadInt32();
                    int i = 0;
                    while (true)
                    {
                        i++;
                        var eg = new EventGroup(reader);
                        eg.Read();
                        Items.Add(eg);

                        if (reader.Tell() >= endPosition) break;
                    }

                }
                else if (identifier == End) break;
            }
        }

        public Events(ByteReader reader) : base(reader)
        {
        }
    }

    public class Quailifer : ChunkLoader
    {
        public int ObjectInfo;
        public int Type;
        public int Qualifier;
        List<int> _objects = new List<int>();



        public Quailifer(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            ObjectInfo = reader.ReadUInt16();
            Type = reader.ReadInt16();
            Qualifier = ObjectInfo & 0b11111111111;
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt16((ushort)ObjectInfo);
            Writer.WriteInt16((short)Type);
        }


    }


    public class EventGroup : ChunkLoader
    {
        public ushort Flags;
        public int IsRestricted;
        public int RestrictCpt;
        public int Identifier;
        public int Undo;
        public List<Condition> Conditions = new List<Condition>();
        public List<Action> Actions = new List<Action>();
        public int Size;
        public byte NumberOfConditions;
        public byte NumberOfActions;
        public bool isMFA = false;



        public EventGroup(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            var currentPosition = reader.Tell();
            Size = reader.ReadInt16() * -1;
            NumberOfConditions = reader.ReadByte();
            NumberOfActions = reader.ReadByte();
            Flags = reader.ReadUInt16();
            if (Settings.android)
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
            for (int i = 0; i < NumberOfConditions; i++)
            {
               
                var item = new Condition(reader);
                item.Read();
                Fixer.FixConditions(ref item);
                Conditions.Add(item);
                
            }

            for (int i = 0; i < NumberOfActions; i++)
            {
                var item = new Action(reader);
                item.Read();
                Fixer.FixActions(ref item);
                Actions.Add(item);
            }
            reader.Seek(currentPosition + Size);
            // Logger.Log($"COND:{NumberOfConditions}, ACT: {NumberOfActions}");

        }

        public override void Write(ByteWriter Writer)
        {
            ByteWriter newWriter = new ByteWriter(new MemoryStream());
            newWriter.WriteInt8((byte)Conditions.Count);
            newWriter.WriteInt8((byte)Actions.Count);
            newWriter.WriteUInt16(Flags);
            if (Settings.Build >= 284)
            {
                if (isMFA)//For MFA
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

            foreach (Condition condition in Conditions)
            {
                var cond = condition;
                condition.Write(newWriter);
            }

            foreach (Action action in Actions)
            {
                var act = action;
                act.Write(newWriter);
            }
            Writer.WriteInt16((short)((newWriter.Size() + 2) * -1));

            Writer.WriteWriter(newWriter);


        }
    }

    public static class Fixer
    {
        public static void FixConditions(ref Condition cond)
        {
            var num = cond.Num;
            //Alterable Values:
            if (num == -42) num = -27;
            //Global Values
                if(cond.ObjectType==-1)
                if (num == -28||num == -29||num == -30||num == -31||num == -32||num == -33)
                    num = -8;
            cond.Num = num;
        }
        public static void FixActions(ref Action act)
        {
            var num = act.Num;
            var type = act.ObjectType;
            if(type==-1)
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
        public int Flags;
        public int OtherFlags;
        public int DefType;
        public int NumberOfParameters;
        public int ObjectType;
        public int Num;
        public int ObjectInfo;
        public int Identifier;
        public int ObjectInfoList;
        public List<Parameter> Items = new List<Parameter>();

        public Condition(ByteReader reader) : base(reader) { }
        public override void Write(ByteWriter Writer)
        {
            ByteWriter newWriter = new ByteWriter(new MemoryStream());
            // Logger.Log($"{ObjectType}-{Num}-{ObjectInfo}-{ObjectInfoList}-{Flags}-{OtherFlags}-{Items.Count}-{DefType}-{Identifier}");
            newWriter.WriteInt16((short)ObjectType);
            newWriter.WriteInt16((short)Num);
            newWriter.WriteUInt16((ushort)ObjectInfo);
            newWriter.WriteInt16((short)ObjectInfoList);
            newWriter.WriteUInt8((sbyte)Flags);
            newWriter.WriteUInt8((sbyte)OtherFlags);
            newWriter.WriteUInt8((sbyte)Items.Count);
            newWriter.WriteInt8((byte)DefType);
            newWriter.WriteUInt16((ushort)(Identifier));
            foreach (Parameter parameter in Items)
            {
                parameter.Write(newWriter);
            }
            Writer.WriteInt16((short)(newWriter.BaseStream.Position + 2));
            Writer.WriteWriter(newWriter);


        }



        public override void Read()
        {
            var currentPosition = reader.Tell();
            var size = reader.ReadUInt16();

            ObjectType =reader.ReadInt16();
            Num = reader.ReadInt16();
            ObjectInfo = reader.ReadUInt16();
            ObjectInfoList = reader.ReadInt16();
            Flags = reader.ReadSByte();
            OtherFlags = reader.ReadSByte();
            NumberOfParameters = reader.ReadByte();
            DefType = reader.ReadByte();
            Identifier = reader.ReadInt16();
            for (int i = 0; i < NumberOfParameters; i++)
            {
                var item = new Parameter(reader);
                item.Read();
                Items.Add(item);
            }
            
            //Logger.Log(this);
            //Console.ReadKey();


        }
        public override string ToString()
        {
            //return Preprocessor.ProcessCondition(this);
            //return $"Condition {(Constants.ObjectType)ObjectType}=={Names.ConditionNames[ObjectType][Num]}{(Items.Count > 0 ? "-"+Items[0].ToString() : " ")}";
            return $"Condition {(Constants.ObjectType)ObjectType}=={Num}{(Items.Count > 0 ? "-" + Items[0].ToString() : " ")}";
        }
    }

    public class Action : ChunkLoader
    {
        public int Flags;
        public int OtherFlags;
        public int DefType;
        public int ObjectType;
        public int Num;
        public int ObjectInfo;
        public int ObjectInfoList;
        public List<Parameter> Items = new List<Parameter>();
        public byte NumberOfParameters;
        public Action(ByteReader reader) : base(reader) { }
        public override void Write(ByteWriter Writer)
        {
            ByteWriter newWriter = new ByteWriter(new MemoryStream());
            newWriter.WriteInt16((short)ObjectType);
            newWriter.WriteInt16((short)Num);
            newWriter.WriteUInt16((ushort)ObjectInfo);
            newWriter.WriteInt16((short)ObjectInfoList);
            newWriter.WriteUInt8((sbyte)Flags);
            newWriter.WriteUInt8((sbyte)OtherFlags);
            newWriter.WriteUInt8((sbyte)Items.Count);
            newWriter.WriteInt8((byte)DefType);

            foreach (Parameter parameter in Items)
            {
                parameter.Write(newWriter);
            }
            Writer.WriteUInt16((ushort)(newWriter.BaseStream.Position + 2));
            Writer.WriteWriter(newWriter);

        }



        public override void Read()
        {
            var old = false;
            var currentPosition = reader.Tell();
            var size = reader.ReadUInt16();
            ObjectType = old ? reader.ReadSByte() : reader.ReadInt16();
            Num = old ? reader.ReadSByte() : reader.ReadInt16();
            if ((int)ObjectType >= 2 && Num >= 48)
            {
                if (old) Num += 32;
            }
            ObjectInfo = reader.ReadUInt16();
            ObjectInfoList = reader.ReadInt16();
            Flags = reader.ReadSByte();
            OtherFlags = reader.ReadSByte();
            NumberOfParameters = reader.ReadByte();
            DefType = reader.ReadByte();
            for (int i = 0; i < NumberOfParameters; i++)
            {
                var item = new Parameter(reader);
                item.Read();
                Items.Add(item);
            }
            //Logger.Log(this);

        }
        public override string ToString()
        {

            return $"Action {ObjectType}-{Num}{(Items.Count > 0 ? "-" + Items[0].ToString() : " ")}";

        }
    }

    public class Parameter : ChunkLoader
    {
        public int Code;
        public ChunkLoader Loader;

        public Parameter(ByteReader reader) : base(reader) { }

        public override void Write(ByteWriter Writer)
        {
            var newWriter = new ByteWriter(new MemoryStream());
            newWriter.WriteInt16((short)Code);
            Loader.Write(newWriter);
            Writer.WriteUInt16((ushort)(newWriter.BaseStream.Position + 2));
            Writer.WriteWriter(newWriter);


        }



        public override void Read()
        {
            var currentPosition = reader.Tell();
            var size = reader.ReadInt16();
            Code = reader.ReadInt16();
            //Console.WriteLine(Code);

            var actualLoader = Parameter.LoadParameter(Code, reader);
            this.Loader = actualLoader;
            //// Loader?.Read();
            if (Loader != null) Loader.Read();
            else throw new Exception("Loader is null: " + Code);

            reader.Seek(currentPosition + size);

        }
        public object Value
        {
            get
            {
                if (Loader != null)
                {


                    if (Loader.GetType().GetField("value") != null)
                    {
                        return Loader.GetType().GetField("value").GetValue(Loader);
                    }
                    else
                    {
                        return null;
                    }
                }
                else return null;
            }
        }
        public static ChunkLoader LoadParameter(int code, ByteReader reader)
        {
            ChunkLoader item = null;
            if (code == 1)
            {
                item = new ParamObject(reader);
            }

            if (code == 2)
            {
                item = new Time(reader);
            }
            if (code == 3 || code == 4 || code == 10 || code == 11 || code == 12 || code == 17 || code == 26 || code == 31 ||
                code == 43 || code == 57 || code == 58 || code == 60 || code == 61)
            {
                item = new Short(reader);
            }
            if (code == 5 || code == 25 || code == 29 || code == 34 || code == 48 || code == 56)
            {
                item = new Int(reader);
            }
            if (code == 6 || code == 7 || code == 35 || code == 36)
            {
                item = new Sample(reader);
            }
            if (code == 9 || code == 21)
            {
                item = new Create(reader);
            }
            if (code == 13)
            {
                item = new Every(reader);
            }
            if (code == 14 || code == 44)
            {
                item = new KeyParameter(reader);
            }
            if (code == 15 || code == 22 || code == 23 || code == 27 || code == 28 || code == 45 || code == 46 || code == 52 || code == 53 || code == 54 || code == 59 || code == 62)
            {
                item = new ExpressionParameter(reader);
            }
            if (code == 16)
            {
                item = new Position(reader);
            }
            if (code == 18)
            {
                item = new Shoot(reader);
            }
            if (code == 19)
            {
                item = new Zone(reader);
            }
            if (code == 24)
            {
                item = new Colour(reader);
            }

            if (code == 40)
            {
                item = new Filename(reader);
            }
            if (code == 50)
            {
                item = new AlterableValue(reader);
            }

            if (code == 32)
            {
                item = new Click(reader);
            }

            if (code == 33)
            {
                item = new MMFParser.EXE.Loaders.Events.Parameters.Program(reader);
            }

            if (code == 55)
            {
                item = new Extension(reader);
            }

            if (code == 38)
            {
                item = new CTFAK.MMFParser.EXE.Loaders.Events.Parameters.Group(reader);
            }

            if (code == 39)
            {
                item = new GroupPointer(reader);
            }

            if (code == 49)
            {
                item = new GlobalValue(reader);
            }

            if (code == 41 || code == 64)
            {
                item = new StringParam(reader);
            }

            if (code == 47 || code == 51)
            {
                item = new TwoShorts(reader);
            }
            if (code == 68) item = new MultipleVariables(reader);
            if (code == 69) item = new ChildEvent(reader);









            return item;
        }

    }



}