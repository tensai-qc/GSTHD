using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GSTHD
{
    class GoMode : PictureBox
    {
        public static GoMode Current;
        Bitmap[] glowFrames;
        int frameIndex;
        Rectangle glowDirty;

        public Bitmap CurrentFrame => glowFrames?[frameIndex];
        Rectangle DirtyRectInParent =>
            new Rectangle(Left + glowDirty.X, Top + glowDirty.Y, glowDirty.Width, glowDirty.Height);

        readonly List<string> ListImageName;
        private new readonly string BackgroundImage;        
        Point FirstLocation;
        readonly Timer tictac;
        int imageIndex = 0;

        public PictureBox GoModeImage = new PictureBox();
        Size GoModeImageSize;

        private void BuildFrames(string file)
        {
            using (var src = Image.FromFile(@"Resources/" + file))
            {
                var rect = new Rectangle(0, 0, src.Width, src.Height);
                glowFrames = new Bitmap[4];
                glowFrames[0] = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(glowFrames[0]))
                    g.DrawImage(src, rect);

                for (int i = 1; i < 4; i++)
                {
                    glowFrames[i] = glowFrames[i - 1].Clone(rect, PixelFormat.Format32bppPArgb);
                    glowFrames[i].RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
            }

            glowDirty = Rectangle.Empty;
            foreach (var f in glowFrames)
            {
                var b = OpaqueBounds(f);
                glowDirty = glowDirty.IsEmpty ? b : Rectangle.Union(glowDirty, b);
            }
        }

        static Rectangle OpaqueBounds(Bitmap bmp, byte minAlpha = 40)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                    ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            var buf = new byte[data.Stride * bmp.Height];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            bmp.UnlockBits(data);

            int minX = bmp.Width, minY = bmp.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    if (buf[y * data.Stride + x * 4 + 3] >= minAlpha)
                    {
                        if (x < minX) minX = x; if (x > maxX) maxX = x;
                        if (y < minY) minY = y; if (y > maxY) maxY = y;
                    }
            return maxX < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        public GoMode(ObjectPointGoMode data)
        {
            Current = this;
            tictac = new Timer
            {
                Interval = 95
            };
            tictac.Tick += Tictac_Tick;

            if (data.ImageCollection != null)
            {
                this.ListImageName = data.ImageCollection.ToList();
            }

            GoModeImageSize = data.Size;

            this.FirstLocation = new Point(data.X, data.Y);

            if(ListImageName.Count > 0)
            {
                GoModeImage.Name = ListImageName[0];
                GoModeImage.Image = Image.FromFile(@"Resources/" + ListImageName[0]);
                GoModeImage.SizeMode = PictureBoxSizeMode.StretchImage;
                GoModeImage.Size = this.GoModeImageSize;
            }                        
            GoModeImage.BackColor = Color.Transparent;
            // GoModeImage.MouseUp += Click_MouseUp;
            GoModeImage.MouseDown += Click_MouseDown;

            if (data.BackgroundImage != string.Empty)
            {
                BackgroundImage = data.BackgroundImage;
                this.Name = data.BackgroundImage;
                BuildFrames(data.BackgroundImage);
            }
            this.BackColor = Color.Transparent;
            this.Controls.Add(GoModeImage);
        }

        private void Tictac_Tick(object sender, EventArgs e)
        {
            if (glowFrames == null || Image == null || Parent == null) return;
            if (FindForm()?.WindowState == FormWindowState.Minimized) return;

            frameIndex = (frameIndex + 1) & 3;
            Parent.Invalidate(DirtyRectInParent, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var frame = CurrentFrame;
            if (Image == null || frame == null) return;
            e.Graphics.DrawImage(frame, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        private void Click_MouseDown(object sender, MouseEventArgs e)
        {
            var oldBounds = this.Bounds;

            if (e.Button == MouseButtons.Left && imageIndex < ListImageName.Count - 1)
            {
                imageIndex += 1;
            }

            if (e.Button == MouseButtons.Right && imageIndex > 0)
            {
                imageIndex -= 1;
            }

            tictac.Enabled = imageIndex > 0;

            var pbox = (PictureBox)sender;
            pbox.Image = Image.FromFile(@"Resources/" + ListImageName[imageIndex]);
            pbox.Name = ListImageName[imageIndex];
            pbox.Size = GoModeImageSize;

            if (imageIndex <= 0)
            {
                this.Name = "NoImage";
                this.Image = null;
            }
            else
            {
                this.Name = BackgroundImage;
                this.Image = glowFrames[0];
                this.Size = glowFrames[0].Size;
            }

            SetLocation();

            Parent?.Invalidate(Rectangle.Union(oldBounds, this.Bounds), true);
        }

        public void SetLocation()
        {
            GoModeImage.Location = new Point(this.Width / 2 - GoModeImage.Width / 2, this.Height / 2 - GoModeImage.Height / 2);
            this.Location = new Point(this.FirstLocation.X - GoModeImage.Location.X, this.FirstLocation.Y - GoModeImage.Location.Y);
        }
    }
}
