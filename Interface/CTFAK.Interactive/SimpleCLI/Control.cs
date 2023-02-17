using System.Collections.Generic;

namespace SimpleCLI;

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

    public virtual void OnActivated()
    {
        
    }
}