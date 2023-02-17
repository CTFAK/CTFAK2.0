namespace SimpleCLI.Controls;

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