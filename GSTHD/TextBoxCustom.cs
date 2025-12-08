using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GSTHD
{
    class TextBoxCustom : Control
    {
        public TextBox TextBoxField;
        public ListBox SuggestionContainer;
        readonly Dictionary<string, string> ListSuggestion;
        bool SuggestionContainerIsFocus = false;
        bool SuggestionContainerIsDisabled = false;
        Action<object, KeyEventArgs> KeyDownAction = null;

        public TextBoxCustom(Dictionary<string, string> listSuggestion, Point location, Color color, Font font, string name, Size size, string text)
        {
            ListSuggestion = listSuggestion;

            TextBoxField = new TextBox
            {
                Location = location,
                BorderStyle = BorderStyle.None,
                BackColor = color,
                CausesValidation = false,
                Font = font,
                ForeColor = Color.White,
                Name = name,
                AutoSize = false,
                Size = size,
                Text = text,
                AcceptsTab = true
            };
            TextBoxField.KeyUp += TextBoxCustom_KeyUp;
            TextBoxField.PreviewKeyDown += TextBoxField_PreviewKeyDown;
            TextBoxField.KeyDown += TextBoxField_KeyDown;
            TextBoxField.LostFocus += TextBoxField_LostFocus;
            TextBoxField.SelectionStart = TextBoxField.TextLength;

            SuggestionContainer = new ListBox
            {
                Visible = false,
                Location = new Point(TextBoxField.Location.X, TextBoxField.Location.Y + TextBoxField.Height + 5),
                IntegralHeight = false,
                Width = 155,
                Sorted = true
            };
            SuggestionContainer.Items.AddRange(listSuggestion.Keys.ToArray());
            SuggestionContainer.KeyDown += SuggestionContainer_KeyDown;
            SuggestionContainer.KeyUp += SuggestionContainer_KeyUp;
            SuggestionContainer.LostFocus += SuggestionContainer_LostFocus;
            SuggestionContainer.MouseEnter += SuggestionContainer_MouseEnter;
            SuggestionContainer.MouseLeave += SuggestionContainer_MouseLeave;
        }

        public void SetKeyDownAction(Action<object, KeyEventArgs> action)
        {
            KeyDownAction = action;
        }

        private void SuggestionContainer_LostFocus(object sender, EventArgs e)
        {
            SuggestionContainer.Hide();
        }

        private void TextBoxField_LostFocus(object sender, EventArgs e)
        {
            if (!SuggestionContainerIsFocus) SuggestionContainer.Hide();
        }

        private void TextBoxField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if(e.Control && e.KeyCode == Keys.R)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TextBoxField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.IsInputKey = true;
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.SelectedIndex = 0;
                    TextBoxField.Text = SuggestionContainer.Text;
                    SuggestionContainer.Hide();
                    SuggestionContainer.Items.Clear();
                }
            }

            if (e.KeyCode == Keys.Enter)
            {
                var textbox = (TextBox)sender;
                if(!SuggestionContainer.Items.Contains(textbox.Text) && SuggestionContainer.Items.Count > 0)
                    textbox.Text = SuggestionContainer.Items[0].ToString();
            }

            if (e.KeyCode == Keys.Down)
            {
                SuggestionContainerIsFocus = true;
            }
        }
        private void SuggestionContainer_MouseEnter(object sender, EventArgs e)
        {
            SuggestionContainerIsFocus = true;
        }

        private void SuggestionContainer_MouseLeave(object sender, EventArgs e)
        {
            SuggestionContainerIsFocus = false;
        }

        private void SuggestionContainer_KeyDown(object sender, KeyEventArgs e)
        {
            SuggestionContainerIsFocus = true;
        }

        private void SuggestionContainer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBoxField.Text = SuggestionContainer.SelectedItem.ToString();
                TextBoxField.Focus();
                SuggestionContainer.Hide();
                SuggestionContainerIsFocus = false;
                KeyDownAction?.Invoke(TextBoxField, e);
            }
        }

        private void TextBoxCustom_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.Focus();
                    SuggestionContainer.SelectedIndex = 0;
                }
            }
            else if (e.KeyCode == Keys.Tab)
            {
                //Do Nothing
            }
            else
            {
                var textbox = (TextBox)sender;
                SuggestionContainer.Items.Clear();

                var listTagFiltered = ListSuggestion.Where(x => x.Value.Contains(textbox.Text.ToLower())).Select(y => y.Key);
                SuggestionContainer.Items.AddRange(listTagFiltered.ToArray());

                var listPlacesFiltered = ListSuggestion.Keys.Where(x => x.ToLower().Contains(textbox.Text.ToLower()));
                foreach (var element in listPlacesFiltered.ToArray())
                {
                    if (!SuggestionContainer.Items.Contains(element))
                        SuggestionContainer.Items.Add(element);
                }
                if (TextBoxField.Text.Length > 0 && SuggestionContainer.Items.Count > 0 && !this.SuggestionContainerIsDisabled)
                {
                    SuggestionContainer.Show();
                }
                else
                {
                    SuggestionContainer.Hide();
                }
            }
        }

        public void SetSuggestionsContainerLocation(Point location)
        {
            SuggestionContainer.Location = new Point
            (
                location.X + TextBoxField.Location.X,
                location.Y + TextBoxField.Location.Y + TextBoxField.Height
            );
        }

        public void SetSuggestionsContainerDisabled(bool disabled)
        {
            this.SuggestionContainerIsDisabled = disabled;
        }

    public void NewLocation(Point newLocation, Point panelLocation)
        {
            TextBoxField.Location = newLocation;
            SetSuggestionsContainerLocation(panelLocation);
        }
    }
}
