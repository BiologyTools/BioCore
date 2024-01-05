namespace BioCore
{
    public partial class OpenInTab : Form
    {
        /* This is the constructor for the class. It is called when an instance of the class is
        created. */
        public OpenInTab()
        {
            InitializeComponent();
        }

        /// When the user clicks the yes button, the dialog result is set to yes and the form is closed.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void yesBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        /// If the user clicks the "No" button, the dialog result is set to "No" and the form is closed.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void noBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
