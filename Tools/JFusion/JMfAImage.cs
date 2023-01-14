using System.Drawing;
using Newtonsoft.Json;

namespace JFusion;

public class JMfAImage
{
    public short ActionX;
    public short ActionY;

    [JsonIgnore] public Bitmap bmp;

    public uint Flags;
    public int Handle;
    public short HotspotX;
    public short HotspotY;
}