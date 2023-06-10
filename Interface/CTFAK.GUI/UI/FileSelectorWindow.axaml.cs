using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CTFAK.FileReaders;

namespace CTFAK.GUI;

public partial class FileSelectorWindow : Window
{
    public List<IFileReader> fileReaders = new List<IFileReader>();
    public FileSelectorWindow()
    {
        InitializeComponent();
        FileReaders = this.FindControl<ListBox>("FileReaders");
        FilePath = this.FindControl<TextBox>("FilePath");
        LoadFileButton = this.FindControl<Button>("LoadFileButton");
        if (!Design.IsDesignMode)
        {
            FilePath.TextChanged += (a, b) =>
            {
                LoadFileButton.IsEnabled = File.Exists(FilePath.Text);
            };
            var types = typeof(IFileReader).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.GetInterface(typeof(IFileReader).FullName) != null)
                {
                    fileReaders.Add(Activator.CreateInstance(type) as IFileReader); 
                }
            }

            fileReaders=fileReaders.OrderBy(a => a.Priority).ToList();
        
            foreach (var reader in fileReaders)
            {
            
                var listItem = new TextBlock();
                listItem.Text = reader.Name;
                listItem.Tag = reader;
                FileReaders.Items.Add(listItem);
            }

            FileReaders.SelectedIndex = 0;
        }
        

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    

    private async void SelectPath_OnClick(object? sender, RoutedEventArgs e)
    {
        var filters = new List<FilePickerFileType>();
        filters.Add(new FilePickerFileType("Fusion Game"){Patterns = new string[]{"*.ccn","*.exe","*.dat"}});
        var task = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions(){AllowMultiple = false, Title = "Select Fusion game", FileTypeFilter = filters});

        FilePath.Text = task.Count>0 ? task[0].Path.LocalPath: string.Empty;
    }

    private void LoadFile_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
        var reader = (FileReaders.SelectedItem as TextBlock).Tag as IFileReader;
        MainWindow.Instance.StartLoadingGame(reader,FilePath.Text);
    }
}