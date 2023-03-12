using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCLI;

public class Window
{
    public static bool horizontalLayout;
    public List<Control> Controls = new List<Control>();
    public Control SelectedControl;
    
    public bool ShouldStop;

    public static Window currentWindow;
    public static void Show(Window window)
    {
        if (currentWindow != null)
        {
            currentWindow.Stop();
            currentWindow = null;
        }

        currentWindow = window;
        currentWindow.ShouldStop = false;

        currentWindow.Start();
    }
    public void Stop()
    {
        ShouldStop = true;
    }
    public void Start()
    {
        Console.Clear();
        SelectedControl = Controls.FirstOrDefault(a => a.IsSelectable);
        if (SelectedControl != null)
        {
            SelectedControl.IsSelected = true;
            SelectedControl.OnSelected();
        }
            
        
        
        while (true)
        {
            if (ShouldStop) break;
            horizontalLayout = false;

            int currentXPos = 0;
            int currentYPos = 0;
            foreach (var control in Controls)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                control.Draw(ref currentXPos,ref currentYPos);
                
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            var keyResult = Console.ReadKey();
            switch (keyResult.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (horizontalLayout)
                    {
                        
                        var currentIndex = Controls.IndexOf(SelectedControl);
                        if(currentIndex==0)
                            break;
                        var range = Controls.GetRange(0, currentIndex);
                        range.Reverse();
                        if (!range.Any(a=>a.IsSelectable))
                            break;
                        var newSelected = range.First(a => a.IsSelectable);
                        SelectedControl.IsSelected = false;
                        SelectedControl = newSelected;
                        SelectedControl.IsSelected = true;
                        SelectedControl.OnSelected();
                    }
                    break;
                
                case ConsoleKey.RightArrow:
                    if (horizontalLayout)
                    {
                        
                        var currentIndex = Controls.IndexOf(SelectedControl);
                        var range = Controls.GetRange(currentIndex+1, Controls.Count - (currentIndex+1));
                        if (!range.Any(a=>a.IsSelectable))
                            break;
                        var newSelected = range.First(a => a.IsSelectable);
                        SelectedControl.IsSelected = false;
                        SelectedControl = newSelected;
                        SelectedControl.IsSelected = true;
                        SelectedControl.OnSelected();
                    }
                    break;
                
                case ConsoleKey.UpArrow:
                    if (!horizontalLayout)
                    {
                        
                        var currentIndex = Controls.IndexOf(SelectedControl);
                        var range = Controls.GetRange(0, currentIndex);
                        range.Reverse();
                        if (!range.Any(a=>a.IsSelectable))
                            break;
                        var newSelected = range.First(a => a.IsSelectable);
                        SelectedControl.IsSelected = false;
                        SelectedControl = newSelected;
                        SelectedControl.IsSelected = true;
                        SelectedControl.OnSelected();
                    }
                    break;
                
                case ConsoleKey.DownArrow:
                    if (!horizontalLayout)
                    {
                        
                        var currentIndex = Controls.IndexOf(SelectedControl);
                        var range = Controls.GetRange(currentIndex+1, Controls.Count - (currentIndex+1));
                        if (!range.Any(a=>a.IsSelectable))
                            break;
                        var newSelected = range.First(a => a.IsSelectable);
                        SelectedControl.IsSelected = false;
                        SelectedControl = newSelected;
                        SelectedControl.IsSelected = true;
                        SelectedControl.OnSelected();
                    }
                    break;
                default:
                    if (SelectedControl == null)
                        break;
                    SelectedControl.OnActivated(keyResult.Key);
                    break;
                    
            }
            
            Console.Clear();
        }
    }
}