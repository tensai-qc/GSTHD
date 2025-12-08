using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    class SometimesHint : TextBox
    {
        public SometimesHint(SortedSet<string> listSometimesHints, AutoFillTextBox textInput, int borderWidth)
        {
            this.BackColor = textInput.BackColor;
            this.Font = new Font(textInput.FontName, textInput.FontSize, textInput.FontStyle);
            this.ForeColor = textInput.FontColor;
            this.Name = textInput.Name;

            this.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            this.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.AutoCompleteSource = AutoCompleteSource.CustomSource;

            this.KeyDown += TextBox_KeyDown;
            this.AutoCompleteCustomSource.AddRange(listSometimesHints.ToArray());
            if (borderWidth > 0)
            {
                this.BorderStyle = BorderStyle.None;
                this.Size = new Size(textInput.Width - borderWidth, textInput.Height - borderWidth);
                this.Location = new Point(4, textInput.Height - textInput.FontSize * 2 + Math.Max(4-borderWidth, 0));
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
                if (array.Length > 0)
                    ((GossipStone)array[0]).Mouse_ClickUp(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
            }
        }
    }
}
