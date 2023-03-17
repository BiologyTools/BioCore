namespace Bio
{
    public partial class MagicSelect : Form
    {
        /* A constructor. */
        public MagicSelect(int index)
        {
            InitializeComponent();
            thBox.SelectedIndex = index;
        }
        private bool numeric = false;

        /* A property. */
        public bool Numeric
        {
            get
            {
                return numeric;
            }
        }

        public int Threshold
        {
            get
            {
                return (int)numBox.Value;
            }
        }

        public int Min
        {
            get { return (int)minBox.Value; }
        }
        public int Max
        {
            get { return (int)maxBox.Value; }
        }
        public int Index
        {
            get
            {
                return thBox.SelectedIndex;
            }
        }
        /// If the checkbox is checked, then the variable numeric is set to true
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void numericBox_CheckedChanged(object sender, EventArgs e)
        {
            numeric = numericBox.Checked;
        }

        /// The function is called when the user clicks the OK button
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void okBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
