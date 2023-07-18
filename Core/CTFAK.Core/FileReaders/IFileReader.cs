using CTFAK.CCN;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.FileReaders
{
    public interface IFileReader
    {
        string Name { get; }

        GameData getGameData();
        void LoadGame(string gamePath);
        Dictionary<int, Bitmap> getIcons();
        void PatchMethods();
        IFileReader Copy();

    }
}