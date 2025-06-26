using AForge;
using BioCore;
using BioLib;
using System.Diagnostics;
namespace BioCore
{
    public partial class TabsView : Form
    {
        public static bool init = false;
        public Filter filters = null;
        public static System.Drawing.Graphics graphics = null;
        public List<ImageView> viewers = new List<ImageView>();
        public ImageView Viewer
        {
            get
            {
                if (tabControl.TabPages.Count == 0)
                    return null;
                if (tabControl.SelectedIndex == -1)
                    return null;
                if (tabControl.SelectedTab.Controls.Count == 0)
                    return null;
                return (ImageView)tabControl.SelectedTab.Controls[0];
            }
        }
        /* A property that returns the image that is currently selected in the tabsView. */
        public static BioImage SelectedImage
        {
            get
            {
                return App.tabsView.Image;
            }
        }
        /* A property that returns the ImageViewer object from the tab control. */
        public static ImageView SelectedViewer
        {
            get
            {
                return App.tabsView.ImageView;
            }
        }
        /* Initializing the component. */
        public TabsView(BioImage arg)
        {
            InitializeComponent();
            LoadProperties();
            ResizeView();
            Init();
        }
        /* Initializing the component and loading the properties. */
        public TabsView()
        {
            InitializeComponent();
            LoadProperties();
            ResizeView();
            Init();
        }
        /* A constructor for a class called TabsView. It is initializing the component and loading
        properties. It is also initializing the class. */
        public TabsView(string[] arg)
        {
            InitializeComponent();
            LoadProperties();
            Init();
            if (arg.Length == 0)
                return;
            else
            {
                for (int i = 0; i < arg.Length; i++)
                {
                    if (arg[i].EndsWith(".cs"))
                    {
                        App.runner.RunScriptFile(arg[0]);
                        return;
                    }
                    if (arg[i].EndsWith(".ijm"))
                    {
                        Fiji.RunMacroFiji(ImageView.SelectedImage, arg[i], "");
                        return;
                    }
                    else
                    {
                        AddTab(BioImage.OpenFile(arg[i]));
                    }
                }
            }
        }
        /// It creates a new tab, adds an image to it, and then resizes the window to fit the image
        /// 
        /// @param BioImage This is a class that contains the image data, and some other information
        /// about the image.
        /// 
        /// @return A TabPage object.
        public void AddTab(BioImage b)
        {
            if (b == null)
                return;
            if (b.filename.Contains('/') || b.filename.Contains('\\'))
                b.filename = Path.GetFileName(b.filename);
            TabPage t = new TabPage(b.filename);
            ImageView v = new ImageView(b);
            v.Dock = DockStyle.Fill;
            t.Controls.Add(v);
            if (Width < b.SizeX || Height < b.SizeY)
            {
                Width = b.SizeX;
                Height = b.SizeY + 190;
            }
            tabControl.TabPages.Add(t);
            tabControl.Dock = DockStyle.Fill;
            tabControl.SelectedIndex = tabControl.TabCount - 1;
            viewers.Add(v);
            ResizeView();
        }

        /// This function removes a tab from the tab control
        /// 
        /// @param tabName The name of the tab to remove.
        public void RemoveTab(string tabName)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                var item = tabControl.TabPages[i];
                string name = System.IO.Path.GetFileName(item.Name);
                if (name == tabName)
                {
                    ImageView iv = viewers[i];
                    for (int v = 0; v < iv.Images.Count; v++)
                    {
                        Images.RemoveImage(iv.Images[v]);
                    }
                    tabControl.TabPages.Remove(item);
                    viewers.RemoveAt(i);
                    App.nodeView.UpdateNodes();
                    return;
                }
                i++;
            }
        }

        public void SetTab(string tabName)
        {
            tabControl.SelectTab(tabName);
        }
        public void SetTab(int i)
        {
            tabControl.SelectedIndex = i;
        }

        public void RenameTab(string tabName, string text)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                TabPage t = tabControl.TabPages[i];
                if (t.Text == tabName)
                {
                    t.Text = text;
                    return;
                }
            }
        }

        /// The function is called Init(). It initializes the filters variable to a new instance of the
        /// Filter class. It also sets the init variable to true
        private void Init()
        {
            App.tabsView = this;
            Plugins.Initialize();
            filters = new Filter();
            string a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (char c in a)
            {
                ToolStripMenuItem me = new ToolStripMenuItem(c.ToString());
                foreach (Fiji.Macro.Command command in Fiji.Macro.Commands.Values)
                {
                    if (command.Name.StartsWith(c))
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem(command.Name.ToString());
                        menuItem.Click += MenuItem_Click;
                        me.DropDownItems.Add(menuItem);
                    }
                }
                commandToolStripMenuItem.DropDownItems.Add(me);
            }
            foreach (Scripting.Script s in Scripting.Scripts.Values)
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(s.name);
                mi.Click += Mi_Click;
                runToolStripMenuItem1.DropDownItems.Add(mi);
            }
            foreach (Fiji.Macro.Command c in Fiji.Macros)
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(c.Name);
                mi.Click += Mi_Click;
                runToolStripMenuItem1.DropDownItems.Add(mi);
            }
            init = true;
        }
        private void Mi_Click(object? sender, EventArgs e)
        {
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            if (m.Text.EndsWith(".ijm") || m.Text.EndsWith(".txt") && !m.Text.EndsWith(".cs"))
            {
                string ma = File.ReadAllText(m.Text);
                Fiji.RunOnImage(ImageView.SelectedImage,ma, BioConsole.headless, BioConsole.onTab, BioConsole.useBioformats, BioConsole.newTab);
            }
            else
                Scripting.RunByName(m.Text);
        }
        public ImageView GetViewer(int i)
        {
            return viewers[i];
        }
        public ImageView? GetViewer(string name)
        {
            for (int v = 0; v < viewers.Count; v++)
            {
                for (int i = 0; i < viewers.Count; i++)
                {
                    if (viewers[v].Images[i].Filename == name)
                        return viewers[v];
                }
            }
            return null;
        }
        public int GetViewerCount()
        {
            return viewers.Count;
        }
        public void RemoveViewer(ImageView v)
        {
            viewers.Remove(v);
        }
        public void AddViewer(ImageView v)
        {
            viewers.Add(v);
        }
        private void MenuItem_Click(object? sender, EventArgs e)
        {
            if (ImageView.SelectedImage == null) return;
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            Fiji.RunOnImage(ImageView.SelectedImage,"run(\"" + m.Text + "\");", BioConsole.headless, BioConsole.onTab, BioConsole.useBioformats, BioConsole.newTab);
            ToolStripMenuItem mi = new ToolStripMenuItem(m.Text);
            mi.Click += MenuItem_Click;
            bool con = false;
            foreach (ToolStripMenuItem item in recentToolStripMenuItem.DropDownItems)
            {
                if (item.Text == m.Text)
                    con = true;
            }
            if (!con)
                recentToolStripMenuItem.DropDownItems.Add(mi);
        }
        /// If the image is pyramidal, add 42 to the width and 206 to the height. Otherwise, add 20 to
        /// the width and 180 to the height
        /// 
        /// @return The size of the image.
        public void ResizeView()
        {
            if (Image == null)
                return;
            System.Drawing.Size s;
            if (SelectedImage.isPyramidal)
                s = new System.Drawing.Size(ImageView.SelectedImage.Resolutions[ImageView.Level].SizeX + 42, ImageView.SelectedImage.Resolutions[ImageView.Level].SizeY + 206);
            else
                s = new System.Drawing.Size(ImageView.SelectedImage.SizeX + 20, ImageView.SelectedImage.SizeY + 180);
            if (s.Width > Screen.PrimaryScreen.Bounds.Width || s.Height > Screen.PrimaryScreen.Bounds.Height)
            {
                this.WindowState = FormWindowState.Maximized;
                Viewer.GoToImage();
            }
            else
            {
                Size = s;
                Viewer.GoToImage();
            }
        }

        /* Setting the image property of the viewer. */
        public BioImage Image
        {
            get
            {
                if (Viewer == null)
                    return null;
                if (ImageView.SelectedImage == null)
                    return null;
                return ImageView.SelectedImage;
            }
            set
            {
                App.viewer.Images[App.viewer.SelectedIndex] = value;
            }
        }

        /* A property of the ImageView class. */
        public ImageView ImageView
        {
            get
            {
                return Viewer;
            }
        }

        /* A property that returns the number of tabs in the tab control. */
        public int TabCount
        {
            get
            {
                return tabControl.TabPages.Count;
            }
        }

        /// It creates a new process, sets the file name to the current executable, and sets the
        /// arguments to the file name
        /// 
        /// @param file The file to open
        public void OpenInNewProcess(string file)
        {
            Process p = new Process();
            p.StartInfo.FileName = Application.ExecutablePath;
            p.StartInfo.Arguments = file;
            p.Start();
        }

        /// It opens a file dialog, and if the user selects a file, it opens the file and adds it to the
        /// tab control
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The return value is the result of the ShowDialog method of the openFilesDialog
        /// object.
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            int img = Images.images.Count;
            foreach (string item in openFilesDialog.FileNames)
            {
                BioImage im = BioImage.OpenFile(item, 0, true, true);
                AddTab(im);
                if (!App.recent.Contains(im.ID))
                    App.recent.Add(im.ID);
            }
            foreach (string item in App.recent)
            {
                openRecentToolStripMenuItem.DropDownItems.Add(item, null, ItemClicked);
            }
        }

        /// It saves the recent list to the settings file
        private void SaveProperties()
        {
            string s = "";
            for (int i = 0; i < App.recent.Count; i++)
            {
                s += "$" + App.recent[i];
            }
            Properties.Settings.Default["Recent"] = s;
            Properties.Settings.Default.Save();
        }

        /// It loads the recent files from the settings file and adds them to the recent files list
        private void LoadProperties()
        {
            App.recent.Clear();
            string s = (string)Properties.Settings.Default["Recent"];
            string[] sts = s.Split('$');
            foreach (string item in sts)
            {
                if (item != "")
                    App.recent.Add(item);
            }
        }

        /// This function removes the selected image from the list of images
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Viewer != null)
            {
                Images.RemoveImage(SelectedImage);
            }
        }

        /// If the user selects a file to save to, then save the image to that file
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The file name of the image.
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Image == null)
                return;
            saveTiffFileDialog.FileName = Path.GetFileNameWithoutExtension(Image.Filename);
            if (saveTiffFileDialog.ShowDialog() != DialogResult.OK)
                return;
            string[] sts = new string[1];
            sts[0] = ImageView.SelectedImage.ID;
            BioImage.SaveSeries(sts, saveTiffFileDialog.FileName);
        }

        /// If the user presses the 'S' key while holding down the 'Ctrl' key, then the 'Save' menu item
        /// is clicked
        /// 
        /// @param sender The object that raised the event.
        /// @param PreviewKeyDownEventArgs The event data.
        private void ImageViewer_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                saveToolStripMenuItem.PerformClick();
            }
            else
            if (e.KeyCode == Keys.O && e.Control)
            {
                openToolStripMenuItem.PerformClick();
            }
        }

        /// It shows the toolbox when the user clicks on the toolbox menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void toolboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.tools.Show();
        }

        /// This function is called when the user clicks on the "Export ROIs to CSV" menu item. It opens
        /// a file dialog and allows the user to select a file to save the ROIs to. If the user selects
        /// a file, the function calls the BioImage.ExportROIsCSV function to save the ROIs to the file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the file that was selected.
        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveCSVFileDialog.ShowDialog() != DialogResult.OK)
                return;
            BioImage.ExportROIsCSV(saveCSVFileDialog.FileName, ImageView.SelectedImage.Annotations);
        }

        /// This function opens a file dialog to select a CSV file, then adds the ROIs in the CSV file
        /// to the current image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The return value is a list of ROIs.
        private void importCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openCSVFileDialog.ShowDialog() != DialogResult.OK)
                return;
            ImageView.SelectedImage.Annotations.AddRange(BioImage.ImportROIsCSV(openCSVFileDialog.FileName));
        }

        /// This function exports the ROIs of all the images in a folder to a CSV file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The return value is the path of the selected folder.
        private void exportROIsOfFolderOfImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            saveOMEFileDialog.InitialDirectory = folderBrowserDialog.SelectedPath;
            if (saveCSVFileDialog.ShowDialog() != DialogResult.OK)
                return;
            string f = Path.GetFileName(saveCSVFileDialog.FileName);

            BioImage.ExportROIFolder(folderBrowserDialog.SelectedPath, f);
        }

        /// It opens the ROI Manager
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void rOIManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.manager.Show();
        }

        /// When the ImageViewer is activated, the tabsView is set to the current ImageViewer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void ImageViewer_Activated(object sender, EventArgs e)
        {
            App.tabsView = this;
            //App.Image = SelectedImage;
        }

        /// If the user clicks on the Channels Tool menu item, then if the Channels Tool window is not
        /// open, open it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The image that is selected in the ImageView.
        private void channelsToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ImageView.SelectedImage == null)
                return;
            if (App.channelsTool == null)
                App.channelsTool = new ChannelsTool(App.Channels);
            App.channelsTool.Show();
        }

        /// This function is called when the user clicks on the RGB menu item. It sets the Viewer.Mode
        /// to ImageView.ViewMode.RGBImage
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The image is being returned.
        private void rGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Viewer == null)
                return;
            Viewer.Mode = ImageView.ViewMode.RGBImage;
            filteredToolStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            emissionToolStripMenuItem.Checked = false;
            Viewer.UpdateStatus();
        }

        /// This function is called when the user clicks on the "Filtered" menu item in the "View" menu
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The image is being returned.
        private void filteredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Viewer == null)
                return;
            Viewer.Mode = ImageView.ViewMode.Filtered;
            rGBToolStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            emissionToolStripMenuItem.Checked = false;
            Viewer.UpdateStatus();
        }

        /// This function is called when the user clicks on the "Raw" menu item in the "View" menu
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The image is being returned.
        private void rawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Viewer == null)
                return;
            Viewer.Mode = ImageView.ViewMode.Raw;
            rGBToolStripMenuItem.Checked = false;
            filteredToolStripMenuItem.Checked = false;
            emissionToolStripMenuItem.Checked = false;
            Viewer.UpdateStatus();
        }

        /// This function is called when the user clicks on the "Auto Threshold" menu item. It calls the
        /// BioImage.AutoThreshold function to automatically threshold the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The image is being returned.
        private void autoThresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Viewer == null)
                return;
            BioImage.AutoThreshold(ImageView.SelectedImage, true);
            if (ImageView.SelectedImage.bitsPerPixel > 8)
                ImageView.SelectedImage.StackThreshold(true);
            else
                ImageView.SelectedImage.StackThreshold(false);
        }
        /// This function is called when the user clicks on the menu item to change the view mode. It updates
        /// the menu item to reflect the current view mode.
        /// 
        /// @param v The view mode to be updated

        public void UpdateViewMode(ImageView.ViewMode v)
        {
            if (v == ImageView.ViewMode.RGBImage)
                rGBToolStripMenuItem.Checked = true;
            if (v == ImageView.ViewMode.Filtered)
                filteredToolStripMenuItem.Checked = true;
            if (v == ImageView.ViewMode.Raw)
                rawToolStripMenuItem.Checked = true;
        }

        /// It opens the script runner window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void scriptRunnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.runner.WindowState = FormWindowState.Normal;
            App.runner.Show();
        }

        /// It opens the stackTools form when the user clicks on the stackToolsToolStripMenuItem
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void stackToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.stackTools.Show();
        }

        /// It opens the script recorder window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void scriptRecorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Recorder.recorder.WindowState = FormWindowState.Normal;
            Recorder.recorder.Show();
        }

        /// The function saves the current image as an OME-TIFF file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The image is being returned.
        private void saveOMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Image == null)
                return;
            saveOMEFileDialog.FileName = Path.GetFileNameWithoutExtension(ImageView.SelectedImage.Filename);
            if (saveOMEFileDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string file in saveOMEFileDialog.FileNames)
            {
                BioImage.SaveOME(file, Image.ID);
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        /// It opens a new window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void setToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetTool tool = new SetTool();
            tool.Show();
        }

        /// Here we update the tabs based on images in the table.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs This is the event arguments.
        private void timer_Tick(object sender, EventArgs e)
        {
            //Here we update the tabs based on images in the table.

        }

        /// It opens a new window called "filters" when the user clicks on the "Filters" button in the
        /// menu bar.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void filtersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            filters = new Filter();
            filters.Show();
        }

        /// It converts the image to 8 bit, then updates the GUI and the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data,
        /// and provides a value to use for events that do not include event data.
        private void bit8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image.To8Bit();
            Viewer.InitGUI();
            Viewer.UpdateImages();
        }

        /// When the user clicks on the bit16ToolStripMenuItem, the image is converted to 16 bit, the GUI
        /// is updated, and the image is updated.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void bit16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image.To16Bit();
            Viewer.InitGUI();
            Viewer.UpdateImages();
        }

        /// Converts the image to 24 bit color.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void to24BitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image.To24Bit();
            //We update the viewer gui since there are less planes now.
            Viewer.InitGUI();
            Viewer.UpdateImages();
        }

        /// It converts the image to 48 bit color
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void to48BitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image.To48Bit();
            //We update the viewer gui since there are less planes now.
            Viewer.InitGUI();
            Viewer.UpdateImages();
        }

        /// It converts the image to 32 bit color
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void to32BitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image.To32Bit();
            Viewer.InitGUI();
            Viewer.UpdateImages();
        }

        /// If the user clicks on the "To Window" menu item, then if there is a selected image, create a
        /// new ImageWindow and show it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The image that is selected in the tab control.
        private void toWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == -1)
                return;
            BioImage im = ImageView.SelectedImage;
            ImageWindow vi = new ImageWindow(im);
            vi.Show();
        }

        /// We update the view status based on tab
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The selected index of the tab control.
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == -1)
                return;
            //We update the view status based on tab.
            if (Viewer.Mode == ImageView.ViewMode.Raw)
                rawToolStripMenuItem.Checked = true;
            else
                rawToolStripMenuItem.Checked = false;
            if (Viewer.Mode == ImageView.ViewMode.Filtered)
                filteredToolStripMenuItem.Checked = true;
            else
                filteredToolStripMenuItem.Checked = false;
            if (Viewer.Mode == ImageView.ViewMode.RGBImage)
                rGBToolStripMenuItem.Checked = true;
            else
                rGBToolStripMenuItem.Checked = false;
            //We update the active Viewer.
            App.viewer = Viewer;
            ImageView.SelectedIndex = 0;
        }

        /// It removes the tab from the tab control, disposes the imageview, removes the images from the
        /// image list, and disposes the images
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        /// 
        /// @return The ImageView object is being returned.
        private void closeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == -1)
                return;
            ImageView v = (ImageView)tabControl.SelectedTab.Controls[0];
            tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
            v.Dispose();
            foreach (BioImage item in v.Images)
            {
                Images.RemoveImage(item);
                item.Dispose();
            }
            GC.Collect();
        }

        /// This function opens the OME tool.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void openOMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            int img = Images.images.Count;
            foreach (string item in openFilesDialog.FileNames)
            {
                BioImage im = BioImage.OpenOME(item, true);
                AddTab(im);
                if (!App.recent.Contains(im.ID))
                    App.recent.Add(im.ID);
            }
            foreach (string item in App.recent)
            {
                openRecentToolStripMenuItem.DropDownItems.Add(item, null, ItemClicked);
            }
        }

        /// It saves all the images in the list of images
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> sts = new List<string>();
            foreach (BioImage item in Images.images)
            {
                BioImage.SaveFile(Image.ID, Image.ID);
            }
        }

        /// When the user clicks on the "About" menu item, a new About form is created and shown
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        /// It opens a new window called "runner" when the user clicks on the "Script Runner" menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void scriptRunnerToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            App.runner.Show();
        }

        /// It opens a new window called "recorder" when the user clicks on the "Script Recorder" menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void scriptRecorderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            App.recorder.Show();
        }

        /// This function opens an OME file and displays it in a new tab
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The return value is a string.
        private void openOMEToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string sts in openFilesDialog.FileNames)
            {
                BioImage im = BioImage.OpenOME(sts, true);
                if (im == null)
                    return;
                AddTab(im);
            }
        }

        /// It opens a new instance of the application
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event data.
        private void newTabViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath);
        }

        /// It shows the nodeView form
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void nodeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.nodeView.Show();
            App.nodeView.ShowInTaskbar = true;
        }
        /// The saveToolStripMenuItem1_Click function is called when the user clicks the "Save" button
        /// in the menu bar. If the user selects a file name and location, the BioImage.Save function is
        /// called to save the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveTiffFileDialog.ShowDialog() == DialogResult.OK)
                BioImage.SaveFile(ImageView.SelectedImage.ID, saveTiffFileDialog.FileName);
        }

        /// If the user clicks the "Save OME" menu item, then show the save file dialog and if the user
        /// clicks "OK", then save the image to the file name the user specified
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void saveOMEToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveOMEFileDialog.ShowDialog() == DialogResult.OK)
                BioImage.SaveFile(ImageView.SelectedImage.ID, saveOMEFileDialog.FileName);
        }
        /// It clears the dropdown menu, then adds each item in the recent list to the dropdown menu.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void openRecentToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            openRecentToolStripMenuItem.DropDownItems.Clear();
            foreach (string item in App.recent)
            {
                openRecentToolStripMenuItem.DropDownItems.Add(item, null, ItemClicked);
            }
        }
        /// If the file is not an OME file, open the file as a series. If it is an OME file, open the
        /// file as a series and ask the user if they want to open the series in the same tab or in a
        /// new tab
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void ItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem ts = (ToolStripMenuItem)sender;
            if (!BioImage.isOME(ts.Text))
            {
                BioImage[] bs = BioImage.OpenSeries(ts.Text, false);
                for (int i = 0; i < bs.Length; i++)
                {
                    if (i == 0)
                        AddTab(bs[i]);
                    else
                        Viewer.AddImage(bs[i]);
                }
            }
            else
            {
                OpenInTab tb = new OpenInTab();
                if (tb.ShowDialog() == DialogResult.Yes)
                    BioImage.OpenOMESeries(ts.Text, true, true);
                else
                    BioImage.OpenOMESeries(ts.Text, false, true);
            }
            App.viewer.GoToImage();
            SaveProperties();
        }
        /// The function is called when a key is pressed on the keyboard
        /// 
        /// @param sender The object that raised the event.
        /// @param KeyEventArgs 
        /// 
        /// @return The return value is the value of the last expression evaluated in the function.
        private void TabsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (Viewer == null)
                return;
            Viewer.ImageView_KeyDown(sender, e);
            Viewer.UpdateStatus();
            Viewer.UpdateView();
        }

        /// It creates a copy of the current image, and then adds a new tab with the copy
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BioImage im = Image.Copy();
            AddTab(im);
        }

        /// It opens a new window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void saveSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.seriesTool.Show();
        }

        /// When the form is closing, save the properties and close the node view
        /// 
        /// @param sender The object that raised the event.
        /// @param FormClosingEventArgs 
        private void TabsView_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveProperties();
            App.nodeView.Close();
            Application.Exit();
        }

        /// If the user selects a file, open it and add it to the viewer
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The return value is a string array.
        private void addImagesToTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            for (int i = 0; i < openFilesDialog.FileNames.Length; i++)
            {
                if (i == 0 && tabControl.TabPages.Count == 0)
                {
                    BioImage[] bms = BioImage.OpenOMESeries(openFilesDialog.FileNames[0], true, true);
                    AddTab(bms[0]);
                    for (int a = 1; a < bms.Length; a++)
                    {
                        App.viewer.AddImage(bms[a]);
                    }
                    App.viewer.GoToImage();
                    return;
                }
                else
                {
                    BioImage[] bms = BioImage.OpenOMESeries(openFilesDialog.FileNames[0], false, true);
                    for (int a = 0; a < bms.Length; a++)
                    {
                        App.viewer.AddImage(bms[a]);
                    }
                    App.viewer.GoToImage();
                    return;
                }
            }

        }

        /// If the user selects a file, open it and add it to the viewer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The return value is a string.
        private void addImagesOMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            for (int i = 0; i < openFilesDialog.FileNames.Length; i++)
            {
                if (i == 0 && tabControl.TabPages.Count == 0)
                {
                    AddTab(BioImage.OpenOME(openFilesDialog.FileNames[0], false));
                }
                else
                    App.viewer.AddImage(BioImage.OpenOME(openFilesDialog.FileNames[i], false));
            }
            App.viewer.GoToImage();
        }

        /// It opens a dialog box to select a file, then opens the file and adds it to the viewer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The BioImage[] bs is being returned.
        private async void openSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (var file in openFilesDialog.FileNames)
            {
                int c = BioImage.GetSeriesCount(file);
                for (int i = 0; i < c; i++)
                {
                    await BioImage.OpenAsync(file, true, true, true, c);
                }
            }
        }

        /// If the user selects a file to save to, and the images are not 8 or 16 bit, ask the user if
        /// they want to convert them to 8 or 16 bit, and if they do, convert them and save them
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        /// 
        /// @return The return value is a string array.
        private void saveTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveOMEFileDialog.ShowDialog() != DialogResult.OK)
                return;
            bool convert = false;
            foreach (BioImage b in ImageView.Images)
            {
                if (b.isRGB)
                {
                    convert = true;
                    break;
                }
            }
            if (convert)
            {
                string mes;
                if (ImageView.SelectedImage.bitsPerPixel > 8)
                    mes = "Saving Series as OME only supports 8 bit & 16 bit images. Convert 16 bit?";
                else
                    mes = "Saving Series as OME only supports 8 bit & 16 bit images. Convert 8 bit?";
                if (MessageBox.Show(this, mes, "Convert to supported format?", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    foreach (BioImage b in ImageView.Images)
                    {
                        if (b.bitsPerPixel > 8)
                            b.To16Bit();
                        else
                            b.To8Bit();
                    }
                }
                else
                    return;
            }

            BioImage[] sts = new BioImage[App.viewer.Images.Count];
            for (int i = 0; i < sts.Length; i++)
            {
                sts[i] = Images.GetImage(App.viewer.Images[i].ID);
            }
            BioImage.SaveOMESeries(sts, saveOMEFileDialog.FileName, Properties.Settings.Default.Planes);
        }

        /// It saves the current image series to a single multi-page TIFF file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The file name of the file that was selected.
        private void saveTabTiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveTiffFileDialog.ShowDialog() != DialogResult.OK)
                return;
            string[] sts = new string[App.viewer.Images.Count];
            for (int i = 0; i < sts.Length; i++)
            {
                sts[i] = App.viewer.Images[i].Filename;
            }
            BioImage.SaveSeries(sts, saveTiffFileDialog.FileName);
        }

        /// It opens a series of images and adds them to the viewer
        /// 
        /// @param sender
        /// @param EventArgs 
        /// 
        /// @return The return type is void.
        private void openSeriesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            BioImage[] bms = null;
            foreach (string item in openFilesDialog.FileNames)
            {
                bms = BioImage.OpenSeries(openFilesDialog.FileName, true);
                for (int i = 0; i < bms.Length; i++)
                {
                    if (i == 0)
                    {
                        AddTab(bms[i]);
                    }
                    else
                        App.viewer.AddImage(bms[i]);
                }
            }
            App.viewer.GoToImage();
        }

        /// If the viewer is not null, then go to the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void TabsView_Load(object sender, EventArgs e)
        {
            if (App.viewer != null)
                App.viewer.GoToImage();

            Function.Initialize();
        }

        /// It clears the recent files list.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void clearRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Recent = "";
            Properties.Settings.Default.Save();
            openRecentToolStripMenuItem.DropDownItems.Clear();
            App.recent.Clear();
        }

        /// When the user clicks on a menu item in the drop down menu, the image is rotated by the amount
        /// specified in the menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param ToolStripItemClickedEventArgs The event arguments for the event.
        /// 
        /// @return The image is being rotated and flipped.
        private void rotateToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (ImageView.SelectedImage == null)
                return;
            string st = e.ClickedItem.Text;
            AForge.RotateFlipType rot = (AForge.RotateFlipType)Enum.Parse(typeof(AForge.RotateFlipType), st);
            ImageView.SelectedImage.RotateFlip(rot);
            ImageView.UpdateImage();
            ImageView.UpdateView();
        }

        /// If the dropdown menu has no items, add the items from the RotateFlipType enum
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The names of the enumeration values.
        private void rotateToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (rotateToolStripMenuItem.DropDownItems.Count > 0)
                return;
            string[] sts = Enum.GetNames(typeof(AForge.RotateFlipType));
            foreach (string item in sts)
            {
                rotateToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        /// If the Viewer is not null, set the Viewer's mode to Emission, uncheck all the other modes, check the
        /// Emission mode, and update the Viewer's status.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The image is being returned.
        private void emissionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Viewer == null)
                return;
            Viewer.Mode = ImageView.ViewMode.Emission;
            rGBToolStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            filteredToolStripMenuItem.Checked = false;
            emissionToolStripMenuItem.Checked = true;
            Viewer.UpdateStatus();
        }

        /// It opens the console window when the user clicks on the console menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.console.Show();
        }

        /// It loops through all the buffers in the selected image, and calls the `SwitchRedBlue`
        /// function on each buffer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void switchRedBlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Bitmap bf in ImageView.SelectedImage.Buffers)
            {
                bf.SwitchRedBlue();
            }
            ImageView.UpdateImages();
        }
        /// This function creates a new function form and shows it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void createFunctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FunctionForm f = new FunctionForm(new Function());
            f.Show();
        }
        /// When a user clicks on a dropdown item, it will call the function that is associated with
        /// that item
        /// 
        /// @param sender The object that triggered the event.
        /// @param EventArgs The event arguments.
        private void DropDownItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem ts = (ToolStripMenuItem)sender;
            Function.Functions[ts.Text].PerformFunction(true);
        }
        /// It clears the dropdown menu, then adds a new item for each function in the Function.Functions
        /// dictionary
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void runToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            runToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in Function.Functions)
            {
                runToolStripMenuItem.DropDownItems.Add(item.Value.Name, null, DropDownItemClicked);
            }
        }

        /// It reloads the image in the picturebox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageView.SelectedImage.Update();
        }

        /// It opens an XML file, and displays it in a new window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void xMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XMLView v = new XMLView(BioImage.OpenXML(ImageView.SelectedImage.file));
            v.Show();
        }
        /// If the user clicks on the "Hardware Acceleration" menu item, then the viewer will use
        /// hardware acceleration if the menu item is checked, and the viewer will not use hardware
        /// acceleration if the menu item is not checked
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Viewer.HardwareAcceleration = dToolStripMenuItem.Checked;
            Properties.Settings.Default.HardwareAcceleration = dToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
            Viewer.UpdateView();
        }

        /// It opens a file dialog, and if the user selects a bunch of images, it creates a new BioImage
        /// object that is a stack of the images, and adds a new tab to the main form with the new
        /// BioImage object
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return A BioImage object.
        private void imagesToStackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Images to Stack";
            fd.Multiselect = true;
            if (fd.ShowDialog() != DialogResult.OK)
                return;
            BioImage b = BioImage.ImagesToStack(fd.FileNames, true);
            AddTab(b);
        }

        /// It creates a new instance of the View3D class, passing in the selected image from the
        /// ImageView class, and then shows the View3D form
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The selected image.
        private void _3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            View3D d = new View3D(ImageView.SelectedImage);
            d.Show();
        }

        /// This function opens a dialog box to select a file, then adds the ROI to the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return A list of annotations.
        private void importImageJROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openImageJDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string item in openImageJDialog.FileNames)
            {
                ImageView.SelectedImage.Annotations.Add(Fiji.RoiDecoder.open(item, ImageView.SelectedImage.PhysicalSizeX, ImageView.SelectedImage.PhysicalSizeY,ImageView.SelectedImage.StageSizeX,ImageView.SelectedImage.StageSizeY));
            }
            App.viewer.UpdateView();
        }

        /// It saves all ROI's in the current image to a file in the ImageJ ROI format
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The ROI object is being returned.
        private void exportImageJROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveImageJDialog.Title = "Set Filename for exported ROI's.";
            saveImageJDialog.FileName = ImageView.SelectedImage.Filename;
            if (saveImageJDialog.ShowDialog() != DialogResult.OK)
                return;
            int i = 1;
            foreach (ROI roi in ImageView.SelectedImage.Annotations)
            {
                string s = Path.GetDirectoryName(saveImageJDialog.FileName) + "//" + Path.GetFileNameWithoutExtension(saveImageJDialog.FileName) + "-" + i + ".roi";
                Fiji.RoiEncoder.save(ImageView.SelectedImage,roi, s);
                i++;
            }
            App.viewer.UpdateView();
        }

        private void autoFocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedImage == null) return;
            ZCT co = ImageView.GetCoordinate();
            int f = BioImage.FindFocus(SelectedImage, co.C, co.T);
            ZCT z = SelectedImage.Buffers[f].Coordinate;
            ImageView.SetCoordinate(z.Z, z.C, z.T);
        }

        private void savePyramidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveOMEFileDialog.Title = "Save Pyramidal";
            if (saveOMEFileDialog.ShowDialog() != DialogResult.OK)
                return;
            BioImage.SaveOMEPyramidal(App.viewer.Images.ToArray(), saveOMEFileDialog.FileName, NetVips.Enums.ForeignTiffCompression.Lzw, 0);
        }

        private void TabsView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Viewer == null) return;
            Viewer.ImageView_KeyPress(Viewer, e);
        }

        private void TabsView_KeyUp(object sender, KeyEventArgs e)
        {
            if (Viewer == null) return;
            Viewer.ImageView_KeyUp(Viewer, e);
        }

        private void importQuPathROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openQuPathDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string item in openQuPathDialog.FileNames)
            {
                ROI[] rois = QuPath.ReadROI(item, ImageView.SelectedImage);
                foreach (ROI r in rois)
                {
                    ImageView.SelectedImage.Annotations.Add(r);
                }
            }
        }

        private void exportQuPathROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveQuPathDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string item in saveQuPathDialog.FileNames)
            {
                QuPath.SaveROI(item, ImageView.SelectedImage);
            }
        }

        private void extractRegionPyramidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BioImage bm = SelectedImage.Copy(true);
            bm.ID = Images.GetImageName(bm.ID);
            Images.AddImage(bm);
        }

        private void openQuPathProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openQuPathProjDialog.ShowDialog() != DialogResult.OK)
                return;
            QuPath.Project pr = QuPath.OpenProject(openQuPathProjDialog.FileName);
            foreach (QuPath.Image p in pr.Images)
            {
                string s = p.ServerBuilder.Uri.Replace("file:/", "");
                App.tabsView.AddTab(BioImage.OpenFile(s));
            }
        }

        private void saveQuPathProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveNumPyDialog.ShowDialog() != DialogResult.OK)
                return;
            List<BioImage> bms = new List<BioImage>();
            foreach (var v in viewers)
            {
                foreach (var item in v.Images)
                {
                    bms.Add(item);
                }
            }
            QuPath.Project.SaveProject(saveNumPyDialog.FileName, new List<BioImage[]> { bms.ToArray() });
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextInput ti = new TextInput("Rename Image");
            if (ti.ShowDialog() != DialogResult.OK)
                return;
            RenameTab(ImageView.SelectedImage.Filename, ti.TextValue);
        }

        private void saveNumPyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveNumPyDialog.ShowDialog() != DialogResult.OK)
                return;
            NumPy.SaveNumPy(ImageView.SelectedImage, saveNumPyDialog.FileName);
        }
    }
}
