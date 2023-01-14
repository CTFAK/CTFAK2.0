using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.FileReaders;

public interface IFileReader
{
    string Name { get; }

    GameData GetGameData();
    int ReadHeader(ByteReader reader);
    void LoadGame(string gamePath);
    Dictionary<int, Bitmap> GetIcons();
    void PatchMethods();
}