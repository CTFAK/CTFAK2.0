using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.FileReaders;

public interface IFileReader
{
    int Priority { get; }
    string Name { get; }

    GameData GetGameData();
    bool LoadGame(string gamePath);
    Dictionary<int, Bitmap> GetIcons();
    void Close();
    ByteReader GetFileReader();
}