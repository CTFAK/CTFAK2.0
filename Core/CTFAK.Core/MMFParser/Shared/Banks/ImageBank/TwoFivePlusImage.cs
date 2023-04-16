using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CTFAK.Memory;
using CTFAK.MMFParser.Translation;
using CTFAK.Utils;
using K4os.Compression.LZ4;

namespace CTFAK.Shared.Banks.ImageBank;


// 2.5+ DX11 Images use GraphicMode 8, which is RGBA (no mask), and, surprisingly, is supported by non-2.5+ versions of Fusion
public class 
    TwoFivePlusImage:FusionImage
{
    public override void Read(ByteReader reader)
    {
        
        Handle = reader.ReadInt32();
        if (Settings.Build >= 284)
            Handle--;
        Checksum = reader.ReadInt32();
        references = reader.ReadInt32();
        reader.Skip(4);
        var dataSize = reader.ReadInt32();
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
        GraphicMode = reader.ReadByte();
        Flags.flag = reader.ReadByte();
        reader.Skip(2);
        HotspotX = reader.ReadInt16();
        HotspotY = reader.ReadInt16();
        ActionX = reader.ReadInt16();
        ActionY = reader.ReadInt16();
        Transparent = reader.ReadInt32();
        var decompSizePlus = reader.ReadInt32();
        var rawImg = reader.ReadBytes(dataSize - 4);
        var target = new byte[decompSizePlus];
        LZ4Codec.Decode(rawImg, target);
        imageData = target;
        
        

    }
}