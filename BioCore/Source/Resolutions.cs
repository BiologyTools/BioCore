namespace Bio
{
    public partial class Resolutions : Form
    {
        private int res;
        public int Resolution
        {
            get { return res; }
        }

        public Resolutions(List<Resolution> res)
        {
            InitializeComponent();
            foreach (var item in res)
            {
                resBox.Items.Add(item);
            }
        }

        private void resBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            res = resBox.SelectedIndex;
        }

        private void okBut_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
