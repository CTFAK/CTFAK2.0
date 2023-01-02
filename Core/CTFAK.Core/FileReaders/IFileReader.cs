using System.Collections.Generic;
using System.Drawing;
using CTFAK.CCN;
using CTFAK.Memory;

namespace CTFAK.FileReaders;

public interface IFileReader
{
    string Name { get; }

    GameData getGameData();
    int ReadHeader(ByteReader reader);
    void LoadGame(string gamePath);
    Dictionary<int, Bitmap> getIcons();
    void PatchMethods();
}