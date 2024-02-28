namespace BioCore
{
    public partial class NodeView : Form
    {
        public class Node
        {
            public TreeNode node;
            /* An enum. */
            public enum DataType
            {
                image,
                buf,
                roi,
                text
            }
            private DataType type;
            public DataType Type
            {
                get { return type; }
                set { type = value; }
            }
            private object obj;
            public object Object
            {
                get { return obj; }
                set { obj = value; }
            }
            /* A constructor. */
            public Node(object data, DataType typ)
            {
                type = typ;
                obj = data;
                node = new TreeNode();
                node.Tag = this;
                node.Text = obj.ToString();
                node.ForeColor = Color.White;
            }
            public string Text
            {
                get { return node.Text; }
                set { node.Text = value; }
            }
        }
        /* A constructor. */
        public NodeView(string[] args)
        {
            InitializeComponent();
            //Let's make sure the directories we need for startup exist.
            Console.WriteLine("Reading start up folders.");
            string st = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
            System.IO.Directory.CreateDirectory(st + "/Scripts");
            System.IO.Directory.CreateDirectory(st + "/Functions");
            System.IO.Directory.CreateDirectory(st + "/Tools");
            Init();
            InitNodes();
            App.nodeView = this;
            foreach (string s in args)
            {
                BioImage.OpenAsync(s,true,true,true,0).Wait();
            }
            Show();
        }

        private static void Init()
        {
            if(!App.Initialized)
            App.Initialize();
            Filters.Init();
        }
       /// > UpdateOverlay() is a function that updates the overlay
        public void UpdateOverlay()
        {
            if (App.viewer != null)
                App.viewer.UpdateOverlay();
        }
        /// It creates a treeview with a root node called "BioImages" and then adds a child node for
        /// each BioImage in the Images.images list. Each child node has two child nodes, one for the
        /// planes and one for the ROIs
        public void InitNodes()
        {
            treeView.Nodes.Clear();
            TreeNode images = new TreeNode();
            images.Text = "BioImages";
            images.ForeColor = Color.White;
            foreach (BioImage item in Images.images)
            {
                //TreeNode node = new TreeNode();
                Node tree = new Node(item, Node.DataType.image);

                Node implanes = new Node(item, Node.DataType.text);
                implanes.Text = "Planes";

                foreach (BufferInfo buf in item.Buffers)
                {
                    Node plane = new Node(buf, Node.DataType.buf);
                    plane.Text = buf.ID + ", " + buf.Coordinate.ToString();

                    implanes.node.Nodes.Add(plane.node);
                }
                tree.node.Nodes.Add(implanes.node);

                Node rois = new Node(item, Node.DataType.text);
                rois.Text = "ROI";

                foreach (ROI an in item.Annotations)
                {
                    Node roi = new Node(an, Node.DataType.roi);
                    rois.node.Nodes.Add(roi.node);
                }
                tree.node.Nodes.Add(rois.node);
                images.Nodes.Add(tree.node);
            }
            treeView.Nodes.Add(images);
        }

        /// If the number of images in the treeview is not the same as the number of images in the
        /// Images class, we refresh the whole treeview. If the number of ROIs in the treeview is not
        /// the same as the number of ROIs in the image, we refresh the ROIs
        /// 
        /// @return The return type is void.
        public void UpdateNodes()
        {
            if (Images.images.Count != treeView.Nodes[0].Nodes.Count)
            {
                //If image count is not same as node count we refresh whole tree.
                InitNodes();
                return;
            }
            TreeNode images = treeView.Nodes[0];
            foreach (TreeNode item in images.Nodes)
            {
                //TreeNode node = new TreeNode();
                Node node = (Node)item.Tag;
                BioImage im = (BioImage)node.Object;

                TreeNode rois = node.node.Nodes[1];
                if (im.Annotations.Count != rois.Nodes.Count)
                {
                    //If ROI count is not same as node count we refresh annotations.
                    rois.Nodes.Clear();
                    foreach (ROI an in im.Annotations)
                    {
                        Node roi = new Node(an, Node.DataType.roi);
                        rois.Nodes.Add(roi.node);
                    }
                }
                else
                    for (int i = 0; i < im.Annotations.Count; i++)
                    {
                        TreeNode roi = rois.Nodes[i];
                        Node n = (Node)roi.Tag;
                        ROI an = (ROI)n.Object;
                        roi.Text = an.ToString();
                    }
            }
        }

        /// It closes the current form, exits the application, and exits the thread
        public void Exit()
        {
            this.Close();
            Application.Exit();
            Application.ExitThread();
        }

        /// When the main form is activated, update the nodes
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void MainForm_Activated(object sender, EventArgs e)
        {
            UpdateNodes();
        }

        /// The function opens a dialog box that allows the user to select one or more files to open
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The file name of the image that was opened.
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilesDialog.ShowDialog() != DialogResult.OK)
                return;
            foreach (string file in openFilesDialog.FileNames)
            {
                BioImage.Open(file);
            }
        }
        /// It's a function that refreshes the treeview
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void refreshToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            InitNodes();
        }

        /// This function creates a new instance of the TabsView class and displays it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabsView iv = new TabsView();
            iv.Show();
        }

        /// The function is called after a node is selected in the tree view
        /// 
        /// @param sender The object that raised the event.
        /// @param TreeViewEventArgs 
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        /// It opens the script runner window
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void scriptRunnerToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            App.runner.WindowState = FormWindowState.Normal;
            App.runner.Show();
        }

        /// The function deletes the selected node from the treeview
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The node object.
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            Node node = (Node)treeView.SelectedNode.Tag;
            if (node == null)
                return;
            if (node.Type == Node.DataType.roi)
            {
                ROI an = (ROI)node.Object;
                Node nod = (Node)treeView.SelectedNode.Parent.Tag;
                BioImage im = (BioImage)nod.Object;
                im.Annotations.Remove(an);
            }
            if (node.Type == Node.DataType.image)
            {
                BioImage im = (BioImage)node.Object;
                Images.RemoveImage(im);
                im.Dispose();
            }
            UpdateNodes();
            UpdateOverlay();
        }

        /// The function is called when the user clicks on the "Set Text" menu item in the context menu
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The text value and the font.
        private void setTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node node = (Node)treeView.SelectedNode.Tag;
            if (node.Type == Node.DataType.roi)
            {
                ROI an = (ROI)node.Object;
                Node nod = (Node)treeView.SelectedNode.Parent.Tag;
                BioImage im = (BioImage)nod.Object;
                TextInput input = new TextInput(an.Text);
                if (input.ShowDialog() != DialogResult.OK)
                    return;
                an.Text = input.TextValue;
                an.font = input.font;
                an.strokeColor = input.color;
            }
            UpdateNodes();
            UpdateOverlay();
        }

        /// The function is called when the user clicks on the "Set ID" menu item in the context menu
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The return value is the DialogResult.OK.
        private void setIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node node = (Node)treeView.SelectedNode.Tag;
            if (node.Type == Node.DataType.roi)
            {
                ROI an = (ROI)node.Object;
                Node nod = (Node)treeView.SelectedNode.Parent.Tag;
                BioImage im = (BioImage)nod.Object;
                TextInput input = new TextInput(an.id);
                if (input.ShowDialog() != DialogResult.OK)
                    return;
                an.id = input.TextValue;
            }
            UpdateNodes();
            UpdateOverlay();
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

       /// When the user clicks on the "New Tabs View" menu item, a new TabsView form is created and
       /// shown
       /// 
       /// @param sender The object that raised the event.
       /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void newTabsViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabsView v = new TabsView();
            v.Show();
        }

        /// When a node is double clicked, the viewer will go to the image that the node is associated
        /// with
        /// 
        /// @param sender System.Windows.Forms.TreeView
        /// @param EventArgs 
        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            Node node = (Node)treeView.SelectedNode.Tag;
            if (node != null)
                if (node.Type == Node.DataType.buf)
                {
                    setIDToolStripMenuItem.Visible = false;
                    setTextToolStripMenuItem.Visible = false;
                    BufferInfo buf = (BufferInfo)node.Object;
                    App.viewer.SetCoordinate(buf.Coordinate.Z, buf.Coordinate.C, buf.Coordinate.T);
                    App.viewer.GoToImage();
                }
                else
                if (node.Type == Node.DataType.roi)
                {
                    setIDToolStripMenuItem.Visible = true;
                    setTextToolStripMenuItem.Visible = true;
                    ROI an = (ROI)node.Object;
                    string name = node.node.Parent.Parent.Text;
                    ImageView v = TabsView.SelectedViewer;
                    if (v != null)
                        v.SetCoordinate(an.coord.Z, an.coord.C, an.coord.T);
                }
        }
    }
}
