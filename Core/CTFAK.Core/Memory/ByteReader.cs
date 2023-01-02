using System;
using System.Drawing;
using System.IO;
using System.Text;
using CTFAK.Utils;

namespace CTFAK.Memory;

public class ByteReader : BinaryReader
{
    public ByteReader(Stream input) : base(input)
    {
    }

    public ByteReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public ByteReader(byte[] data) : base(new MemoryStream(data))
    {
    }

    public ByteReader(string path, FileMode fileMode) : base(new FileStream(path, fileMode))
    {
    }

    public void Seek(long offset, SeekOrigin seekOrigin = SeekOrigin.Begin)
    {
        BaseStream.Seek(offset, seekOrigin);
    }

    public void Skip(long count)
    {
        BaseStream.Seek(count, SeekOrigin.Current);
    }

    public long Tell()
    {
        return BaseStream.Position;
    }

    public long Size()
    {
        return BaseStream.Length;
    }

    public bool HasMemory(int size)
    {
        return Size() - Tell() >= size;
    }

    public ushort PeekUInt16()
    {
        var value = ReadUInt16();
        Seek(-2, SeekOrigin.Current);
        return value;
    }

    public byte PeekByte()
    {
        var value = ReadByte();
        Seek(-1, SeekOrigin.Current);
        return value;
    }

    public short PeekInt16()
    {
        var value = ReadInt16();
        Seek(-2, SeekOrigin.Current);
        return value;
    }

    public int PeekInt32()
    {
        var value = ReadInt32();
        Seek(-4, SeekOrigin.Current);
        return value;
    }

    public string ReadAscii(int length = -1)
    {
        var str = "";
        if (length >= 0)
        {
            for (var i = 0; i < length; i++) str += Convert.ToChar(ReadByte());
        }
        else
        {
            var b = ReadByte();
            while (b != 0)
            {
                str += Convert.ToChar(b);
                b = ReadByte();
            }
        }

        return str;
    }

    public string ReadWideString(int length = -1)
    {
        var str = "";
        if (length >= 0)
        {
            for (var i = 0; i < length; i++) str += Convert.ToChar(ReadUInt16());
        }
        else
        {
            var b = ReadUInt16();
            while (b != 0)
            {
                str += Convert.ToChar(b);
                b = ReadUInt16();
            }
        }

        return str;
    }

    public string ReadUniversal(int len = -1)
    {
        if (Settings.Unicode)
            return ReadWideString(len);
        return ReadAscii(len);
    }

    public Color ReadColor()
    {
        var r = ReadByte();
        var g = ReadByte();
        var b = ReadByte();
        var a = ReadByte();

        return Color.FromArgb(a, r, g, b);
    }

    public override byte[] ReadBytes(int count = -1)
    {
        if (count == -1) return base.ReadBytes((int)Size());
        return base.ReadBytes(count);
    }
}