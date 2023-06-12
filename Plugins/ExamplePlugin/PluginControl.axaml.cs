using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CTFAK.GUI.PluginSystem;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MFA;
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
    public string Name => "MFA Test";
    
    
    public void DrawCLI(GameData game)
    {
        
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var mfaReader = new ByteReader("test.mfa",FileMode.Open);
        var mfaWriter = new ByteWriter("output.mfa",FileMode.Create);
        var mfa = new MFAData();
        mfa.Read(mfaReader);
        mfa.Write(mfaWriter);
        mfaReader.Close();
        mfaWriter.Close();
    }
}