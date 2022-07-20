using System.Drawing;
using Newtonsoft.Json;

namespace JFusion
{
    public class JMfAImage
    {
        public int Handle;
        public short HotspotX;
        public short HotspotY;
        public short ActionX;
        public short ActionY;
        public uint Flags;
        [JsonIgnore]
        public Bitmap bmp;
    }
}