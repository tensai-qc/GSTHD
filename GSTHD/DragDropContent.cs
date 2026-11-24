using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSTHD
{
    public struct DragDropContent
    {
        public bool IsAutocheck;
        public string ImageName;
        public string[] ImageCollection { get; set; }
        public int ImageIndex { get; set; }

        public DragDropContent(bool isAutocheck, string imageName)
        {
            IsAutocheck = isAutocheck;
            ImageName = imageName;
            ImageCollection = null;
            ImageIndex = 0;
        }
    }
}
