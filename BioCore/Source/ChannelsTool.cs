using AForge;
using System.ComponentModel;
namespace BioCore
{
    public partial class ChannelsTool : Form
    {
        private HistogramControl hist = null;
        private bool initializing;
        private int histogramRequestVersion;
        private BioImage histogramLoadingImage;
        private Task histogramLoadingTask;
        private BioImage histogramImage;
        private ZCT histogramCoordinate;
        private ZCT histogramLoadingCoordinate;
        /* A property that returns a list of channels. */
        public List<Channel> Channels
        {
            get
            {
                return App.Channels;
            }
        }
        /* A property that returns the selected channel from the channelsBox. */
        public Channel SelectedChannel
        {
            get
            {
                if (channelsBox.SelectedIndex != -1)
                    return Channels[channelsBox.SelectedIndex];
                else
                    return Channels[0];
            }
        }
        /* A property that is used to get and set the value of the sampleBox.Value. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedSample
        {
            get
            {
                return (int)sampleBox.Value;
            }
            set
            {
                sampleBox.Value = (decimal)value;
            }
        }
        /// It clears the listbox and then adds all the items in the Channels list to the listbox.
        public void UpdateItems()
        {
            int selectedIndex = channelsBox.SelectedIndex;
            initializing = true;
            channelsBox.Items.Clear();
            foreach (Channel item in Channels)
            {
                channelsBox.Items.Add(item);
            }
            channelsBox.SelectedIndex = Channels.Count == 0
                ? -1
                : Math.Clamp(selectedIndex, 0, Channels.Count - 1);
            initializing = false;
        }
        /* Creating a new instance of the ChannelsTool class. */
        public ChannelsTool(List<Channel> Channels)
        {
            initializing = true;
            InitializeComponent();
            foreach (Channel item in Channels)
            {
                channelsBox.Items.Add(item);
            }
            MouseWheel += new System.Windows.Forms.MouseEventHandler(ChannelsTool_MouseWheel);

            if (Channels.Count == 0)
            {
                channelsBox.Enabled = false;
                sampleBox.Enabled = false;
                statsPanel.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "No channel metadata is available for this image.",
                    TextAlign = ContentAlignment.MiddleCenter
                });
                initializing = false;
                return;
            }

            channelsBox.SelectedIndex = 0;
            Channel channel = Channels[0];
            int sample = channel.range?.Length > 0 ? 0 : -1;
            int defaultMax = channel.BitsPerPixel <= 8 ? byte.MaxValue : ushort.MaxValue;
            int minimum = sample >= 0 ? channel.range[sample].Min : 0;
            int maximum = sample >= 0 ? channel.range[sample].Max : defaultMax;
            if (channel.stats?.Length > 0)
            {
                minimum = (int)channel.stats[0].StackMin;
                maximum = (int)channel.stats[0].StackMax;
            }
            minBox.Value = Math.Clamp(minimum, (int)minBox.Minimum, (int)minBox.Maximum);
            maxBox.Value = Math.Clamp(maximum, (int)maxBox.Minimum, (int)maxBox.Maximum);
            hist = new HistogramControl(channel);
            hist.GraphMax = Math.Max(1, (int)maxBox.Value);
            maxGraphBox.Value = Math.Clamp(maxBox.Value, maxGraphBox.Minimum, maxGraphBox.Maximum);
            statsPanel.Controls.Add(hist);
            initializing = false;
            Shown += async (_, _) => await EnsureHistogramAsync();
        }

        /// When the user changes the minimum value of the histogram, the histogram is updated, the
        /// image is updated, and the view is updated
        /// 
        /// @param sender System.Object
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The value of the minBox.Value
        private void minBox_ValueChanged(object sender, EventArgs e)
        {
            if (initializing || channelsBox.SelectedIndex == -1)
                return;
            if (hist != null)
            {
                hist.UpdateChannel(Channels[channelsBox.SelectedIndex]);
                hist.Invalidate();
                hist.Min = (int)minBox.Value;
            }
            App.Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Max = (int)maxBox.Value;
            App.viewer.UpdateImage();
            App.viewer.UpdateView();
        }
        /// When the user changes the value of the maxBox, the histogram is updated to reflect the new
        /// value
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The value of the maxBox.Value
        private void maxBox_ValueChanged(object sender, EventArgs e)
        {
            if (initializing || channelsBox.SelectedIndex == -1)
                return;
            if (hist != null)
            {
                hist.UpdateChannel(Channels[channelsBox.SelectedIndex]);
                hist.Invalidate();
                hist.Max = (int)maxBox.Value;
            }
            App.Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Min = (int)minBox.Value;
            App.viewer.UpdateImage();
            App.viewer.UpdateView();
        }
        /// When the user selects a channel from the dropdown menu, the function updates the histogram
        /// and the image viewer
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        /// 
        /// @return The selected item in the channelsBox.
        private void channelsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing || channelsBox.SelectedIndex == -1)
                return;
            Channel channel = Channels[channelsBox.SelectedIndex];
            if (channel.range == null || channel.range.Length == 0)
                return;
            sampleBox.Value = 0;
            sampleBox.Maximum = channel.range.Length - 1;
            if (minBox.Maximum < Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Min || maxBox.Maximum < Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Max)
            {
                minBox.Value = 0;
                maxBox.Value = ushort.MaxValue;
            }
            else
            {
                minBox.Value = Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Min;
                maxBox.Value = Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Max;
            }
            if (hist != null)
            {
                //hist.Statistics = Channels[channelsBox.SelectedIndex].statistics;
                hist.UpdateChannel(SelectedChannel);
                hist.Invalidate();
            }
            fluorBox.Text = SelectedChannel.Fluor;
            emisBox.Value = (decimal)SelectedChannel.Emission;
            excBox.Value = (decimal)SelectedChannel.Excitation;
            App.viewer.UpdateView();
        }

        /// If the user selects a value in the dropdown list that is less than or equal to the maximum
        /// value of the numeric up/down control, then set the value of the numeric up/down control to
        /// the value selected in the dropdown list
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void maxUintBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = int.Parse((string)maxUintBox.SelectedItem, System.Globalization.CultureInfo.InvariantCulture);
            if (i <= maxBox.Maximum)
                maxBox.Value = i;

        }

        /// If the user tries to close the form, cancel the close event and hide the form instead
        /// 
        /// @param sender The object that raised the event.
        /// @param FormClosingEventArgs The event data generated from the form closing.
        private void ChannelsTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        /// For each channel, set the maximum value of each range to the value in the maxBox
        /// 
        /// @param sender
        /// @param EventArgs System.EventArgs
        private void setMaxAllBut_Click(object sender, EventArgs e)
        {
            foreach (Channel c in Channels)
            {
                for (int i = 0; i < c.range.Length; i++)
                {
                    c.range[i].Max = (int)maxBox.Value;
                }
            }
            App.viewer.UpdateView();
        }

        /// For each channel, set the minimum value of each range to the value in the minBox
        /// 
        /// @param sender
        /// @param EventArgs System.EventArgs
        private void setMinAllBut_Click(object sender, EventArgs e)
        {
            foreach (Channel c in Channels)
            {
                for (int i = 0; i < c.range.Length; i++)
                {
                    c.range[i].Min = (int)minBox.Value;
                }
            }
            App.viewer.UpdateView();
        }

        /// This function is called when the user selects a channel from the dropdown menu. It updates
        /// the histogram and the min/max values
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        private void ChannelsTool_Activated(object sender, EventArgs e)
        {
            UpdateItems();
            if (Channels.Count == 0 || channelsBox.SelectedIndex < 0 || hist == null)
                return;
            Channel channel = SelectedChannel;
            if (channel.range == null || channel.range.Length == 0)
                return;
            initializing = true;
            sampleBox.Maximum = channel.range.Length - 1;
            sampleBox.Value = Math.Min(sampleBox.Value, sampleBox.Maximum);
            minBox.Value = Math.Clamp(channel.range[(int)sampleBox.Value].Min,
                (int)minBox.Minimum, (int)minBox.Maximum);
            maxBox.Value = Math.Clamp(channel.range[(int)sampleBox.Value].Max,
                (int)maxBox.Minimum, (int)maxBox.Maximum);
            initializing = false;
            hist.UpdateChannel(channel);
            hist.UpdateView();
            _ = EnsureHistogramAsync();

        }

        private async Task EnsureHistogramAsync()
        {
            BioImage image = ImageView.SelectedImage;
            if (image == null || hist == null)
                return;

            ZCT coordinate = image.Coordinate;
            int requestVersion = ++histogramRequestVersion;
            if (ReferenceEquals(histogramImage, image) && SameCoordinate(histogramCoordinate, coordinate) &&
                ZarrChannelView.HasHistogramStatistics(image))
            {
                hist.StatusMessage = "";
                ApplyFrameRange();
                hist.Invalidate();
                return;
            }

            hist.StatusMessage = "Loading histogram…";
            hist.Invalidate();
            try
            {
                if (!ReferenceEquals(histogramLoadingImage, image) ||
                    !SameCoordinate(histogramLoadingCoordinate, coordinate) ||
                    histogramLoadingTask == null || histogramLoadingTask.IsCompleted)
                {
                    histogramLoadingImage = image;
                    histogramLoadingCoordinate = coordinate;
                    histogramLoadingTask = ZarrChannelView.EnsureHistogramStatisticsAsync(
                        image, coordinate);
                }
                await histogramLoadingTask;
                if (requestVersion != histogramRequestVersion || image != ImageView.SelectedImage)
                    return;
                histogramImage = image;
                histogramCoordinate = coordinate;
                hist.StatusMessage = ZarrChannelView.HasHistogramStatistics(image)
                    ? ""
                    : "Histogram data is unavailable for this image.";
                hist.UpdateChannel(SelectedChannel);
                ApplyFrameRange();
                hist.Invalidate();
            }
            catch (Exception ex)
            {
                if (requestVersion != histogramRequestVersion)
                    return;
                hist.StatusMessage = $"Could not load histogram: {ex.Message}";
                hist.Invalidate();
            }
        }

        public void RefreshHistogram()
        {
            if (Visible)
                _ = EnsureHistogramAsync();
        }

        private void ApplyFrameRange()
        {
            if (channelsBox.SelectedIndex < 0 || SelectedChannel.stats == null ||
                SelectedChannel.stats.Length == 0)
                return;

            int sample = Math.Clamp((int)sampleBox.Value, 0, SelectedChannel.stats.Length - 1);
            Statistics statistics = SelectedChannel.stats[sample];
            int minimum = Math.Clamp((int)statistics.StackMin, (int)minBox.Minimum, (int)minBox.Maximum);
            int maximum = Math.Clamp((int)statistics.StackMax, minimum, (int)maxBox.Maximum);
            int graphMaximum = Math.Max(1, maximum);
            int graphMinimum = Math.Min(minimum, graphMaximum);
            int histogramBin = Math.Max(1, (graphMaximum - graphMinimum + 255) / 256);

            initializing = true;
            minBox.Value = minimum;
            maxBox.Value = maximum;
            minGraphBox.Value = Math.Clamp(graphMinimum,
                (int)minGraphBox.Minimum, (int)minGraphBox.Maximum);
            maxGraphBox.Value = Math.Clamp(graphMaximum,
                (int)maxGraphBox.Minimum, (int)maxGraphBox.Maximum);
            binBox.Value = Math.Clamp(histogramBin, (int)binBox.Minimum, (int)binBox.Maximum);
            initializing = false;

            hist.GraphMin = graphMinimum;
            hist.GraphMax = graphMaximum;
            hist.Bin = histogramBin;
        }

        private static bool SameCoordinate(ZCT left, ZCT right) =>
            left.Z == right.Z && left.C == right.C && left.T == right.T;

        /// When the user resizes the window, the histogram is updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void ChannelsTool_ResizeEnd(object sender, EventArgs e)
        {
            hist?.UpdateView();
        }

        /// If the user selects a value in the dropdown box, then the value of the numeric up/down box
        /// is set to the same value
        /// 
        /// @param sender The control/object that raised the event.
        /// @param EventArgs e
        private void maxUintBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = int.Parse((string)maxUintBox2.SelectedItem, System.Globalization.CultureInfo.InvariantCulture);
            if (i <= maxGraphBox.Maximum)
                maxGraphBox.Value = i;
            if (i == 255)
            {
                maxGraphBox.Value = 255;
                binBox.Value = 1;
            }
        }

        /// When the user changes the value of the minimum graph value, the histogram's minimum graph
        /// value is changed to the new value and the histogram is redrawn
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void minGraphBox_ValueChanged(object sender, EventArgs e)
        {
            if (hist != null)
            {
                hist.GraphMin = (int)minGraphBox.Value;
                hist.Invalidate();
            }
        }

        /// When the user changes the value of the maxGraphBox, the histogram's GraphMax property is set
        /// to the new value and the histogram is invalidated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void maxGraphBox_ValueChanged(object sender, EventArgs e)
        {
            if (hist != null)
            {
                hist.GraphMax = (int)maxGraphBox.Value;
                hist.Invalidate();
            }
        }
       /// If the histogram is not null, set the bin value to the value of the binBox and invalidate the
       /// histogram
       /// 
       /// @param sender The object that raised the event.
       /// @param EventArgs The event arguments.
        private void binBox_ValueChanged(object sender, EventArgs e)
        {
            if (hist != null)
            {
                hist.Bin = (int)binBox.Value;
                hist.Invalidate();
            }
        }

        /// This function is called when the user clicks on the ChannelsTool
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs The event data.
        private void ChannelsTool_MouseDown(object sender, MouseEventArgs e)
        {

        }
        private bool pressedX1 = false;
        private bool pressedX2 = false;
        /// If the mouse is over the channelsBox and the user presses the XButton1, then the selected
        /// index of the channelsBox is increased by one. If the user presses the XButton2, then the
        /// selected index of the channelsBox is decreased by one
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs e
        /// 
        /// @return The return statement is used to explicitly return from a method.
        private void ChannelsTool_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                if (channelsBox.SelectedIndex < channelsBox.Items.Count - 1)
                {
                    if (pressedX1 == false)
                    {
                        channelsBox.SelectedIndex++;
                        pressedX1 = true;
                        return;
                    }
                }
            }
            pressedX1 = false;
            if (e.Button == MouseButtons.XButton2)
            {
                if (channelsBox.SelectedIndex > 0)
                {
                    if (pressedX2 == false)
                    {
                        channelsBox.SelectedIndex--;
                        pressedX2 = true;
                        return;
                    }
                }
            }
            pressedX2 = false;
        }

        /// If the mouse wheel is scrolled up, increase the value of the maxGraphBox by 10 or 100,
        /// depending on the current value of maxGraphBox. If the mouse wheel is scrolled down, decrease
        /// the value of the maxGraphBox by 10 or 100, depending on the current value of maxGraphBox
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs e
        /// 
        /// @return The value of the maxGraphBox.Value is being returned.
        private void ChannelsTool_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0)
                return;
            int i = 100;
            if (maxGraphBox.Value < 255)
                i = 10;
            if (e.Delta > 0)
            {
                if (maxGraphBox.Value + i < maxGraphBox.Maximum)
                {
                    maxGraphBox.Value += i;
                    hist.Invalidate();
                    return;
                }
            }
            else
            if (maxGraphBox.Value - i > maxGraphBox.Minimum)
            {
                maxGraphBox.Value -= i;
                hist.Invalidate();
                return;
            }
        }

        /// If the checkbox is checked, the histogram will be stacked
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void stackHistoBox_CheckedChanged(object sender, EventArgs e)
        {
            hist.StackHistogram = stackHistoBox.Checked;
            hist.Invalidate();
        }

        /// When the user clicks on the "Set Min" menu item, the value of the minimum box is set to the
        /// value of the mouse's x-coordinate
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void setMinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minBox.Value = (int)hist.MouseValX;
        }

        /// It sets the maximum value of the histogram to the value of the mouse cursor
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void setMaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maxBox.Value = (int)hist.MouseValX;
        }

        /// The function takes the selected image and bakes it using the ranges of the R, G, and B
        /// channels
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void applyBut_Click(object sender, EventArgs e)
        {
            ImageView.SelectedImage.Bake(ImageView.SelectedImage.RChannel.RangeR, ImageView.SelectedImage.GChannel.RangeG, ImageView.SelectedImage.BChannel.RangeB);
        }

        /// When the user clicks the "Min" menu item, the minBox's value is set to the minimum value of
        /// the selected channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void minToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMin;
        }

        /// When the user clicks the "Max" button in the menu, the minimum value of the selected channel
        /// is set to the maximum value of the selected channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void maxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMax;
        }

        /// When the user clicks on the "Median" menu item, the minimum value of the selected channel is
        /// set to the median value of the selected channel
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        private void medianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMedian;
        }

        /// When the user clicks on the "Mean" menu item, the minimum value of the selected channel is
        /// set to the mean of the selected channel
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        private void meanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMean;
        }

        /// When the user clicks on the "Set Max" menu item, the maxBox.Value is set to the minimum
        /// value of the selected channel
        /// 
        /// @param sender System.Object
        /// @param EventArgs System.EventArgs
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            maxBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMin;
        }

        /// When the user clicks on the "Set Max" menu item, the max value of the selected channel is
        /// set to the maximum value of the selected channel
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            maxBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMax;
        }
        /// When the user clicks on the "Set Max to Median" menu item, the maxBox.Value is set to the
        /// median value of the selected channel
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            maxBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMedian;
        }
        /// When the user clicks on the "Set Max to Mean" menu item, the maximum value of the selected
        /// channel is set to the mean value of the selected channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            maxBox.Value = (decimal)SelectedChannel.stats[channelsBox.SelectedIndex].StackMean;
        }

        /// The function updates the threshold values of the image
        /// 
        /// @param sender
        /// @param EventArgs e
        private void updateBut_Click(object sender, EventArgs e)
        {
            if (ImageView.SelectedImage.bitsPerPixel > 8)
            {
                ImageView.SelectedImage.StackThreshold(true);
                if (ImageView.SelectedImage.RGBChannelCount == 1)
                {
                    maxBox.Value = (decimal)ImageView.SelectedImage.Channels[channelsBox.SelectedIndex].stats[0].StackMax;
                    minBox.Value = (decimal)ImageView.SelectedImage.Channels[channelsBox.SelectedIndex].stats[0].StackMin;
                }
                else
                {
                    maxBox.Value = (decimal)ImageView.SelectedImage.Channels[channelsBox.SelectedIndex].stats[channelsBox.SelectedIndex].StackMax;
                    minBox.Value = (decimal)ImageView.SelectedImage.Channels[channelsBox.SelectedIndex].stats[channelsBox.SelectedIndex].StackMin;
                }
            }
            else
            {
                ImageView.SelectedImage.StackThreshold(false);
            }
            App.viewer.UpdateImage();
        }

        /// When the user changes the sample number, the minimum and maximum values of the selected
        /// channel are updated
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void sampleBox_ValueChanged(object sender, EventArgs e)
        {
            if (initializing || Channels.Count == 0)
                return;
            if (channelsBox.SelectedIndex == -1)
                channelsBox.SelectedIndex = 0;
            minBox.Value = Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Min;
            maxBox.Value = Channels[channelsBox.SelectedIndex].range[(int)sampleBox.Value].Max;
            if (hist != null)
            {
                //hist.Statistics = Channels[channelsBox.SelectedIndex].statistics;
                hist.UpdateChannel(SelectedChannel);
                hist.Invalidate();
            }
            App.viewer.UpdateView();
        }

        /// When the value of the excitation box is changed, the excitation value of the selected
        /// channel is changed to the value of the excitation box.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        private void excBox_ValueChanged(object sender, EventArgs e)
        {
            SelectedChannel.Excitation = (int)excBox.Value;
        }

        /// When the value of the emission box is changed, the emission of the selected channel is
        /// changed to the value of the emission box
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        private void emisBox_ValueChanged(object sender, EventArgs e)
        {
            SelectedChannel.Emission = (int)emisBox.Value;
        }

        /// When the text in the fluorBox changes, the SelectedChannel's fluor is set to the text in the
        /// fluorBox
        /// 
        /// @param sender
        /// @param EventArgs System.EventArgs
        private void fluorBox_TextChanged(object sender, EventArgs e)
        {
            SelectedChannel.Fluor = fluorBox.Text;
        }
    }
}
