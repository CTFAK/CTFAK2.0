using System;
using Microsoft.VisualBasic.CompilerServices;

namespace SimpleCLI.Controls;

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