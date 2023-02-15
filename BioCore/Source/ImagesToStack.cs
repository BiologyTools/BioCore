namespace Bio
{
    public partial class ImagesToStack : Form
    {
        public int SizeZ
        {
            get
            {
                return (int)zBox.Value;
            }
        }
        public int SizeC
        {
            get
            {
                return (int)cBox.Value;
            }
        }
        public int SizeT
        {
            get
            {
                return (int)tBox.Value;
            }
        }
        public ImagesToStack()
        {
            InitializeComponent();
        }

        private void okBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
