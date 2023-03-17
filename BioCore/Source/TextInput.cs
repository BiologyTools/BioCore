namespace Bio
{
    public partial class TextInput : Form
    {
        string textValue = "";
        public Font font = DefaultFont;
        public Color color = Color.Yellow;
        public string TextValue
        {
            get
            {
                return textValue;
            }
        }
        /* A constructor. */
        public TextInput(string text)
        {
            InitializeComponent();
            textBox.Text = text;
        }

        /// The function is called when the user clicks the OK button. It sets the textValue variable to
        /// the text in the text box and then sets the DialogResult to OK
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void okBut_Click(object sender, EventArgs e)
        {
            textValue = textBox.Text;
            DialogResult = DialogResult.OK;
        }

        /// The function cancelBut_Click is a private function that takes in two parameters, sender and
        /// e, and returns nothing.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void cancelBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// If the user clicks the font button, then show the font dialog, and if the user clicks OK, then set
        /// the font to the font the user selected
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The font that the user selected.
        private void fontBut_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() != DialogResult.OK)
                return;
            font = fontDialog.Font;
        }

        /// If the user clicks the color button, open the color dialog and if the user clicks OK, set
        /// the color to the color the user selected
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The color that the user selected.
        private void colorBut_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
                return;
            color = colorDialog.Color;
        }
    }
}
