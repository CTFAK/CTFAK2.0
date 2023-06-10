using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CTFAK.GUI.PluginSystem;
using CTFAK.MMFParser.CCN;
using static CTFAK.GUI.PluginSystem.PluginUtils;

namespace ExamplePlugin;

public partial class PluginControl : UserControl,IPlugin
{
    public PluginControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public bool RequiresGame => true;
    public string Name => "Example Plugin";
    
    
    public void DrawCLI(GameData game)
    {
        
    }
}