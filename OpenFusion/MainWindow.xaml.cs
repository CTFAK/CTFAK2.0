using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenFusion.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace OpenFusion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 5
            };
            OpenTkControl.Start(settings);
            while(!OpenTkControl.IsInitialized){}
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            foreach (var obj in objects)
            {
                obj.Init();
            }
            
        }

        private void OpenTkControl_OnRender(TimeSpan obj)
        {
            Title = ((1/obj.TotalMilliseconds)*1000).ToString();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0, 0, 0.3f, 1.0f);
            
            foreach (var drawable in objects)
            {
                drawable.OnRender(this);
            }
            
            
        }
        List<Drawable> objects = new List<Drawable>()
        {
            new Drawable(900,100,200,200)
        };

        private void OpenTkControl_OnInitialized(object sender, EventArgs e)
        {

            
        }

        private void OpenTkControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine($"Size changed: {e.NewSize}");
            //GL.Viewport(0, 0, 200,200);

        }
    }
}