namespace BioCore
{
    public partial class Series : Form
    {
        public Series()
        {
            InitializeComponent();
            UpdateItems();
        }

        /// It clears the listbox, then adds all the items in the Images class to the listbox
        public void UpdateItems()
        {
            imagesBox.Items.Clear();
            foreach (BioImage item in Images.images)
            {
                imagesBox.Items.Add(item);
            }
        }

        /// If the user has selected an image from the list of images, then add that image to the list
        /// of images in the series
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The selected items from the imagesBox are being added to the seriesBox.
        private void addBut_Click(object sender, EventArgs e)
        {
            if (imagesBox.SelectedIndices.Count == 0)
                return;
            foreach (BioImage item in imagesBox.SelectedItems)
            {
                seriesBox.Items.Add(item);
            }
        }

        /// If the user has selected an item in the seriesBox, then remove the selected item from the
        /// seriesBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The selected items in the listbox are being removed.
        private void removeBut_Click(object sender, EventArgs e)
        {
            if (seriesBox.SelectedIndices.Count == 0)
                return;
            foreach (BioImage item in imagesBox.SelectedItems)
            {
                seriesBox.Items.Remove(item);
            }
        }

        /// When the series is activated, update the items
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void Series_Activated(object sender, EventArgs e)
        {
            UpdateItems();
        }

        /// It saves the current series of images to an OME-TIFF file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the file that was saved.
        private void saveOMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            int i = 0;
            List<string> sts = new List<string>();
            foreach (BioImage item in seriesBox.Items)
            {
                sts.Add(item.ID);
                item.series = i;
                i++;
            }
            BioImage.SaveOMESeries(sts.ToArray(), saveFileDialog.FileName, Properties.Settings.Default.Planes);
        }

        /// It takes all the items in the imagesBox and adds them to the seriesBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void addAllBut_Click(object sender, EventArgs e)
        {
            foreach (BioImage item in imagesBox.Items)
            {
                seriesBox.Items.Add(item);
            }
        }

        /// It clears the list of series in the seriesBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void removeAllBut_Click(object sender, EventArgs e)
        {
            seriesBox.Items.Clear();
        }

        /// When the form is closing, hide the form and cancel the closing event.
        /// 
        /// @param sender The object that raised the event.
        /// @param FormClosingEventArgs Provides data for the FormClosing event.
        private void Series_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
