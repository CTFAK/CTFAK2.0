

namespace SimpleCLI
{
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
                    control.Draw(ref currentXPos, ref currentYPos);

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
                            if (currentIndex == 0)
                                break;
                            var range = Controls.GetRange(0, currentIndex);
                            range.Reverse();
                            if (!range.Any(a => a.IsSelectable))
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
                            var range = Controls.GetRange(currentIndex + 1, Controls.Count - (currentIndex + 1));
                            if (!range.Any(a => a.IsSelectable))
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
                            if (!range.Any(a => a.IsSelectable))
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
                            var range = Controls.GetRange(currentIndex + 1, Controls.Count - (currentIndex + 1));
                            if (!range.Any(a => a.IsSelectable))
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
    public class Control
    {
        public List<Control> children = new List<Control>(); 
        public virtual bool IsSelectable => false;
        public virtual bool HandlesSelection => false;
        public virtual void Draw(ref int x,ref int y)
        {
        }

        public bool IsSelected;
        public virtual void OnSelected()
        {
        
        }

        public virtual void OnActivated(ConsoleKey key)
        {
        
        }
    }
}

namespace SimpleCLI.Controls
{
    public class Button:Control
    {
        public string Text;
        public Action<ConsoleKey> Activated;
        public bool activateWithAny;

        public Button(string txt,Action act)
        {
            Text = txt;
            Activated = (key)=>{act.Invoke();};
            activateWithAny = false;
        }
        public Button(string txt,Action<ConsoleKey> act)
        {
            Text = txt;
            Activated = act;
            activateWithAny = true;
        }
    
        public override bool IsSelectable => true;

        public override void Draw(ref int x, ref int y)
        {
            Console.SetCursorPosition(x,y);
            Console.BackgroundColor = ConsoleColor.Black;
            if (IsSelected)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write($"[{Text}]");
            Console.BackgroundColor = ConsoleColor.White;
            if(Window.horizontalLayout)
                x += Text.Length+3;
            else y += 1;
        }

        public override void OnActivated(ConsoleKey key)
        {
            if (activateWithAny)
                Activated.Invoke(key);
            else if (key == ConsoleKey.Enter) 
                Activated.Invoke(key);
        }
    }
    public class Checkbox:Control
    {
        public string Text;
        public bool Activated;

        public Checkbox(string txt)
        {
            Text = txt;

        }

        public override bool IsSelectable => true;

        public override void Draw(ref int x, ref int y)
        {
            Console.SetCursorPosition(x,y);
            if (IsSelected)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write($"{(Activated ? "√":" ")} {Text}");
            if(Window.horizontalLayout)
                x += Text.Length;
            else y += 1;
        }

        public override void OnActivated(ConsoleKey key)
        {
            Activated = !Activated;
        }
    }
    public class HorizontalLayout:Control
    {
        public override bool HandlesSelection => true;
    
        public override void Draw(ref int x, ref int y)
        {
            Window.horizontalLayout = true;
        
        
        }
    }
    public class VerticalLayout:Control
    {
        public override bool HandlesSelection => true;

        public override void Draw(ref int x, ref int y)
        {
            Window.horizontalLayout = false;
        }
    }
    public class Separator:Control
    {
        public int Dist;

        public Separator(int dist)
        {
            Dist = dist;
        }
        public override void Draw(ref int x, ref int y)
        {
            if (Window.horizontalLayout)
            {
                x += Dist;
            }
            else y += Dist;
        }
    }
    public class Label:Control
    {
        public string Text;

        public ConsoleColor Color;

        public Label(string txt,ConsoleColor color = ConsoleColor.White)
        {
            Color = color;
            Text = txt;
        }
        public override void Draw(ref int x, ref int y)
        {
            Console.SetCursorPosition(x,y);
            Console.ForegroundColor = Color;
            Console.Write(Text);
            Console.ForegroundColor = ConsoleColor.White;
            if(Window.horizontalLayout)
                x += Text.Length+1;
            else y += Text.Split('\n').Length;
        }
    }
}
