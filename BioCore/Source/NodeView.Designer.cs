namespace BioCore
{
    partial class NodeView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodeView));
            treeView = new TreeView();
            contextMenuStrip = new ContextMenuStrip(components);
            deleteToolStripMenuItem = new ToolStripMenuItem();
            setTextToolStripMenuItem = new ToolStripMenuItem();
            setIDToolStripMenuItem = new ToolStripMenuItem();
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newTabsViewToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            openFilesDialog = new OpenFileDialog();
            contextMenuStrip.SuspendLayout();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView.BackColor = Color.FromArgb(95, 122, 156);
            treeView.ContextMenuStrip = contextMenuStrip;
            treeView.Location = new Point(-1, 31);
            treeView.Margin = new Padding(4, 3, 4, 3);
            treeView.Name = "treeView";
            treeView.Size = new Size(270, 306);
            treeView.TabIndex = 2;
            treeView.DoubleClick += treeView_DoubleClick;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { deleteToolStripMenuItem, setTextToolStripMenuItem, setIDToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(137, 70);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(136, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // setTextToolStripMenuItem
            // 
            setTextToolStripMenuItem.Name = "setTextToolStripMenuItem";
            setTextToolStripMenuItem.Size = new Size(136, 22);
            setTextToolStripMenuItem.Text = "Set ROI Text";
            setTextToolStripMenuItem.Click += setTextToolStripMenuItem_Click;
            // 
            // setIDToolStripMenuItem
            // 
            setIDToolStripMenuItem.Name = "setIDToolStripMenuItem";
            setIDToolStripMenuItem.Size = new Size(136, 22);
            setIDToolStripMenuItem.Text = "Set ROI ID";
            setIDToolStripMenuItem.Click += setIDToolStripMenuItem_Click;
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(7, 2, 0, 2);
            menuStrip.Size = new Size(269, 24);
            menuStrip.TabIndex = 3;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newTabsViewToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newTabsViewToolStripMenuItem
            // 
            newTabsViewToolStripMenuItem.Name = "newTabsViewToolStripMenuItem";
            newTabsViewToolStripMenuItem.Size = new Size(152, 22);
            newTabsViewToolStripMenuItem.Text = "New Tabs View";
            newTabsViewToolStripMenuItem.Click += newTabsViewToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // openFilesDialog
            // 
            openFilesDialog.Multiselect = true;
            openFilesDialog.Title = "Open Images";
            // 
            // NodeView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(269, 337);
            Controls.Add(menuStrip);
            Controls.Add(treeView);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "NodeView";
            Text = "Bio - Node View";
            WindowState = FormWindowState.Minimized;
            Activated += MainForm_Activated;
            contextMenuStrip.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.OpenFileDialog openFilesDialog;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newTabsViewToolStripMenuItem;
    }
}