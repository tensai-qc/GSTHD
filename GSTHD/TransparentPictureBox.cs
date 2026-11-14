using System.Drawing;
using System.Windows.Forms;

namespace GSTHD
{
    public class TransparentPictureBox : PictureBox
    {
        public TransparentPictureBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var gm = GoMode.Current;
            var frame = gm?.CurrentFrame;
            if (frame == null || gm.Image == null || !gm.Visible || gm.Parent != Parent) return;

            var dest = new Rectangle(gm.Left - Left, gm.Top - Top, gm.Width, gm.Height);
            var area = Rectangle.Intersect(dest, ClientRectangle);
            if (area.IsEmpty) return;

            var src = new Rectangle(area.X - dest.X, area.Y - dest.Y, area.Width, area.Height);
            e.Graphics.DrawImage(frame, area, src, GraphicsUnit.Pixel);
        }
    }
}