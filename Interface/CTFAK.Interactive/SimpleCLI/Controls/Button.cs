using System;

namespace SimpleCLI.Controls;

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