using System;

namespace SimpleCLI.Controls;

public class Button:Control
{
    public string Text;
    public Action Activated;

    public Button(string txt,Action act)
    {
        Text = txt;
        Activated = act;
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

    public override void OnActivated()
    {
        Activated.Invoke();
    }
}