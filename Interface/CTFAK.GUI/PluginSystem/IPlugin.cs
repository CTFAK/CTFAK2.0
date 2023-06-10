using CTFAK.MMFParser.CCN;

namespace CTFAK.GUI.PluginSystem;

public interface IPlugin
{
    string Name { get; }
    bool RequiresGame { get; }
    void DrawCLI(GameData game);
}