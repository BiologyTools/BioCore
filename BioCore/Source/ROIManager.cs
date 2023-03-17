namespace Bio
{
    public partial class ROIManager : Form
    {
        /* Creating a drop down menu for the user to select the type of ROI they want to create. */
        public ROIManager()
        {
            InitializeComponent();
            foreach (ROI.Type item in Enum.GetValues(typeof(ROI.Type)))
            {
                typeBox.Items.Add(item);
            }
        }
        public ROI anno = new ROI();
        /// This function clears the list of annotations and then adds each annotation to the list
        /// 
        /// @return The ROI object is being returned.
        public void UpdateAnnotationList()
        {
            if (ImageView.SelectedImage == null)
                return;
            roiView.Items.Clear();
            foreach (ROI an in ImageView.SelectedImage.Annotations)
            {
                ListViewItem it = new ListViewItem();
                it.Tag = an;
                it.Text = an.ToString();
                roiView.Items.Add(it);
            }
        }
        /// > UpdateOverlay() is a function that updates the overlay
        public void UpdateOverlay()
        {
            if (App.viewer != null)
                App.viewer.UpdateOverlay();
        }
        /// This function updates the ROI at the specified index with the new ROI
        /// 
        /// @param index the index of the annotation to be updated
        /// @param ROI This is the region of interest.
        /// 
        /// @return The ROI is being returned.
        public void updateROI(int index, ROI an)
        {
            if (ImageView.SelectedImage == null)
                return;
            ImageView.SelectedImage.Annotations[index] = an;
            UpdateOverlay();
        }
        /// When the value of the xBox changes, the x value of the annotation is set to the value of the
        /// xBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the xBox.Value is being returned.
        private void xBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.X = (double)xBox.Value;
            UpdateOverlay();
        }
        /// When the user changes the value of the Y coordinate, the Y coordinate of the annotation is
        /// updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the yBox.Value is being returned.
        private void yBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.Y = (double)yBox.Value;
            UpdateOverlay();
        }
        /// When the value of the width box changes, the width of the selected ROI is changed to the
        /// value of the width box
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the width of the ROI.
        private void wBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            if (anno.type == ROI.Type.Rectangle || anno.type == ROI.Type.Ellipse)
                anno.W = (double)wBox.Value;
            UpdateOverlay();
        }
        /// This function is called when the value of the height box is changed
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the hBox.Value property.
        private void hBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            if (anno.type == ROI.Type.Rectangle || anno.type == ROI.Type.Ellipse)
                anno.H = (double)hBox.Value;
            UpdateAnnotationList();
            UpdateOverlay();
        }
        /// If the user has selected an item in the listbox, update the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the selected item in the listbox.
        private void sBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            UpdateOverlay();
        }
        /// When the value of the Z-coordinate text box changes, update the Z-coordinate of the
        /// annotation and update the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the Z coordinate of the annotation.
        private void zBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.coord.Z = (int)zBox.Value;
            UpdateOverlay();
        }
/// If the user changes the value of the C coordinate, update the annotation's C coordinate and update
/// the overlay
/// 
/// @param sender The object that raised the event.
/// @param EventArgs 
/// 
/// @return The value of the cBox.Value is being returned.
        private void cBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.coord.C = (int)cBox.Value;
            UpdateOverlay();
        }
        /// When the user changes the value of the T-slider, the T-value of the annotation is updated
        /// and the overlay is updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the trackbar.
        private void tBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.coord.T = (int)cBox.Value;
            UpdateOverlay();
        }
        /// When the value of the red color box changes, the red color value of the selected annotation
        /// is updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The color of the stroke.
        private void rBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.strokeColor = Color.FromArgb((byte)rBox.Value, anno.strokeColor.G, anno.strokeColor.B);
            UpdateOverlay();
        }
        /// When the user changes the value of the green slider, the green value of the stroke color is
        /// updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the gBox.Value property.
        private void gBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.strokeColor = Color.FromArgb(anno.strokeColor.R, (byte)gBox.Value, anno.strokeColor.B);
            UpdateOverlay();
        }
        /// When the value of the blue slider changes, the blue value of the stroke color is updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the slider is being returned.
        private void bBox_ValueChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.strokeColor = Color.FromArgb(anno.strokeColor.R, anno.strokeColor.G, (byte)bBox.Value);
            UpdateOverlay();
        }
        /// If the user selects a new ROI type, update the ROI type and update the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The type of the ROI.
        private void typeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.type = (ROI.Type)typeBox.SelectedItem;
            UpdateOverlay();
        }
        /// When the text in the textbox changes, update the text of the annotation to the text in the
        /// textbox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The textbox is being returned.
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.Text = textBox.Text;
            UpdateOverlay();
        }
        /// When the text in the idBox changes, the id of the annotation is set to the text in the idBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The idBox.Text is being returned.
        private void idBox_TextChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            anno.id = idBox.Text;
            UpdateOverlay();
        }
        /// When the ROI Manager is activated, if the selected image is not null, the name of the image
        /// is displayed in the imageNameLabel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The image name.
        private void ROIManager_Activated(object sender, EventArgs e)
        {
            if (ImageView.SelectedImage == null)
                return;
            string n = System.IO.Path.GetFileName(ImageView.SelectedImage.ID);
            if (imageNameLabel.Text != n)
                imageNameLabel.Text = n;
            UpdateAnnotationList();
        }

        /// When the user selects an annotation in the list view, the function updates the annotation's
        /// properties in the property grid
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The ROI object is being returned.
        private void roiView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            ListViewItem it = roiView.SelectedItems[0];
            anno = (ROI)it.Tag;
            if (App.viewer != null)
                App.viewer.SetCoordinate(anno.coord.Z, anno.coord.C, anno.coord.T);
            if (anno.type == ROI.Type.Line || anno.type == ROI.Type.Polygon ||
               anno.type == ROI.Type.Polyline)
            {
                xBox.Enabled = false;
                yBox.Enabled = false;
                wBox.Enabled = false;
                hBox.Enabled = false;
            }
            else
            {
                xBox.Enabled = true;
                yBox.Enabled = true;
                wBox.Enabled = true;
                hBox.Enabled = true;
            }
            if (anno.type == ROI.Type.Rectangle || anno.type == ROI.Type.Ellipse)
            {
                pointIndexBox.Enabled = false;
                pointXBox.Enabled = false;
                pointYBox.Enabled = false;
            }
            else
            {
                pointIndexBox.Enabled = true;
                pointXBox.Enabled = true;
                pointYBox.Enabled = true;
            }
            xBox.Value = (decimal)anno.X;
            yBox.Value = (decimal)anno.Y;
            wBox.Value = (decimal)anno.W;
            hBox.Value = (decimal)anno.H;
            zBox.Value = anno.coord.Z;
            cBox.Value = anno.coord.C;
            tBox.Value = anno.coord.T;
            rBox.Value = anno.strokeColor.R;
            gBox.Value = anno.strokeColor.G;
            bBox.Value = anno.strokeColor.B;
            strokeWBox.Value = (decimal)anno.strokeWidth;
            idBox.Text = anno.id;
            textBox.Text = anno.Text;
            typeBox.SelectedIndex = (int)anno.type;
            UpdatePointBox();
        }
        /// If the user has selected an annotation in the list view, and an image in the image viewer,
        /// then update the annotation in the image viewer to the annotation in the list view
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The selected item in the listbox.
        private void updateBut_Click(object sender, EventArgs e)
        {
            if (roiView.SelectedItems.Count == 0)
                return;
            if (ImageView.SelectedImage == null)
                return;
            ImageView.SelectedImage.Annotations[roiView.SelectedIndices[0]] = anno;
            UpdateOverlay();
        }
        /// The function adds the annotation to the image and updates the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void addButton_Click(object sender, EventArgs e)
        {
            ImageView.SelectedImage.Annotations.Add(anno);
            UpdateOverlay();
        }
        /// If the checkbox is checked, then the imageviewer will show the bounds of the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data,
        /// and provides a value to use for events that do not include event data.
        private void showBoundsBox_CheckedChanged(object sender, EventArgs e)
        {
            ImageView.showBounds = showBoundsBox.Checked;
            UpdateOverlay();
        }
        /// If the checkbox is checked, then the showText variable is set to true, and the
        /// UpdateOverlay() function is called.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void showTextBox_CheckedChanged(object sender, EventArgs e)
        {
            ImageView.showText = showTextBox.Checked;
            UpdateOverlay();
        }
        /// When the user changes the value of the pointXBox, the function updates the point in the
        /// annotation object and then updates the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the pointXBox.Value is being returned.
        private void pointXBox_ValueChanged(object sender, EventArgs e)
        {
            if (anno == null)
                return;
            if (anno.type == ROI.Type.Rectangle || anno.type == ROI.Type.Ellipse)
                return;
            anno.UpdatePoint(new PointD((double)pointXBox.Value, (double)pointYBox.Value), (int)pointIndexBox.Value);
            UpdateOverlay();
        }
        /// When the user changes the value of the pointYBox, the function updates the point's
        /// y-coordinate and updates the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the pointXBox.Value and pointYBox.Value
        private void pointYBox_ValueChanged(object sender, EventArgs e)
        {
            if (anno == null)
                return;
            if (anno.type == ROI.Type.Rectangle || anno.type == ROI.Type.Ellipse)
                return;
            anno.UpdatePoint(new PointD((double)pointXBox.Value, (double)pointYBox.Value), (int)pointIndexBox.Value);
            UpdateOverlay();
        }
        public bool autoUpdate = true;
        /// This function updates the point box with the current point index
        /// 
        /// @return A PointD object.
        public void UpdatePointBox()
        {
            if (anno == null)
                return;
            PointD d = anno.GetPoint((int)pointIndexBox.Value);
            pointXBox.Value = (int)d.X;
            pointYBox.Value = (int)d.Y;
        }
        /// When the value of the pointIndexBox changes, update the pointBox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void pointIndexBox_ValueChanged(object sender, EventArgs e)
        {
            UpdatePointBox();
        }
        /// If the user clicks the font button, the font dialog is displayed and the font is changed to
        /// the selected font
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The font that was selected in the font dialog.
        private void fontBut_Click(object sender, EventArgs e)
        {
            if (anno == null)
                return;
            if (fontDialog.ShowDialog() != DialogResult.OK)
                return;
            anno.font = fontDialog.Font;
        }
        /// When the stroke width value changes, update the stroke width of the annotation and update
        /// the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The value of the stroke width.
        private void strokeWBox_ValueChanged(object sender, EventArgs e)
        {
            if (anno == null)
                return;
            anno.strokeWidth = (int)strokeWBox.Value;
            UpdateOverlay();
        }
        /// When the value of the select box size slider changes, update the select box size in the
        /// viewer and update the overlay
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void selectBoxSize_ValueChanged(object sender, EventArgs e)
        {
            App.viewer.UpdateSelectBoxSize((float)selectBoxSize.Value);
            UpdateOverlay();
        }
        /// If the checkbox is checked, then show the RROIs
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The return value is the value of the checkbox.
        private void rChBox_CheckedChanged(object sender, EventArgs e)
        {
            if (App.viewer == null)
                return;
            App.viewer.showRROIs = rChBox.Checked;
            UpdateOverlay();
        }
        /// If the checkbox is checked, show the GROIs
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The return value is the value of the checkbox.
        private void gChBox_CheckedChanged(object sender, EventArgs e)
        {
            if (App.viewer == null)
                return;
            App.viewer.showGROIs = gChBox.Checked;
            UpdateOverlay();
        }
        /// If the checkbox is checked, show the BROIs
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The return value is the value of the checkbox.
        private void bChBox_CheckedChanged(object sender, EventArgs e)
        {
            if (App.viewer == null)
                return;
            App.viewer.showBROIs = bChBox.Checked;
            UpdateOverlay();
        }
        /// When the form is closing, cancel the closing event and minimize the form instead.
        /// 
        /// @param sender The object that raised the event.
        /// @param FormClosingEventArgs
        /// https://msdn.microsoft.com/en-us/library/system.windows.forms.formclosingeventargs(v=vs.110).aspx
        private void ROIManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }

        /// This function removes the selected ROI from the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < roiView.SelectedItems.Count; i++)
            {
                ImageView.SelectedImage.Annotations.Remove((ROI)roiView.SelectedItems[i].Tag);
            }
            UpdateAnnotationList();
            UpdateOverlay();
        }

        /// The function is called when the user clicks on the "Copy" menu item in the context menu. It
        /// gets the selected ROI from the list view and converts it to a string and puts it on the
        /// clipboard
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ROI> annotations = new List<ROI>();
            ROI an = (ROI)roiView.SelectedItems[0].Tag;
            Clipboard.SetText(BioImage.ROIToString(an));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
/// If the checkbox is checked, the form will be topmost. If the checkbox is unchecked, the form will
/// not be topmost
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The EventArgs class is the base class for classes containing event data.

        private void topMostBox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = topMostBox.Checked;
        }
    }
}
