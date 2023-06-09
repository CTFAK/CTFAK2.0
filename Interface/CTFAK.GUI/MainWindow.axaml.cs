

using System.ComponentModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CTFAK.FileReaders;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.CCN.Chunks;
using CTFAK.MMFParser.CCN.Chunks.Frame;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.Utils;

namespace CTFAK.GUI;

public partial class MainWindow : Window
{
    public static MainWindow Instance;
    public IFileReader CurrentReader;
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
        StatusText = this.FindControl<TextBlock>("StatusText");
        StatusProgress = this.FindControl<ProgressBar>("StatusProgress");
        ChunkDetails = this.FindControl<ListBox>("ChunkDetails");
        ChunkTree = this.FindControl<TreeView>("ChunkTree");
        GameInfoText = this.FindControl<TextBlock>("GameInfoText");
    }


    private void SelectFile_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("a");
        var fileSelector = new FileSelectorWindow();
        fileSelector.ShowDialog(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        CTFAKCore.Init();
    }

    public void StartLoadingGame(IFileReader reader, string path)
    {
        StatusText.Text = "Loading...";
        var backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += (o,e) =>
        {
            CurrentReader = reader;
            CurrentReader.LoadGame(path);
        };
        backgroundWorker.RunWorkerCompleted += (o, e) =>
        {
            var game = CurrentReader.GetGameData();
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"Name: {game.Name}");
            strBuilder.AppendLine($"Author: {game.Author}");
            strBuilder.AppendLine($"Copyright: {game.Copyright}");
            strBuilder.AppendLine($"Number of frames: {game.Frames.Count}");
            strBuilder.AppendLine($"Number of objects: {game.FrameItems.Count}");
            strBuilder.AppendLine($"Number of images: {game.Images.Items.Count}");
            strBuilder.AppendLine($"Number of sounds: {game.Sounds.Items.Count}");
            GameInfoText.Text = strBuilder.ToString();
            ChunkDetails.Items.Clear();
            ChunkTree.Items.Clear();
            var chunks = game.Chunks;
            foreach (var chk in chunks.Items)
            {
                var treeViewItem = new TreeViewItem();
                string chunkName;
                if (!ChunkList.ChunkNames.TryGetValue(chk.Id, out chunkName))
                    chunkName = $"Unknown-{chk.Id}";
                
                treeViewItem.Header = chunkName;
                if (chk.Loader is Frame frmLoader)
                {
                    treeViewItem.Header = $"Frame \"{frmLoader.Name}\"";
                }
                treeViewItem.Tag = chk;
                treeViewItem.Tapped += (o, e) =>
                {
                    DisplayChunk(chk);
                };
                ChunkTree.Items.Add(treeViewItem);
            }
        };
        backgroundWorker.RunWorkerAsync();
    }

    public void DisplayChunk(Chunk chk)
    {
        ChunkDetails.Items.Clear();
        string chunkName;
        if (!ChunkList.ChunkNames.TryGetValue(chk.Id, out chunkName))
            chunkName = $"Unknown-{chk.Id}";
        ChunkDetails.Items.Add(new TextBlock() { Text = $"Name: {chunkName}" });
        ChunkDetails.Items.Add(new TextBlock() { Text = $"Loader: {chk?.Loader?.GetType().Name ?? "None"}" });
        ChunkDetails.Items.Add(new TextBlock() { Text = $"Flag: {chk.Flag}" });
        ChunkDetails.Items.Add(new TextBlock() { Text = $"File offset: 0x{chk.FileOffset.ToString("X4")}" });
        ChunkDetails.Items.Add(new TextBlock() { Text = $"File size: {chk.FileSize.ToPrettySize()}" });
        ChunkDetails.Items.Add(new TextBlock() { Text = $"Unpacked size: {chk.UnpackedSize.ToPrettySize()}" });
        ChunkDetails.Items.Add(new TextBlock());

        var loader = chk.Loader;
        if(loader is null) return;

        if (loader is StringChunk strChk)
        {
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Contents: {strChk.Value}", TextWrapping = TextWrapping.WrapWithOverflow});
        }
        else if (loader is AppHeader hdrChk)
        {
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Screen Resolution: {hdrChk.WindowWidth}x{hdrChk.WindowHeight}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Initial Lives: {hdrChk.InitialLives}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Initial Score: {hdrChk.InitialScore}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Flags: {hdrChk.Flags}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"New flags: {hdrChk.NewFlags}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Other flags: {hdrChk.OtherFlags}", TextWrapping = TextWrapping.WrapWithOverflow});
        }
        else if (loader is Frame frmChk)
        {
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Frame size: {frmChk.Width}x{frmChk.Height}", TextWrapping = TextWrapping.WrapWithOverflow});
            ChunkDetails.Items.Add(new TextBlock() { Text = $"Flags: {frmChk.Flags}", TextWrapping = TextWrapping.WrapWithOverflow});

        }
        
    }
}
