using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    class GuaranteedHint : TransparentPictureBox
    {
        readonly List<string> ListImageName = new List<string>();

        Size GuaranteddHintSize;

        public GuaranteedHint(ObjectPoint data)
        {
            if(data.ImageCollection != null)
                ListImageName = data.ImageCollection.ToList();

            GuaranteddHintSize = data.Size;

            this.BackColor = Color.Transparent;
            if (ListImageName.Count > 0)
            {
                this.Name = ListImageName[0];
                this.Image = ImageCache.Get(ListImageName[0]);
                this.SizeMode = PictureBoxSizeMode.StretchImage;
                this.Size = GuaranteddHintSize;
            }            
            this.Location = new Point(data.X, data.Y);
            this.TabStop = false;
            this.AllowDrop = false;
        }
    }
}
