using System;

namespace SimpleCLI.Controls;

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

    public override void OnActivated()
    {
        Activated = !Activated;
    }
}