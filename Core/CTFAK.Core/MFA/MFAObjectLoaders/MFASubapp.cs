using CTFAK.Memory;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFASubApp : ObjectLoader
    {
        public uint source;
        public uint FrameNum;
        public int GVS;
        public int lives;
        public int Scores;
        public int PControls;
        public int MDICW;
        public int RNA;
        public int Dock;
        public int CustomSize;
        public int StretchToOBJ;
        public int DisplaySprite;
        public int popup;
        public int ClipSibs;
        public int Border;
        public int Dialog;
        public int Resize;
        public int Caption;
        public int TCaption;
        public int SysMenu;
        public int DisClose;
        public int HidClose;
        public int Modal;

        public MFASubApp(ByteReader reader)
          : base(reader)
        {
        }

        public override void Read()
        {
            base.Read();
            this.source = this.reader.ReadUInt32();
            this.FrameNum = this.reader.ReadUInt32();
            this.GVS = this.reader.ReadInt32();
            this.lives = this.reader.ReadInt32();
            this.Scores = this.reader.ReadInt32();
            this.PControls = this.reader.ReadInt32();
            this.MDICW = this.reader.ReadInt32();
            this.RNA = this.reader.ReadInt32();
            this.Dock = this.reader.ReadInt32();
            this.CustomSize = this.reader.ReadInt32();
            this.StretchToOBJ = this.reader.ReadInt32();
            this.DisplaySprite = this.reader.ReadInt32();
            this.popup = this.reader.ReadInt32();
            this.ClipSibs = this.reader.ReadInt32();
            this.Border = this.reader.ReadInt32();
            this.Dialog = this.reader.ReadInt32();
            this.Resize = this.reader.ReadInt32();
            this.Caption = this.reader.ReadInt32();
            this.TCaption = this.reader.ReadInt32();
            this.SysMenu = this.reader.ReadInt32();
            this.DisClose = this.reader.ReadInt32();
            this.HidClose = this.reader.ReadInt32();
            this.Modal = this.reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteUInt32(this.source);
            Writer.WriteUInt32(this.FrameNum);
            Writer.WriteInt32(this.GVS);
            Writer.WriteInt32(this.lives);
            Writer.WriteInt32(this.Scores);
            Writer.WriteInt32(this.PControls);
            Writer.WriteInt32(this.MDICW);
            Writer.WriteInt32(this.RNA);
            Writer.WriteInt32(this.Dock);
            Writer.WriteInt32(this.CustomSize);
            Writer.WriteInt32(this.StretchToOBJ);
            Writer.WriteInt32(this.DisplaySprite);
            Writer.WriteInt32(this.popup);
            Writer.WriteInt32(this.ClipSibs);
            Writer.WriteInt32(this.Border);
            Writer.WriteInt32(this.Dialog);
            Writer.WriteInt32(this.Resize);
            Writer.WriteInt32(this.Caption);
            Writer.WriteInt32(this.TCaption);
            Writer.WriteInt32(this.SysMenu);
            Writer.WriteInt32(this.DisClose);
            Writer.WriteInt32(this.HidClose);
            Writer.WriteInt32(this.Modal);
        }
    }
}
