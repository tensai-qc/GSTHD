using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    class SometimesHint : TextBox
    {
        readonly bool CustomBorder = false;
        readonly SortedSet<string> ListSometimesHints;
        public SometimesHint(SortedSet<string> listSometimesHints, AutoFillTextBox textInput, int borderWidth)
        {
            this.BackColor = textInput.BackColor;
            this.Font = new Font(textInput.FontName, textInput.FontSize, textInput.FontStyle);
            this.ForeColor = textInput.FontColor;
            this.Name = textInput.Name;
            this.ListSometimesHints = listSometimesHints;

            this.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            this.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.AutoCompleteSource = AutoCompleteSource.CustomSource;

            this.KeyDown += TextBox_KeyDown;
            this.KeyUp += TextBox_KeyUp;
            this.AutoCompleteCustomSource.AddRange(listSometimesHints.ToArray());
            if (borderWidth > 0)
            {
                this.CustomBorder = true;
                this.BorderStyle = BorderStyle.None;
                this.Size = new Size(textInput.Width - borderWidth, textInput.Height - borderWidth);
                this.Location = new Point(4, textInput.Height - textInput.FontSize * 2 + Math.Max(4 - borderWidth, 0));
            }
            else
            {
                this.Size = new Size(textInput.Width, textInput.Height);
                this.Location = new Point(textInput.X, textInput.Y);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var array = this.Parent.Controls.Find(this.Name + "_GossipStone", false);
                foreach (string hint in ListSometimesHints.ToArray())
                {
                    if (Text.Equals(hint, StringComparison.OrdinalIgnoreCase))
                    {
                        Text = hint;
                        break;
                    }
                }
                if (array.Length > 0)
                {
                    ((GossipStone)array[0]).Mouse_ClickUp(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
                ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                var textbox = (TextBox)sender;
                Control currentControl = (Control)sender;
                Control parentControl = currentControl.Parent.Parent;
                Form parentForm = currentControl.FindForm();
                Control parentFormControl = parentForm.Controls[0];

                var tabOrderControls = parentFormControl.Controls.Cast<Control>()
                        .Where(c => c.GetType().Name == "SometimesHint" || c.Name == "SometimesHintWrapper" || c.GetType().Name == "PanelWothBarren")
                        .OrderBy(c => c.TabIndex)
                        .ToList();

                int currentIndex;
                if (CustomBorder)
                {
                    currentIndex = tabOrderControls.IndexOf(parentControl);
                }
                else
                {
                    currentIndex = tabOrderControls.IndexOf(currentControl);
                }

                if (currentIndex < tabOrderControls.Count - 1)
                {
                    if (tabOrderControls[currentIndex + 1].Name == "SometimesHintWrapper")
                    {
                        tabOrderControls[currentIndex + 1].Controls[0].Controls[0].Focus();
                    }
                    else if (tabOrderControls[currentIndex + 1].GetType().Name == "SometimesHint")
                    {
                        tabOrderControls[currentIndex + 1].Focus();
                    }
                    else
                    {
                        tabOrderControls[currentIndex + 1].Controls[0].Focus();
                    }
                }

            }
        }
    }
}
