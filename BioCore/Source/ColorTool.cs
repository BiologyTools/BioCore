namespace Bio
{
    public partial class ColorTool : Form
    {
        private ColorS color = new ColorS(65535, 65535, 65535);
        private int bitsPerPixel = 16;
        /* A property. */
        public ColorS Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        /// It updates the GUI
        public void UpdateGUI()
        {
            color = new ColorS((ushort)redBox.Value, (ushort)greenBox.Value, (ushort)blueBox.Value);
            colorPanel.BackColor = ColorS.ToColor(color, bitsPerPixel);
            if (rBar.Value != redBox.Value)
                redBox.Value = rBar.Value;
            if (gBar.Value != greenBox.Value)
                greenBox.Value = gBar.Value;
            if (bBar.Value != blueBox.Value)
                blueBox.Value = bBar.Value;
        }
        /* A constructor. */
        public ColorTool()
        {
            InitializeComponent();
            UpdateGUI();
        }
        /* A constructor. */
        public ColorTool(ColorS col, int bitPerPixel)
        {
            InitializeComponent();
            this.bitsPerPixel = bitPerPixel;
            if (bitsPerPixel == 8)
            {
                rBar.Maximum = 255;
                gBar.Maximum = 255;
                bBar.Maximum = 255;
                redBox.Maximum = 255;
                greenBox.Maximum = 255;
                blueBox.Maximum = 255;
            }
            if (rBar.Maximum <= col.R)
                rBar.Value = rBar.Maximum;
            if (gBar.Maximum <= col.G)
                gBar.Value = gBar.Maximum;
            if (bBar.Maximum <= col.B)
                bBar.Value = bBar.Maximum;
            UpdateGUI();
        }

        /// The function is called when the value of the redBox control changes
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void redBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        /// The function is called when the value of the greenBox is changed
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void greenBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        /// When the value of the blueBox changes, update the GUI
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void blueBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        /// If the checkbox is checked, set the value of the variable to true. If the checkbox is not
        /// checked, set the value of the variable to false
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void rEnbaled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.rEnabled = rEnbaled.Checked;
        }

        /// If the checkbox is checked, set the value of the gEnabled variable to true
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void gEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.gEnabled = gEnabled.Checked;
        }

        /// If the checkbox is checked, set the value of the bEnabled variable to true. If the checkbox
        /// is unchecked, set the value of the bEnabled variable to false
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void bEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.bEnabled = bEnabled.Checked;
        }

        /// This function is called when the user clicks the "Apply" button
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void applyButton_Click(object sender, EventArgs e)
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

        /// The function is called when the scroll bar is moved
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void rBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        /// The function is called when the scroll bar is moved
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void gBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        /// The function is called when the scroll bar is moved
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void bBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }
    }
}
