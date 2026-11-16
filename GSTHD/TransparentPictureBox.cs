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
            GoMode.PaintGlowBehind(this, e);
        }
    }
}