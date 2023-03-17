namespace Bio
{
    public partial class Resolutions : Form
    {
        private int res;
        public int Resolution
        {
            get { return res; }
        }

        /* Adding the items in the list to the combobox. */
        public Resolutions(List<Resolution> res)
        {
            InitializeComponent();
            foreach (var item in res)
            {
                resBox.Items.Add(item);
            }
        }

        /// When the user selects a resolution from the dropdown menu, the variable res is set to the
        /// index of the selected resolution
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void resBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            res = resBox.SelectedIndex;
        }

        /// This function closes the form when the OK button is clicked
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void okBut_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
