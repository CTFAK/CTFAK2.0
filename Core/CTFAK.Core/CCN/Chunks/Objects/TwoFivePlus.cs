using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;
using Ionic.Zlib;

namespace CTFAK.CCN.Chunks.Objects
{
    public class TwoFilePlusContainer
    {
        public TwoFilePlusContainer()
        {
            instance = this;
        }
        public static TwoFilePlusContainer instance;
        public Dictionary<int, ObjectInfo> objectsContainer = new Dictionary<int, ObjectInfo>();
    }
    [ChunkLoader(8790, "TwoFivePlusProperties")]
    public class TwoFilePlusProps : ChunkLoader
    {
        public override void Read(ByteReader reader)
        {
            var start = reader.Tell();
            var end = start + reader.Size();
            if (start == end) return;
            reader.ReadInt32();

            int current = 0;
            while (reader.Tell() <= end)
            {
                var currentPosition = reader.Tell();
                var chunkSize = reader.ReadInt32();
                var data = reader.ReadBytes(chunkSize);
                var decompressed = ZlibStream.UncompressBuffer(data);
                var decompressedReader = new ByteReader(decompressed);
                var objectData = TwoFilePlusContainer.instance.objectsContainer[current];

                if (objectData.ObjectType == 0)
                    objectData.properties = new Quickbackdrop();
                else if (objectData.ObjectType == 1)
                    objectData.properties = new Backdrop();
                else
                    objectData.properties = new ObjectCommon(null);

                objectData.properties.Read(decompressedReader);
                TwoFilePlusContainer.instance.objectsContainer[current] = objectData;
                reader.Seek(currentPosition + chunkSize + 8);
                current++;
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }

    [ChunkLoader(8788, "TwoFivePlusNames")]
    public class TwoFivePlusNames : ChunkLoader
    {
        public override void Read(ByteReader reader)
        {
            var nstart = reader.Tell();

            var nend = nstart + reader.Size();
            //reader.ReadInt32();
            int ncurrent = 0;
            while (reader.Tell() < nend)
            {
                var newName = "sex";

                TwoFilePlusContainer.instance.objectsContainer[ncurrent].name = reader.ReadUniversal();
                ncurrent++;
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
    [ChunkLoader(8787, "TwoFivePlusHeaders")]
    public class TwoFivePlusHeaders : ChunkLoader
    {
        public override void Read(ByteReader reader)
        {
            new TwoFilePlusContainer();
            while (true)
            {
                if (reader.Tell() >= reader.Size()) break;
                var newObject = new ObjectInfo();
                newObject.handle = reader.ReadInt16();
                newObject.ObjectType = reader.ReadInt16();
                newObject.Flags = reader.ReadInt16();
                reader.Skip(2);
                newObject.InkEffect = reader.ReadByte();
                if (newObject.InkEffect != 1)
                {
                    reader.Skip(3);
                    var r = reader.ReadByte();
                    var g = reader.ReadByte();
                    var b = reader.ReadByte();
                    newObject.rgbCoeff = Color.FromArgb(0, b, g, r);
                    newObject.blend = reader.ReadByte();
                }
                else
                {
                    var flag = reader.ReadByte();
                    reader.Skip(2);
                    newObject.InkEffectValue = reader.ReadByte();
                    reader.Skip(3);
                }
                TwoFilePlusContainer.instance.objectsContainer.Add(newObject.handle, newObject);
            }
        }
        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }
    }
    /*[ChunkLoader(8791, "TwoFivePlusShaders")]
    public class TwoFivePlusShaders : ChunkLoader
    {
        public override void Read(ByteReader reader)
        {
            while (true)
            {
                var start = reader.Tell();
                var end = start + reader.Size();
                if (start == end) return;
                reader.ReadInt32();

                int current = 0;
                while (start <= end)
                {
                    TwoFilePlusContainer.instance.objectsContainer[current].shaderId = reader.ReadInt32();
                    var count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var newReader = new ByteReader(new MemoryStream(reader.ReadBytes(4)));
                        TwoFilePlusContainer.instance.objectsContainer[current].effectItems.Add(newReader);
                        Logger.Log("Loading Shader " + newReader.ReadInt32() + " on " + TwoFilePlusContainer.instance.objectsContainer[current].name);
                    }
                    current++;
                }
            }
        }

        public override void Write(ByteWriter Writer)
        {

        }*/
}