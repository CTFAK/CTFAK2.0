using System.Collections.Generic;

namespace JFusion.ObjectProperties
{
    public class JMFAAnimationDirection
    {
        public int minSpeed;
        public int maxSpeed;
        public int repeat;
        public int backTo;
        public List<int> frames = new List<int>();
    }
    public class JMFAAnimation
    {
        public string name;
        public List<JMFAAnimationDirection> directions = new List<JMFAAnimationDirection>();
    }
}