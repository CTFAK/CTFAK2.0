using CTFAK.FileReaders;
using CTFAK.GUI.CLI;
using CTFAK.MMFParser.CCN;

namespace CTFAK.GUI.PluginSystem;

public static class PluginUtils
{
    public static IFileReader GetReader()
    {
        if (Program.IsCli)
            return MainInterface.CurrentReader;
        else return MainWindow.Instance.CurrentReader;
    }

    public static GameData GetGameData()
    {
        return GetReader().GetGameData();
    }
}