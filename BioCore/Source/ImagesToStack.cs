namespace BioCore
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
        /* This is the constructor of the class. It is called when an instance of the class is created. */
        public ImagesToStack()
        {
            InitializeComponent();
        }

        /// The function is called when the user clicks the OK button. It sets the DialogResult property
        /// to OK and closes the form
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void okBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// The cancel button closes the form and sets the dialog result to cancel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void cancelBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
