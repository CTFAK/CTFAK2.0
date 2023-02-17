namespace SimpleCLI.Controls;

public class VerticalLayout:Control
{
    public override bool HandlesSelection => true;

    public override void Draw(ref int x, ref int y)
    {
        Window.horizontalLayout = false;
    }
}