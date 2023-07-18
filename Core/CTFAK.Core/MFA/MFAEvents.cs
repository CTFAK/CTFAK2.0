using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA
{
    public class MFAEvents : ChunkLoader
    {
        public const string EventData = "Evts";
        public const string CommentData = "Rems";
        public const string ObjectData = "EvOb";
        public const string EventEditorData = "EvCs";
        public const string ObjectListData = "EvEd";
        public const string TimeListData = "EvEd";
        public const string EditorPositionData = "EvTs";
        public const string EditorLineData = "EvLs";
        public const string UnknownEventData = "E2Ts";
        public const string EventEnd = "!DNE";
        public List<EventGroup> Items=new List<EventGroup>();
        public ushort Version;
        public ushort FrameType;
        public List<Comment> Comments=new List<Comment>();
        public List<EventObject> Objects=new List<EventObject>();
        public ushort ConditionWidth;
        public ushort ObjectHeight;
        public List<ushort> ObjectTypes=new List<ushort>();
        public List<ushort> ObjectHandles=new List<ushort>();
        public List<ushort> ObjectFlags=new List<ushort>();
        public List<string> Folders=new List<string>();
        public uint X;
        public uint Y;
        public uint CaretType;
        public uint CaretX;
        public uint CaretY;
        public uint LineY;
        public uint LineItemType;
        public uint EventLine;
        public uint EventLineY;
        public byte[] Saved;
        public int EditorDataUnk;
        public uint EventDataLen;
        public uint CommentDataLen;
        public byte[] _cache;
        public bool _ifMFA;





        public override void Read(ByteReader reader)
        {

            Version = reader.ReadUInt16();
            FrameType = reader.ReadUInt16();
            Items = new List<EventGroup>();

            while (true)
            {

                string name = reader.ReadAscii(4);
                if (name == EventData)
                {
                    EventDataLen = reader.ReadUInt32();
                    uint end = (uint)(reader.Tell() + EventDataLen);
                    while (true)
                    {
                        EventGroup evGrp = new EventGroup();
                        evGrp.isMFA = true;
                        evGrp.Read(reader);
                        Items.Add(evGrp);
                        if (reader.Tell() >= end) break;
                    }
                }
                else if (name == CommentData)
                {
                    try
                    {
                        CommentDataLen = reader.ReadUInt32();
                        Comments = new List<Comment>();
                        Comment comment = new Comment();
                        comment.Read(reader);
                        Comments.Add(comment);
                    }
                    catch
                    {
                        //What the fuck?

                        /*
                        import code
                        code.interact(local = locals())
                        */
                    }
                }
                else if (name == ObjectData)
                {
                    Objects = new List<EventObject>();
                    uint len = reader.ReadUInt32();
                    for (int i = 0; i < len; i++)
                    {
                        EventObject eventObject = new EventObject();
                        eventObject.Read(reader);
                        Objects.Add(eventObject);

                    }
                }
                else if (name == EventEditorData)
                {
                    EditorDataUnk = reader.ReadInt32();
                    ConditionWidth = reader.ReadUInt16();
                    ObjectHeight = reader.ReadUInt16();
                    reader.Skip(12);
                }
                else if (name == ObjectListData)
                {
                    short count = reader.ReadInt16();
                    short realCount = count;
                    if (count == -1)
                    {
                        realCount = reader.ReadInt16();
                    }

                    ObjectTypes = new List<ushort>();
                    for (int i = 0; i < realCount; i++)
                    {
                        ObjectTypes.Add(reader.ReadUInt16());
                    }
                    ObjectHandles = new List<ushort>();
                    for (int i = 0; i < realCount; i++)
                    {
                        ObjectHandles.Add(reader.ReadUInt16());
                    }
                    ObjectFlags = new List<ushort>();
                    for (int i = 0; i < realCount; i++)
                    {
                        ObjectFlags.Add(reader.ReadUInt16());
                    }

                    if (count == -1)
                    {
                        Folders = new List<string>();
                        var folderCount = reader.ReadUInt16();
                        for (int i = 0; i < folderCount; i++)
                        {
                            Folders.Add(reader.AutoReadUnicode());
                        }
                    }
                }
                else if (name == TimeListData)
                {
                    throw new NotImplementedException("I don't like no timelist");
                }
                else if (name == EditorPositionData)
                {
                    if (reader.ReadUInt16() != 1)//throw new NotImplementedException("Invalid chunkversion");
                        X = reader.ReadUInt32();
                    Y = reader.ReadUInt32();
                    CaretType = reader.ReadUInt32();
                    CaretX = reader.ReadUInt32();
                    CaretY = reader.ReadUInt32();
                }
                else if (name == EditorLineData)
                {
                    if (reader.ReadUInt16() != 1)//throw new NotImplementedException("Invalid chunkversion");
                        LineY = reader.ReadUInt32();
                    LineItemType = reader.ReadUInt32();
                    EventLine = reader.ReadUInt32();
                    EventLineY = reader.ReadUInt32();
                }
                else if (name == UnknownEventData)
                {
                    reader.Skip(12);
                }
                else if (name == EventEnd)
                {
                    // _cache = reader.ReadBytes(122);

                    break;
                }
                else Logger.Log("UnknownGroup: " + name);//throw new NotImplementedException("Fuck Something is Broken: "+name);

            }
        }

        public override void Write(ByteWriter Writer)
        {

            Writer.WriteUInt16(Version);
            Writer.WriteUInt16(FrameType);
            if (Items.Count > 0)
            {
                Writer.WriteAscii(EventData);

                ByteWriter newWriter = new ByteWriter(new MemoryStream());
                //Writer.WriteUInt32(EventDataLen);

                foreach (EventGroup eventGroup in Items)
                {
                    eventGroup.isMFA = true;
                    eventGroup.Write(newWriter);
                }


                Writer.WriteUInt32((uint)newWriter.BaseStream.Position);
                Writer.WriteWriter(newWriter);

            }


            if (Objects?.Count > 0)
            {
                Writer.WriteAscii(ObjectData);
                Writer.WriteUInt32((uint)Objects.Count);
                foreach (EventObject eventObject in Objects)
                {
                    eventObject.Write(Writer);
                }
            }
            if (ObjectTypes != null)
            {
                Writer.WriteAscii(ObjectListData);
                Writer.WriteInt16(-1);
                Writer.WriteInt16((short)ObjectTypes.Count);
                foreach (ushort objectType in ObjectTypes)
                {
                    Writer.WriteUInt16(objectType);
                }

                foreach (ushort objectHandle in ObjectHandles)
                {
                    Writer.WriteUInt16(objectHandle);
                }

                foreach (ushort objectFlag in ObjectFlags)
                {
                    Writer.WriteUInt16(objectFlag);
                }

                Writer.WriteUInt16((ushort)Folders.Count);
                foreach (string folder in Folders)
                {
                    Writer.AutoWriteUnicode(folder);
                }
            }





            // if (X != 0)
            {
                Writer.WriteAscii(EditorPositionData);
                Writer.WriteInt16(10);
                Writer.WriteInt32((int)X);
                Writer.WriteInt32((int)Y);
                Writer.WriteUInt32(CaretType);
                Writer.WriteUInt32(CaretX);
                Writer.WriteUInt32(CaretY);
            }
            // if (LineY != 0)
            {
                Writer.WriteAscii(EditorLineData);
                Writer.WriteInt16(10);
                Writer.WriteUInt32(LineY);
                Writer.WriteUInt32(LineItemType);
                Writer.WriteUInt32(EventLine);
                Writer.WriteUInt32(EventLineY);
            }
            Writer.WriteAscii(UnknownEventData);
            Writer.WriteInt8(8);
            Writer.Skip(9);
            Writer.WriteInt16(0);

            Writer.WriteAscii(EventEditorData);
            // Writer.Skip(4+2*2+4*3);
            Writer.WriteInt32(EditorDataUnk);
            Writer.WriteInt16((short)ConditionWidth);
            Writer.WriteInt16((short)ObjectHeight);
            Writer.Skip(12);


            Writer.WriteAscii(EventEnd);

            // Writer.WriteBytes(_cache);



            //TODO: Fix commented part
            // 

            //
            // if (Comments != null)
            // {
            //     Console.WriteLine("Writing Comments");
            //     Writer.WriteAscii(CommentData);
            //     foreach (Comment comment in Comments)
            //     {
            //         comment.Write(Writer);
            //     }
            // }

        }


    }

    public class Comment : ChunkLoader
    {
        public uint Handle;
        public string Value;
        
        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadUInt32();
            Value = reader.AutoReadUnicode();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt32(Handle);
            Writer.AutoWriteUnicode(Value);
        }


    }

    public class EventObject : ChunkLoader
    {
        public uint Handle;
        public ushort ObjectType;
        public ushort ItemType;
        public string Name;
        public string TypeName;
        public ushort Flags;
        public uint ItemHandle;
        public uint InstanceHandle;
        public string Code;
        public string IconBuffer;
        public ushort SystemQualifier;




        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadUInt32();
            ObjectType = reader.ReadUInt16();
            ItemType = reader.ReadUInt16();
            Name = reader.AutoReadUnicode();//Not Sure
            TypeName = reader.AutoReadUnicode();//Not Sure
            Flags = reader.ReadUInt16();
            if (ObjectType == 1)//FrameItemType
            {
                ItemHandle = reader.ReadUInt32();
                InstanceHandle = reader.ReadUInt32();
            }
            else if (ObjectType == 2)//ShortcutItemType
            {
                Code = reader.ReadAscii(4);
                //Logger.Log("Code: " + Code);
                if (Code == "OIC2")//IconBufferCode
                {
                    IconBuffer = reader.AutoReadUnicode();
                }
            }
            if (ObjectType == 3) //SystemItemType
            {
                SystemQualifier = reader.ReadUInt16();
            }

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt32(Handle);
            Writer.WriteUInt16(ObjectType);
            Writer.WriteUInt16(ItemType);
            Writer.AutoWriteUnicode(Name);//Not Sure
            Writer.AutoWriteUnicode(TypeName);//Not Sure
            Writer.WriteUInt16(Flags);
            if (ObjectType == 1)
            {
                Writer.WriteUInt32(ItemHandle);
                Writer.WriteUInt32(InstanceHandle);
            }
            else if (ObjectType == 2)
            {
                // Code = "OIC2";
                Writer.WriteAscii(Code);
                if (Code == "OIC2")
                {
                    Writer.AutoWriteUnicode(IconBuffer);
                }
            }
            if (ObjectType == 3)
            {
                Writer.WriteUInt16(SystemQualifier);
            }


        }


    }
}