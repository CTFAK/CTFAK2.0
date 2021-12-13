using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTFAK.CCN.Constants;

namespace CTFAK.CCN.Chunks.Objects
{
    public class ObjectCommon : ChunkLoader
    {
        private ushort _valuesOffset;
        private ushort _stringsOffset;
        private uint _fadeinOffset;
        private uint _fadeoutOffset;
        private ushort _movementsOffset;
        private ushort _animationsOffset;
        private ushort _systemObjectOffset;
        private ushort _counterOffset;
        private ushort _extensionOffset;
        public string Identifier;

        public Animations Animations;

        public BitDict Preferences = new BitDict(new string[]
            {
                "Backsave",
                "ScrollingIndependant",
                "QuickDisplay",
                "Sleep",
                "LoadOnCall",
                "Global",
                "BackEffects",
                "Kill",
                "InkEffects",
                "Transitions",
                "FineCollisions",
                "AppletProblems"
            }
        );

        public BitDict Flags = new BitDict(new string[]
            {
                "DisplayInFront",
                "Background",
                "Backsave",
                "RunBeforeFadeIn",
                "Movements",
                "Animations",
                "TabStop",
                "WindowProc",
                "Values",
                "Sprites",
                "InternalBacksave",
                "ScrollingIndependant",
                "QuickDisplay",
                "NeverKill",
                "NeverSleep",
                "ManualSleep",
                "Text",
                "DoNotCreateAtStart",
                "FakeSprite",
                "FakeCollisions"
            }
        );

        public BitDict NewFlags = new BitDict(new string[]
            {
                "DoNotSaveBackground",
                "SolidBackground",
                "CollisionBox",
                "VisibleAtStart",
                "ObstacleSolid",
                "ObstaclePlatform",
                "AutomaticRotation"
            }
        );

        public Color BackColor;
        public ObjectInfo Parent;
        public Counters Counters;
        public byte[] ExtensionData;
        public int ExtensionPrivate;
        public int ExtensionId;
        public int ExtensionVersion;
        //public AlterableValues Values;
        //public AlterableStrings Strings;
        public Movements Movements;
        public Text Text;
        public Counter Counter;
        public short[] _qualifiers = new short[8];
        public ObjectCommon(ByteReader reader) : base(reader) { }
        public ObjectCommon(ByteReader reader,ObjectInfo parent) : base(reader) { this.Parent = parent; }
        public override void Read()
        {
            var currentPosition = reader.Tell();
            if (Settings.Build >= 284)
            {
                var size = reader.ReadInt32();
                _animationsOffset = reader.ReadUInt16();
                _movementsOffset = reader.ReadUInt16();
                var version = reader.ReadUInt16();
                reader.Skip(2);
                _extensionOffset = reader.ReadUInt16();
                _counterOffset = reader.ReadUInt16();
                Flags.flag = reader.ReadUInt16();
                reader.Skip(2);
                var end = reader.Tell() + 8 * 2;
                for (int i = 0; i < 8; i++)
                {
                    _qualifiers[i] = reader.ReadInt16();
                }

                reader.Seek(end);

                _systemObjectOffset = reader.ReadUInt16();

                _valuesOffset = reader.ReadUInt16();
                _stringsOffset = reader.ReadUInt16();
                NewFlags.flag = reader.ReadUInt16();
                Preferences.flag = reader.ReadUInt16();
                Identifier = reader.ReadAscii(4);
                BackColor = reader.ReadColor();
                _fadeinOffset = reader.ReadUInt32();
                _fadeoutOffset = reader.ReadUInt32();
            }
            else
            {
                var size = reader.ReadInt32();
                _movementsOffset = reader.ReadUInt16();
                _animationsOffset = reader.ReadUInt16();
                var version = reader.ReadUInt16();
                _counterOffset = reader.ReadUInt16();
                _systemObjectOffset = reader.ReadUInt16();
                reader.Skip(2);
                Flags.flag = reader.ReadUInt32();
                //reader.Skip(2);
                var end = reader.Tell() + 8 * 2;
                for (int i = 0; i < 8; i++)
                {
                    _qualifiers[i] = reader.ReadInt16();
                }

                reader.Seek(end);

                _extensionOffset = reader.ReadUInt16();

                _valuesOffset = reader.ReadUInt16();
                _stringsOffset = reader.ReadUInt16();
                NewFlags.flag = reader.ReadUInt16();
                Preferences.flag = reader.ReadUInt16();
                Identifier = reader.ReadAscii(2);
                BackColor = reader.ReadColor();
                _fadeinOffset = reader.ReadUInt32();
                _fadeoutOffset = reader.ReadUInt32();
            }


            if (_animationsOffset > 0)
            {
                reader.Seek(currentPosition + _animationsOffset);
                Animations = new Animations(reader);
                Animations.Read();
            }


            if (_movementsOffset > 0)
            {
                reader.Seek(currentPosition + _movementsOffset);

                    Movements = new Movements(reader);
                    Movements.Read();
                

            }

            if (_systemObjectOffset > 0)
            {
                reader.Seek(currentPosition + _systemObjectOffset);
                switch (((ObjectType)Parent.ObjectType))
                {
                    //Text
                    case Constants.ObjectType.Text:
                        Text = new Text(reader);
                        Text.Read();
                        break;
                    //Counter
                    case Constants.ObjectType.Counter:
                    case Constants.ObjectType.Score:
                    case Constants.ObjectType.Lives:
                        Counters = new Counters(reader);
                        Counters.Read();
                        break;

                }
            }

            if (_extensionOffset > 0)
            {

                    reader.Seek(currentPosition + _extensionOffset);

                    var dataSize = reader.ReadInt32() - 20;
                    reader.Skip(4); //maxSize;
                    ExtensionVersion = reader.ReadInt32();
                    ExtensionId = reader.ReadInt32();
                    ExtensionPrivate = reader.ReadInt32();
                    if (dataSize != 0)
                    {
                        ExtensionData = reader.ReadBytes(dataSize);
                    }
                    else ExtensionData = new byte[0];
              
            }

            if (_counterOffset > 0)
            {
                reader.Seek(currentPosition + _counterOffset);
                Counter = new Counter(reader);
                Counter.Read();
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
