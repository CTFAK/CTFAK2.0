using System.Collections.Generic;

namespace JFusion.ObjectProperties;

public class JMFAAnimationDirection
{
    public int backTo;
    public List<int> frames = new();
    public int maxSpeed;
    public int minSpeed;
    public int repeat;
}

public class JMFAAnimation
{
    public List<JMFAAnimationDirection> directions = new();
    public string name;
}