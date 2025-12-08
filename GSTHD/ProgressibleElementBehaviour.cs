using System;
using System.Windows.Forms;

namespace GSTHD
{
    public interface IProgressibleElement<T>
    {
        void IncrementState();
        void DecrementState();
        void ResetState();
        void HandleMouseWheel(object sender, MouseEventArgs e);
    }

    public class ProgressibleElementBehaviour<T>
    {
        protected IProgressibleElement<T> Element;
        protected Settings Settings;

        public ProgressibleElementBehaviour(IProgressibleElement<T> element, Settings settings)
        {
            Element = element;
            Settings = settings;
        }

        public void Mouse_ClickDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Mouse_LeftClickDown(sender, e);
                    break;
                case MouseButtons.Middle:
                    Mouse_MiddleClickDown(sender, e);
                    break;
                case MouseButtons.Right:
                    Mouse_RightClickDown(sender, e);
                    break;
            }
        }

        public void Mouse_LeftClickDown(object sender, MouseEventArgs e)
        {
            Element.IncrementState();
        }

        public void Mouse_MiddleClickDown(object sender, MouseEventArgs e)
        {
            Element.ResetState();
        }

        public void Mouse_RightClickDown(object sender, MouseEventArgs e)
        {
            Element.DecrementState();
        }

        public void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            Element.HandleMouseWheel(sender, e);
        }
    }
}
