using System;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(8759, "SecNum")]
public class SecNum : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        // I removed the implementation for that because Clickteam asked me to
        
        // 09.06.23
        // I'm adding the implementation the implementation back. Clickteam didn't respect my wish, so I'm not respecting theirs either
        // This chunk contains a unique identifier (if the game was built with webshop version) of the user who built the game.
        // If you have a fusion keygen - this can be used to generate a key with the same license number and potentially do some nasty stuff and get the user banned
        int eax = reader.ReadInt32();
        int ecx = reader.ReadInt32();
        int tickCount = eax ^ 0xBD75329;
        int serialSlice = ecx + eax;
        serialSlice ^= 0xF75A3F;
        serialSlice ^= eax;
        serialSlice -= 10;
        Logger.Log($"Creator's license id: "+serialSlice);

    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}