using System;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class ObjectCommon : ChunkLoader
{
    private short _animationsOffset;
    private short _counterOffset;
    private short _extensionOffset;
    private uint _fadeinOffset;
    private uint _fadeoutOffset;
    private short _movementsOffset;
    private short _stringsOffset;
    private short _systemObjectOffset;
    private short _valuesOffset;

    public Animations Animations;

    public Color BackColor;
    public Counter Counter;
    public Counters Counters;
    public byte[] ExtensionData;
    public int ExtensionId;
    public int ExtensionPrivate;
    public int ExtensionVersion;

    public BitDict Flags = new(new[]
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

    public string Identifier;
    public Movements Movements;

    public BitDict NewFlags = new(new[]
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

    public ObjectInfo Parent;

    public BitDict Preferences = new(new[]
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

    public short[] Qualifiers = new short[8];
    public AlterableStrings Strings;
    public SubApplication SubApplication;
    public Text Text;
    public AlterableValues Values;

    public ObjectCommon(ObjectInfo parent)
    {
        Parent = parent;
    }

    public override void Read(ByteReader reader)
    {
        if (Settings.Android) return;
        var currentPosition = reader.Tell();

        //START NEW PART. This has no support for 290+ android games. I will ask Yuni to rewrite it once I officially come back
        var size = 0;
        if (Settings.Old)
            size = reader.ReadInt16();
        else size = reader.ReadInt32();
        if (Settings.Old)
        {
            var checksum = reader.ReadInt16();
        }

        //offset 4
        if (Settings.Build >= 284 && !Settings.Old && !Settings.Android && !Settings.CBM)
        {
            _animationsOffset = reader.ReadInt16();
            _movementsOffset = reader.ReadInt16();
        }
        else
        {
            _movementsOffset = reader.ReadInt16();
            _animationsOffset = reader.ReadInt16();
        }

        //offset 8
        var version = reader.ReadInt16();

        //offset 10
        if (Settings.Build < 284 || Settings.Old || Settings.Android)
            _counterOffset = reader.ReadInt16();
        else reader.Skip(2);

        //offset 12
        if (Settings.Build < 284 || Settings.Old || Settings.Android)
            _systemObjectOffset = reader.ReadInt16();
        else _extensionOffset = reader.ReadInt16();

        //offset 14
        if (Settings.Build >= 284)
            _counterOffset = reader.ReadInt16();
        else reader.Skip(2); // either a zero byte or ocVariable part1

        //offset 16
        if (!Settings.Old)
            Flags.flag = reader.ReadUInt16();
        else
            reader.Skip(2); //ocVariable part2

        //offset 18
        if (Settings.Old)
        {
            Flags.flag = (uint)reader.ReadInt16();
        }
        else
        {
            var penisFlags = reader.ReadInt16();
            if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;
        }

        //offset 20
        var end = reader.Tell() + 8 * 2;
        for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

        //offset 36
        if (Settings.Old || Settings.Build < 284 || Settings.Android)
            _extensionOffset = reader.ReadInt16();
        else _systemObjectOffset = reader.ReadInt16();

        //offset 38
        _valuesOffset = reader.ReadInt16();

        if (!Settings.Old)
            _stringsOffset = reader.ReadInt16();

        //Can't calculate offset here, because old games are 2 bytes shorter. Use common sense to figure it out
        NewFlags.flag = reader.ReadUInt16();
        Preferences.flag = reader.ReadUInt16();
        Identifier = reader.ReadAscii(4);
        BackColor = reader.ReadColor();
        _fadeinOffset = reader.ReadUInt32();
        _fadeoutOffset = reader.ReadUInt32();

        //END NEW PART
        /*if (Settings.Build >= 284 && Settings.gameType == Settings.GameType.NORMAL)
        {
            var size = reader.ReadInt32();
            _animationsOffset = reader.ReadInt16();
            _movementsOffset = reader.ReadInt16();
            var version = reader.ReadUInt16();
            reader.Skip(2);
            _extensionOffset = reader.ReadInt16();
            _counterOffset = reader.ReadInt16();

            Flags.flag = reader.ReadUInt16();
            var penisFlags = reader.ReadInt16();
            if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;

            var end = reader.Tell() + 8 * 2;
            for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

            reader.Seek(end);

            _systemObjectOffset = reader.ReadInt16();
            _valuesOffset = reader.ReadInt16();
            _stringsOffset = reader.ReadInt16();
            NewFlags.flag = reader.ReadUInt16();
            Preferences.flag = reader.ReadUInt16();
            Identifier = reader.ReadAscii(4);
            BackColor = reader.ReadColor();
            _fadeinOffset = reader.ReadUInt32();
            _fadeoutOffset = reader.ReadUInt32();
        }
        else if (Settings.gameType == Settings.GameType.NORMAL)
        {
            //I have absolutely no idea what this is. Probably used for custom builder, but I'm not sure
            Logger.Log("Is this it?");
            var size = reader.ReadInt32();
            _movementsOffset = reader.ReadInt16();
            _animationsOffset = reader.ReadInt16();
            var version = reader.ReadInt16();
            _counterOffset = reader.ReadInt16();
            _systemObjectOffset = reader.ReadInt16();
            reader.Skip(2);
            Flags.flag = reader.ReadUInt16();
            var penisFlags = reader.ReadInt16();
            //reader.Skip(2);
            if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;
            var end = reader.Tell() + 8 * 2;
            for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

            reader.Seek(end);

            _extensionOffset = reader.ReadInt16();
            _valuesOffset = reader.ReadInt16();
            _stringsOffset = reader.ReadInt16();
            NewFlags.flag = reader.ReadUInt16();
            Preferences.flag = reader.ReadUInt16();
            Identifier = reader.ReadAscii(4); //Is this right
            Logger.Log(Identifier);
            BackColor = reader.ReadColor();
            _fadeinOffset = reader.ReadUInt32();
            _fadeoutOffset = reader.ReadUInt32();
        }
        else if (Settings.TwoFivePlus)
        {
            var size = reader.ReadInt32();
            _animationsOffset = reader.ReadInt16();
            _movementsOffset = reader.ReadInt16();
            var version = reader.ReadUInt16();
            reader.Skip(2);
            _extensionOffset = reader.ReadInt16();
            _counterOffset = reader.ReadInt16();
            Flags.flag = reader.ReadUInt16();
            var penisFlags = reader.ReadInt16();
            if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;
            var end = reader.Tell() + 8 * 2;
            for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

            reader.Seek(end);

            _systemObjectOffset = reader.ReadInt16();
            _valuesOffset = reader.ReadInt16();
            _stringsOffset = reader.ReadInt16();
            NewFlags.flag = reader.ReadUInt16();
            Preferences.flag = reader.ReadUInt16();
            Identifier = reader.ReadAscii(4);
            BackColor = reader.ReadColor();
            _fadeinOffset = reader.ReadUInt32();
            _fadeoutOffset = reader.ReadUInt32();
        }
        else if (Settings.Old)
        {
            var size = reader.ReadUInt16();
            var checksum = reader.ReadUInt16();
            _movementsOffset = reader.ReadInt16();
            _animationsOffset = reader.ReadInt16();
            var version = reader.ReadUInt16();
            _counterOffset = reader.ReadInt16();
            _systemObjectOffset = reader.ReadInt16();
            var ocVariable = reader.ReadUInt32();
            Flags.flag = reader.ReadUInt16();

            var end = reader.Tell() + 8 * 2;
            for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

            reader.Seek(end);

            _extensionOffset = reader.ReadInt16();
            _valuesOffset = reader.ReadInt16();
            NewFlags.flag = reader.ReadUInt16();
            Preferences.flag = reader.ReadUInt16();
            Identifier = reader.ReadAscii(4);
            BackColor = reader.ReadColor();
            _fadeinOffset = reader.ReadUInt32();
            _fadeoutOffset = reader.ReadUInt32();
        }
        else if (Settings.Android)
        {
            if (Settings.Build >= 290)
            {
                var size = reader.ReadInt32();
                reader.Skip(-4);
                var data = reader.ReadBytes(size + 4);
                reader.Skip(-size + 4);
                currentPosition = 0;

                _movementsOffset = reader.ReadInt16();
                var version = reader.ReadUInt16();
                _extensionOffset = reader.ReadInt16();
                _counterOffset = reader.ReadInt16();
                _valuesOffset = reader.ReadInt16();
                Flags.flag = reader.ReadUInt16();
                var penisFlags = reader.ReadInt16();
                if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;

                for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

                _systemObjectOffset = reader.ReadInt16();
                _animationsOffset = reader.ReadInt16();
                reader.Skip(2);
                _stringsOffset = reader.ReadInt16();
                NewFlags.flag = reader.ReadUInt32();
                Preferences.flag = reader.ReadUInt16();
                Identifier = reader.ReadAscii(4);
                BackColor = reader.ReadColor();
                _fadeinOffset = reader.ReadUInt32();
                _fadeoutOffset = reader.ReadUInt32();
            }
            else
            {
                var size = reader.ReadInt32();
                File.WriteAllBytes($"Test\\{Utils.Utils.ClearName(Parent.name)}.chunk",reader.ReadBytes(size-4));
                reader.Skip(-size+4);
                _movementsOffset = reader.ReadInt16();
                _animationsOffset = reader.ReadInt16();

                var version = reader.ReadInt16();
                _counterOffset = reader.ReadInt16();
                _systemObjectOffset = reader.ReadInt16();
                _valuesOffset = reader.ReadInt16();
                Flags.flag = reader.ReadUInt16();
                var penisFlags = reader.ReadInt16();
                if (penisFlags == 6) Flags["DoNotCreateAtStart"] = true;

                for (var i = 0; i < 8; i++) Qualifiers[i] = reader.ReadInt16();

                _extensionOffset = reader.ReadInt16();
                reader.Skip(2);
                _stringsOffset = reader.ReadInt16();
                NewFlags.flag = reader.ReadUInt32();
                Preferences.flag = reader.ReadUInt16();
                Identifier = reader.ReadAscii(4);
                BackColor = reader.ReadColor();
                _fadeinOffset = reader.ReadUInt32();
                _fadeoutOffset = reader.ReadUInt32();
            }
        }*/

        if (_animationsOffset > 0)
        {
            reader.Seek(currentPosition + _animationsOffset);
            Animations = new Animations();
            Animations.Read(reader);
        }

        if (_movementsOffset > 0)
        {
            if (Settings.Old)
            {
                reader.Seek(currentPosition + _movementsOffset);
                Movements = new Movements();
                var newMovement = new Movement();
                newMovement.Read(reader);
                Movements.Items.Add(newMovement);
            }
            else
            {
                reader.Seek(currentPosition + _movementsOffset);

                Movements = new Movements();
                Movements.Read(reader);
            }
        }

        if (_valuesOffset > 0)
        {
            reader.Seek(currentPosition + _valuesOffset);
            Values = new AlterableValues();
            Values.Read(reader);
        }

        if (_stringsOffset > 0)
        {
            reader.Seek(currentPosition + _stringsOffset);
            Strings = new AlterableStrings();
            Strings.Read(reader);
        }

        if (_systemObjectOffset > 0)
        {
            reader.Seek(currentPosition + _systemObjectOffset);
            switch (Identifier)
            {
                //Text
                case "XTÿÿ":
                case "TE":
                case "TEXT":
                    Text = new Text();
                    Text.Read(reader);
                    break;
                //Counter
                case "TRÿÿ":
                case "CNTR":
                case "SCORE":
                case "LIVE":
                case "CN":
                case "LIVES":
                    Counters = new Counters();
                    Counters.Read(reader);
                    break;
                case "CCA":
                    SubApplication = new SubApplication();
                    SubApplication.Read(reader);
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
                ExtensionData = reader.ReadBytes(dataSize);
            else ExtensionData = new byte[0];
        }

        if (_counterOffset > 0)
        {
            reader.Seek(currentPosition + _counterOffset);
            Counter = new Counter();
            Counter.Read(reader);
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}