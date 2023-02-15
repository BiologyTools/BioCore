namespace Bio
{
    public partial class OpenInTab : Form
    {
        public OpenInTab()
        {
            InitializeComponent();
        }

        private void yesBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void noBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
