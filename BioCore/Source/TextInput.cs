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
        public TextInput(string text)
        {
            InitializeComponent();
            textBox.Text = text;
        }

        private void okBut_Click(object sender, EventArgs e)
        {
            textValue = textBox.Text;
            DialogResult = DialogResult.OK;
        }

        private void cancelBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void fontBut_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() != DialogResult.OK)
                return;
            font = fontDialog.Font;
        }

        private void colorBut_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
                return;
            color = colorDialog.Color;
        }
    }
}
