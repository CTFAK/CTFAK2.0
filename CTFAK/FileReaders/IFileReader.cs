using CTFAK.CCN;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.FileReaders
{
    public interface IFileReader
    {

        GameData getGameData();
        int ReadHeader(ByteReader reader);
        void LoadGame(ByteReader reader);
    }
}
