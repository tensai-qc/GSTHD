using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    class CollectedItem : PictureBox, IProgressibleElement<int>, IDraggableAutocheckElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private readonly string[] ImageNames;
        private int ImageIndex = 0;
        private readonly Label ItemCount;
        private Size CollectedItemSize;
        private Size CollectedItemCountPosition;
        private readonly bool HideMin = false;
        private readonly int CollectedItemMin;
        private readonly int CollectedItemMax;
        private readonly int CollectedItemDefault;
        private int CollectedItems;
        private readonly int Step;
        private int StepIndex = 0;
        private readonly int[] Steps;
        private readonly Color LabelColor;
        private readonly Color CountMaxLabelColor;

        public CollectedItem(ObjectPointCollectedItem data, Settings settings)
        {
            Settings = settings;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            Steps = data.Steps;
            CollectedItemMin = Steps != null ? Steps[0] : data.CountMin;
            CollectedItemMax = Steps != null ? Steps[Steps.Length - 1] : data.CountMax ?? 100;
            CollectedItemDefault = Steps != null ? Steps[0] : data.DefaultValue;
            Step = data.Step == 0 ? 1 : data.Step;
            CollectedItems = Steps != null ? Steps[0] : System.Math.Min(System.Math.Max(CollectedItemMin, CollectedItemDefault), CollectedItemMax);
            HideMin = data.HideMin;
            CollectedItemSize = data.Size;
            LabelColor = data.LabelColor;
            CountMaxLabelColor = data.CountMaxLabelColor;

            if (ImageNames.Length > 0)
            {
                Image = Image.FromFile(@"Resources/" + ImageNames[0]);
                Name = ImageNames[0];
                SizeMode = PictureBoxSizeMode.StretchImage;
                Size = CollectedItemSize;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<int>(this, Settings);

            Location = new Point(data.X, data.Y);
            CollectedItemCountPosition = data.CountPosition.IsEmpty ? new Size(0, -7) : data.CountPosition;
            BackColor = Color.Transparent;
            TabStop = false;


            ItemCount = new Label
            {
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Text = HideMin == true && CollectedItems == CollectedItemMin ? "" : CollectedItems.ToString(),
                Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                ForeColor = data.LabelColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 30,
                Width = 40,
                Location = new Point((CollectedItemSize.Width / 2) + CollectedItemCountPosition.Width - 19, (CollectedItemSize.Height / 2) + CollectedItemCountPosition.Height - 15),
            };

            MouseDown += ProgressBehaviour.Mouse_ClickDown;
            MouseUp += DragBehaviour.Mouse_ClickUp;
            MouseDown += DragBehaviour.Mouse_ClickDown;
            MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
            MouseWheel += Mouse_Wheel;
            ItemCount.MouseDown += ProgressBehaviour.Mouse_ClickDown; // must add these lines because MouseDown/Up on PictureBox won't fire when hovering above Label
            ItemCount.MouseDown += DragBehaviour.Mouse_ClickDown;
            ItemCount.MouseUp += DragBehaviour.Mouse_ClickUp;
            ItemCount.MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
            // ItemCount.MouseWheel += Click_MouseWheel; // must NOT add this line because both MouseWheels would fire when hovering above both PictureBox and Label

            Controls.Add(ItemCount);
        }

        private void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            ProgressBehaviour.HandleMouseWheel(sender, e);
            DragBehaviour.HandleMouseWheel(sender, e);
        }
        public void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                if (Steps != null)
                {
                    StepIndex += Step * (Settings.InvertScrollWheel ? scrolls : -scrolls);
                    if (StepIndex < 0) StepIndex = 0;
                    if (StepIndex > Steps.Length - 1) StepIndex = Steps.Length - 1;
                    CollectedItems = Steps[StepIndex];
                }
                else
                {
                    CollectedItems += Step * (Settings.InvertScrollWheel ? scrolls : -scrolls);
                    if (CollectedItems < CollectedItemMin) CollectedItems = CollectedItemMin;
                    else if (CollectedItems > CollectedItemMax) CollectedItems = CollectedItemMax;
                }
                UpdateCount();
                if ((Steps != null || CollectedItems == CollectedItemMin) && ImageIndex > 0) ImageIndex -= 1;
                if (CollectedItems > CollectedItemMin && ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
                UpdateImage();
            }
        }

        private void UpdateCount()
        {
            if (HideMin == true && CollectedItems == CollectedItemMin) ItemCount.Text = "";
            else ItemCount.Text = CollectedItems.ToString();
            if (CollectedItems == CollectedItemMax) ItemCount.ForeColor = CountMaxLabelColor;
            else ItemCount.ForeColor = LabelColor;
            if (Steps != null)
            {
                StepIndex = Array.FindIndex(Steps, item => item == CollectedItems);
            }
        }

        private void UpdateImage()
        {
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
        }

        public int GetState()
        {
            return CollectedItems;
        }

        public void SetState(int state)
        {
            CollectedItems = state;
            UpdateCount();
            if (state == CollectedItemMin) ImageIndex = 0;
            UpdateImage();
        }

        public void IncrementState()
        {
            if (Steps != null)
            {
                if (StepIndex < Steps.Length - 1) StepIndex++;
                CollectedItems = Steps[StepIndex];
            }
            else
            {
                CollectedItems += Step;
                if (CollectedItems > CollectedItemMax) CollectedItems = CollectedItemMax;
            }
            UpdateCount();

            if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
            UpdateImage();
        }

        public void DecrementState()
        {
            if (Steps != null)
            {
                if (StepIndex > 0) StepIndex--;
                CollectedItems = Steps[StepIndex];
            }
            else
            {
                CollectedItems -= Step;
                if (CollectedItems < CollectedItemMin) CollectedItems = CollectedItemMin;
            }
            UpdateCount();

            if ((Steps != null || CollectedItems == CollectedItemMin)
                && ImageIndex > 0
                && StepIndex < ImageNames.Length - 1)
            {
                ImageIndex -= 1;
            }
            UpdateImage();
        }


        public void ResetState()
        {
            CollectedItems = CollectedItemDefault;
            StepIndex = 0;
            UpdateCount();

            ImageIndex = 0;
            UpdateImage();
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[ImageNames.Length > 1 ? 1 : 0]);
            DoDragDrop(dropContent, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges()
        {
            if ((Steps != null || CollectedItems == CollectedItemMin) && ImageIndex > 0)
            {
                int stepIndex = Array.FindIndex(Steps, item => item == CollectedItems);
                if (stepIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
                else ImageIndex = stepIndex;
            }
        }
    }
}
