

namespace SimpleCLI
{
    public class CliWindow
    {
        public static bool horizontalLayout;
        public List<CliControl> Controls = new List<CliControl>();
        public CliControl SelectedControl;

        public bool ShouldStop;

        public static CliWindow currentWindow;

        public static void Show(CliWindow window)
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
    public class CliControl
    {
        public List<CliControl> children = new List<CliControl>(); 
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
    public class CliButton:CliControl
    {
        public string Text;
        public Action<ConsoleKey> Activated;
        public bool activateWithAny;

        public CliButton(string txt,Action act)
        {
            Text = txt;
            Activated = (key)=>{act.Invoke();};
            activateWithAny = false;
        }
        public CliButton(string txt,Action<ConsoleKey> act)
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
            if(CliWindow.horizontalLayout)
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
    public class CliCheckbox:CliControl
    {
        public string Text;
        public bool Activated;

        public CliCheckbox(string txt)
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
            if(CliWindow.horizontalLayout)
                x += Text.Length;
            else y += 1;
        }

        public override void OnActivated(ConsoleKey key)
        {
            Activated = !Activated;
        }
    }
    public class HorizontalLayout:CliControl
    {
        public override bool HandlesSelection => true;
    
        public override void Draw(ref int x, ref int y)
        {
            CliWindow.horizontalLayout = true;
        
        
        }
    }
    public class VerticalLayout:CliControl
    {
        public override bool HandlesSelection => true;

        public override void Draw(ref int x, ref int y)
        {
            CliWindow.horizontalLayout = false;
        }
    }
    public class CliSeparator:CliControl
    {
        public int Dist;

        public CliSeparator(int dist)
        {
            Dist = dist;
        }
        public override void Draw(ref int x, ref int y)
        {
            if (CliWindow.horizontalLayout)
            {
                x += Dist;
            }
            else y += Dist;
        }
    }
    public class CliLabel:CliControl
    {
        public string Text;

        public ConsoleColor Color;

        public CliLabel(string txt,ConsoleColor color = ConsoleColor.White)
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
            if(CliWindow.horizontalLayout)
                x += Text.Length+1;
            else y += Text.Split('\n').Length;
        }
    }
}
