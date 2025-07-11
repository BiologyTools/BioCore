﻿
namespace BioCore
{
    partial class TabsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TabsView));
            openFilesDialog = new OpenFileDialog();
            saveOMEFileDialog = new SaveFileDialog();
            panel = new Panel();
            tabControl = new TabControl();
            tabContextMenuStrip = new ContextMenuStrip(components);
            closeToolStripMenuItem = new ToolStripMenuItem();
            reloadToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem1 = new ToolStripMenuItem();
            saveOMEToolStripMenuItem1 = new ToolStripMenuItem();
            toWindowToolStripMenuItem = new ToolStripMenuItem();
            folderBrowserDialog = new FolderBrowserDialog();
            saveCSVFileDialog = new SaveFileDialog();
            openCSVFileDialog = new OpenFileDialog();
            saveTiffFileDialog = new SaveFileDialog();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            openRecentToolStripMenuItem = new ToolStripMenuItem();
            openOMEToolStripMenuItem = new ToolStripMenuItem();
            openQuPathProjectToolStripMenuItem = new ToolStripMenuItem();
            openOMESeriesToolStripMenuItem = new ToolStripMenuItem();
            openSeriesToolStripMenuItem = new ToolStripMenuItem();
            sepToolStripMenuItem1 = new ToolStripSeparator();
            addImagesToTabToolStripMenuItem = new ToolStripMenuItem();
            addImagesOMEToolStripMenuItem = new ToolStripMenuItem();
            sepToolStripMenuItem2 = new ToolStripSeparator();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveNumPyToolStripMenuItem = new ToolStripMenuItem();
            saveOMEToolStripMenuItem = new ToolStripMenuItem();
            saveTabToolStripMenuItem = new ToolStripMenuItem();
            saveTabTiffToolStripMenuItem = new ToolStripMenuItem();
            saveSeriesToolStripMenuItem = new ToolStripMenuItem();
            savePyramidalToolStripMenuItem = new ToolStripMenuItem();
            saveQuPathProjectToolStripMenuItem = new ToolStripMenuItem();
            sepToolStripMenuItem3 = new ToolStripSeparator();
            imagesToStackToolStripMenuItem = new ToolStripMenuItem();
            newTabViewToolStripMenuItem = new ToolStripMenuItem();
            nodeViewToolStripMenuItem = new ToolStripMenuItem();
            clearRecentToolStripMenuItem = new ToolStripMenuItem();
            renameToolStripMenuItem = new ToolStripMenuItem();
            sizeModeToolStripMenuItem = new ToolStripMenuItem();
            rGBToolStripMenuItem = new ToolStripMenuItem();
            filteredToolStripMenuItem = new ToolStripMenuItem();
            rawToolStripMenuItem = new ToolStripMenuItem();
            emissionToolStripMenuItem = new ToolStripMenuItem();
            _3dToolStripMenuItem = new ToolStripMenuItem();
            sepToolStripMenuItem = new ToolStripSeparator();
            dToolStripMenuItem = new ToolStripMenuItem();
            xMLToolStripMenuItem = new ToolStripMenuItem();
            toolboxToolStripMenuItem = new ToolStripMenuItem();
            setToolToolStripMenuItem = new ToolStripMenuItem();
            rOIToolStripMenuItem = new ToolStripMenuItem();
            rOIManagerToolStripMenuItem = new ToolStripMenuItem();
            exportCSVToolStripMenuItem = new ToolStripMenuItem();
            importCSVToolStripMenuItem = new ToolStripMenuItem();
            exportROIsOfFolderOfImagesToolStripMenuItem = new ToolStripMenuItem();
            importImageJROIToolStripMenuItem = new ToolStripMenuItem();
            exportImageJROIToolStripMenuItem = new ToolStripMenuItem();
            importQuPathROIToolStripMenuItem = new ToolStripMenuItem();
            exportQuPathROIToolStripMenuItem = new ToolStripMenuItem();
            channelsToolToolStripMenuItem = new ToolStripMenuItem();
            autoThresholdToolStripMenuItem = new ToolStripMenuItem();
            channelsToolToolStripMenuItem1 = new ToolStripMenuItem();
            switchRedBlueToolStripMenuItem = new ToolStripMenuItem();
            stackToolsToolStripMenuItem = new ToolStripMenuItem();
            stackToolsToolStripMenuItem1 = new ToolStripMenuItem();
            duplicateToolStripMenuItem = new ToolStripMenuItem();
            rotateToolStripMenuItem = new ToolStripMenuItem();
            autoFocusToolStripMenuItem = new ToolStripMenuItem();
            extractRegionPyramidalToolStripMenuItem = new ToolStripMenuItem();
            menuStrip = new MenuStrip();
            formatToolStripMenuItem = new ToolStripMenuItem();
            bit8ToolStripMenuItem = new ToolStripMenuItem();
            bit16ToolStripMenuItem = new ToolStripMenuItem();
            to24BitToolStripMenuItem = new ToolStripMenuItem();
            to36BitToolStripMenuItem = new ToolStripMenuItem();
            to48BitToolStripMenuItem = new ToolStripMenuItem();
            filtersToolStripMenuItem = new ToolStripMenuItem();
            scriptToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem = new ToolStripMenuItem();
            createFunctionToolStripMenuItem = new ToolStripMenuItem();
            consoleToolStripMenuItem = new ToolStripMenuItem();
            scriptRunnerToolStripMenuItem = new ToolStripMenuItem();
            scriptRecorderToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            recentToolStripMenuItem = new ToolStripMenuItem();
            commandToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem1 = new ToolStripMenuItem();
            openImageJDialog = new OpenFileDialog();
            saveImageJDialog = new SaveFileDialog();
            saveQuPathDialog = new SaveFileDialog();
            openQuPathDialog = new OpenFileDialog();
            openQuPathProjDialog = new OpenFileDialog();
            saveNumPyDialog = new SaveFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            oMEROToolStripMenuItem = new ToolStripMenuItem();
            panel.SuspendLayout();
            tabContextMenuStrip.SuspendLayout();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // openFilesDialog
            // 
            openFilesDialog.Multiselect = true;
            openFilesDialog.Title = "Open Images";
            // 
            // saveOMEFileDialog
            // 
            saveOMEFileDialog.DefaultExt = "ome.tif";
            saveOMEFileDialog.Filter = "OME TIFF Files (*.ome.tif)|*.ome.tif|All files (*.*)|*.*";
            saveOMEFileDialog.SupportMultiDottedExtensions = true;
            saveOMEFileDialog.Title = "Save Image";
            // 
            // panel
            // 
            panel.BackColor = Color.FromArgb(95, 122, 156);
            panel.Controls.Add(tabControl);
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 24);
            panel.Margin = new Padding(4, 3, 4, 3);
            panel.Name = "panel";
            panel.Size = new Size(807, 5);
            panel.TabIndex = 1;
            // 
            // tabControl
            // 
            tabControl.ContextMenuStrip = tabContextMenuStrip;
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4, 3, 4, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(807, 5);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabContextMenuStrip
            // 
            tabContextMenuStrip.Items.AddRange(new ToolStripItem[] { closeToolStripMenuItem, reloadToolStripMenuItem, saveToolStripMenuItem1, saveOMEToolStripMenuItem1, toWindowToolStripMenuItem });
            tabContextMenuStrip.Name = "tabContextMenuStrip";
            tabContextMenuStrip.Size = new Size(165, 114);
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(164, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click_1;
            // 
            // reloadToolStripMenuItem
            // 
            reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            reloadToolStripMenuItem.Size = new Size(164, 22);
            reloadToolStripMenuItem.Text = "Reload";
            reloadToolStripMenuItem.Click += reloadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem1
            // 
            saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            saveToolStripMenuItem1.Size = new Size(164, 22);
            saveToolStripMenuItem1.Text = "Save";
            saveToolStripMenuItem1.Click += saveToolStripMenuItem1_Click;
            // 
            // saveOMEToolStripMenuItem1
            // 
            saveOMEToolStripMenuItem1.Name = "saveOMEToolStripMenuItem1";
            saveOMEToolStripMenuItem1.Size = new Size(164, 22);
            saveOMEToolStripMenuItem1.Text = "Save OME";
            saveOMEToolStripMenuItem1.Click += saveOMEToolStripMenuItem1_Click;
            // 
            // toWindowToolStripMenuItem
            // 
            toWindowToolStripMenuItem.Name = "toWindowToolStripMenuItem";
            toWindowToolStripMenuItem.Size = new Size(164, 22);
            toWindowToolStripMenuItem.Text = "Open as Window";
            toWindowToolStripMenuItem.Click += toWindowToolStripMenuItem_Click;
            // 
            // saveCSVFileDialog
            // 
            saveCSVFileDialog.DefaultExt = "csv";
            saveCSVFileDialog.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
            saveCSVFileDialog.Title = "Save ROIs to CSV";
            // 
            // openCSVFileDialog
            // 
            openCSVFileDialog.DefaultExt = "csv";
            openCSVFileDialog.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
            openCSVFileDialog.Title = "Import ROI from CSV";
            // 
            // saveTiffFileDialog
            // 
            saveTiffFileDialog.DefaultExt = "tif";
            saveTiffFileDialog.Filter = "TIFF Files (*.tif)|*.tif | PNG Files (*.png)|*.png | BMP Files (*.bmp)|*.bmp | JPG Files (*.jpg)|*.jpg | Gif Files (*.gif)|*.gif";
            saveTiffFileDialog.SupportMultiDottedExtensions = true;
            saveTiffFileDialog.Title = "Save Image";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, openRecentToolStripMenuItem, openOMEToolStripMenuItem, openQuPathProjectToolStripMenuItem, openOMESeriesToolStripMenuItem, openSeriesToolStripMenuItem, sepToolStripMenuItem1, addImagesToTabToolStripMenuItem, addImagesOMEToolStripMenuItem, sepToolStripMenuItem2, saveToolStripMenuItem, saveNumPyToolStripMenuItem, saveOMEToolStripMenuItem, saveTabToolStripMenuItem, saveTabTiffToolStripMenuItem, saveSeriesToolStripMenuItem, savePyramidalToolStripMenuItem, saveQuPathProjectToolStripMenuItem, sepToolStripMenuItem3, imagesToStackToolStripMenuItem, newTabViewToolStripMenuItem, nodeViewToolStripMenuItem, clearRecentToolStripMenuItem, renameToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(204, 22);
            openToolStripMenuItem.Text = "Open Images";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openRecentToolStripMenuItem
            // 
            openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
            openRecentToolStripMenuItem.Size = new Size(204, 22);
            openRecentToolStripMenuItem.Text = "Open Recent";
            openRecentToolStripMenuItem.DropDownOpening += openRecentToolStripMenuItem_DropDownOpening;
            // 
            // openOMEToolStripMenuItem
            // 
            openOMEToolStripMenuItem.Name = "openOMEToolStripMenuItem";
            openOMEToolStripMenuItem.Size = new Size(204, 22);
            openOMEToolStripMenuItem.Text = "Open OME Images";
            openOMEToolStripMenuItem.Click += openOMEToolStripMenuItem_Click_1;
            // 
            // openQuPathProjectToolStripMenuItem
            // 
            openQuPathProjectToolStripMenuItem.Name = "openQuPathProjectToolStripMenuItem";
            openQuPathProjectToolStripMenuItem.Size = new Size(204, 22);
            openQuPathProjectToolStripMenuItem.Text = "Open QuPath Project";
            openQuPathProjectToolStripMenuItem.Click += openQuPathProjectToolStripMenuItem_Click;
            // 
            // openOMESeriesToolStripMenuItem
            // 
            openOMESeriesToolStripMenuItem.Name = "openOMESeriesToolStripMenuItem";
            openOMESeriesToolStripMenuItem.Size = new Size(204, 22);
            openOMESeriesToolStripMenuItem.Text = "Open OME Series";
            openOMESeriesToolStripMenuItem.Click += openSeriesToolStripMenuItem_Click;
            // 
            // openSeriesToolStripMenuItem
            // 
            openSeriesToolStripMenuItem.Name = "openSeriesToolStripMenuItem";
            openSeriesToolStripMenuItem.Size = new Size(204, 22);
            openSeriesToolStripMenuItem.Text = "Open Series";
            openSeriesToolStripMenuItem.Click += openSeriesToolStripMenuItem_Click_1;
            // 
            // sepToolStripMenuItem1
            // 
            sepToolStripMenuItem1.Name = "sepToolStripMenuItem1";
            sepToolStripMenuItem1.Size = new Size(201, 6);
            // 
            // addImagesToTabToolStripMenuItem
            // 
            addImagesToTabToolStripMenuItem.Name = "addImagesToTabToolStripMenuItem";
            addImagesToTabToolStripMenuItem.Size = new Size(204, 22);
            addImagesToTabToolStripMenuItem.Text = "Add Images To Tab";
            addImagesToTabToolStripMenuItem.Click += addImagesToTabToolStripMenuItem_Click;
            // 
            // addImagesOMEToolStripMenuItem
            // 
            addImagesOMEToolStripMenuItem.Name = "addImagesOMEToolStripMenuItem";
            addImagesOMEToolStripMenuItem.Size = new Size(204, 22);
            addImagesOMEToolStripMenuItem.Text = "Add OME Images To Tab";
            addImagesOMEToolStripMenuItem.Click += addImagesOMEToolStripMenuItem_Click;
            // 
            // sepToolStripMenuItem2
            // 
            sepToolStripMenuItem2.Name = "sepToolStripMenuItem2";
            sepToolStripMenuItem2.Size = new Size(201, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(204, 22);
            saveToolStripMenuItem.Text = "Save Selected Tiff";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveNumPyToolStripMenuItem
            // 
            saveNumPyToolStripMenuItem.Name = "saveNumPyToolStripMenuItem";
            saveNumPyToolStripMenuItem.Size = new Size(204, 22);
            saveNumPyToolStripMenuItem.Text = "Save NumPy";
            saveNumPyToolStripMenuItem.Click += saveNumPyToolStripMenuItem_Click;
            // 
            // saveOMEToolStripMenuItem
            // 
            saveOMEToolStripMenuItem.Name = "saveOMEToolStripMenuItem";
            saveOMEToolStripMenuItem.Size = new Size(204, 22);
            saveOMEToolStripMenuItem.Text = "Save Selected OME";
            saveOMEToolStripMenuItem.Click += saveOMEToolStripMenuItem_Click;
            // 
            // saveTabToolStripMenuItem
            // 
            saveTabToolStripMenuItem.Name = "saveTabToolStripMenuItem";
            saveTabToolStripMenuItem.Size = new Size(204, 22);
            saveTabToolStripMenuItem.Text = "Save Tab OME";
            saveTabToolStripMenuItem.Click += saveTabToolStripMenuItem_Click;
            // 
            // saveTabTiffToolStripMenuItem
            // 
            saveTabTiffToolStripMenuItem.Name = "saveTabTiffToolStripMenuItem";
            saveTabTiffToolStripMenuItem.Size = new Size(204, 22);
            saveTabTiffToolStripMenuItem.Text = "Save Tab Tiff";
            saveTabTiffToolStripMenuItem.Click += saveTabTiffToolStripMenuItem_Click;
            // 
            // saveSeriesToolStripMenuItem
            // 
            saveSeriesToolStripMenuItem.Name = "saveSeriesToolStripMenuItem";
            saveSeriesToolStripMenuItem.Size = new Size(204, 22);
            saveSeriesToolStripMenuItem.Text = "Save Series";
            saveSeriesToolStripMenuItem.Click += saveSeriesToolStripMenuItem_Click;
            // 
            // savePyramidalToolStripMenuItem
            // 
            savePyramidalToolStripMenuItem.Name = "savePyramidalToolStripMenuItem";
            savePyramidalToolStripMenuItem.Size = new Size(204, 22);
            savePyramidalToolStripMenuItem.Text = "Save Pyramidal";
            savePyramidalToolStripMenuItem.Click += savePyramidalToolStripMenuItem_Click;
            // 
            // saveQuPathProjectToolStripMenuItem
            // 
            saveQuPathProjectToolStripMenuItem.Name = "saveQuPathProjectToolStripMenuItem";
            saveQuPathProjectToolStripMenuItem.Size = new Size(204, 22);
            saveQuPathProjectToolStripMenuItem.Text = "Save QuPath Project";
            saveQuPathProjectToolStripMenuItem.Click += saveQuPathProjectToolStripMenuItem_Click;
            // 
            // sepToolStripMenuItem3
            // 
            sepToolStripMenuItem3.Name = "sepToolStripMenuItem3";
            sepToolStripMenuItem3.Size = new Size(201, 6);
            // 
            // imagesToStackToolStripMenuItem
            // 
            imagesToStackToolStripMenuItem.Name = "imagesToStackToolStripMenuItem";
            imagesToStackToolStripMenuItem.Size = new Size(204, 22);
            imagesToStackToolStripMenuItem.Text = "Images To Stack";
            imagesToStackToolStripMenuItem.Click += imagesToStackToolStripMenuItem_Click;
            // 
            // newTabViewToolStripMenuItem
            // 
            newTabViewToolStripMenuItem.Name = "newTabViewToolStripMenuItem";
            newTabViewToolStripMenuItem.Size = new Size(204, 22);
            newTabViewToolStripMenuItem.Text = "New Process";
            newTabViewToolStripMenuItem.Click += newTabViewToolStripMenuItem_Click;
            // 
            // nodeViewToolStripMenuItem
            // 
            nodeViewToolStripMenuItem.Name = "nodeViewToolStripMenuItem";
            nodeViewToolStripMenuItem.Size = new Size(204, 22);
            nodeViewToolStripMenuItem.Text = "Node View";
            nodeViewToolStripMenuItem.Click += nodeViewToolStripMenuItem_Click;
            // 
            // clearRecentToolStripMenuItem
            // 
            clearRecentToolStripMenuItem.Name = "clearRecentToolStripMenuItem";
            clearRecentToolStripMenuItem.Size = new Size(204, 22);
            clearRecentToolStripMenuItem.Text = "Clear Recent";
            clearRecentToolStripMenuItem.Click += clearRecentToolStripMenuItem_Click;
            // 
            // renameToolStripMenuItem
            // 
            renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            renameToolStripMenuItem.Size = new Size(204, 22);
            renameToolStripMenuItem.Text = "Rename";
            renameToolStripMenuItem.Click += renameToolStripMenuItem_Click;
            // 
            // sizeModeToolStripMenuItem
            // 
            sizeModeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rGBToolStripMenuItem, filteredToolStripMenuItem, rawToolStripMenuItem, emissionToolStripMenuItem, _3dToolStripMenuItem, sepToolStripMenuItem, dToolStripMenuItem, xMLToolStripMenuItem });
            sizeModeToolStripMenuItem.Name = "sizeModeToolStripMenuItem";
            sizeModeToolStripMenuItem.Size = new Size(44, 20);
            sizeModeToolStripMenuItem.Text = "View";
            // 
            // rGBToolStripMenuItem
            // 
            rGBToolStripMenuItem.CheckOnClick = true;
            rGBToolStripMenuItem.Name = "rGBToolStripMenuItem";
            rGBToolStripMenuItem.Size = new Size(194, 22);
            rGBToolStripMenuItem.Text = "RGB";
            rGBToolStripMenuItem.Click += rGBToolStripMenuItem_Click;
            // 
            // filteredToolStripMenuItem
            // 
            filteredToolStripMenuItem.Checked = true;
            filteredToolStripMenuItem.CheckOnClick = true;
            filteredToolStripMenuItem.CheckState = CheckState.Checked;
            filteredToolStripMenuItem.Name = "filteredToolStripMenuItem";
            filteredToolStripMenuItem.Size = new Size(194, 22);
            filteredToolStripMenuItem.Text = "Filtered";
            filteredToolStripMenuItem.Click += filteredToolStripMenuItem_Click;
            // 
            // rawToolStripMenuItem
            // 
            rawToolStripMenuItem.CheckOnClick = true;
            rawToolStripMenuItem.Name = "rawToolStripMenuItem";
            rawToolStripMenuItem.Size = new Size(194, 22);
            rawToolStripMenuItem.Text = "Raw";
            rawToolStripMenuItem.Click += rawToolStripMenuItem_Click;
            // 
            // emissionToolStripMenuItem
            // 
            emissionToolStripMenuItem.Name = "emissionToolStripMenuItem";
            emissionToolStripMenuItem.Size = new Size(194, 22);
            emissionToolStripMenuItem.Text = "Emission";
            emissionToolStripMenuItem.Click += emissionToolStripMenuItem_Click;
            // 
            // _3dToolStripMenuItem
            // 
            _3dToolStripMenuItem.Name = "_3dToolStripMenuItem";
            _3dToolStripMenuItem.Size = new Size(194, 22);
            _3dToolStripMenuItem.Text = "3D";
            _3dToolStripMenuItem.Click += _3dToolStripMenuItem_Click;
            // 
            // sepToolStripMenuItem
            // 
            sepToolStripMenuItem.Name = "sepToolStripMenuItem";
            sepToolStripMenuItem.Size = new Size(191, 6);
            // 
            // dToolStripMenuItem
            // 
            dToolStripMenuItem.Checked = true;
            dToolStripMenuItem.CheckOnClick = true;
            dToolStripMenuItem.CheckState = CheckState.Checked;
            dToolStripMenuItem.Name = "dToolStripMenuItem";
            dToolStripMenuItem.Size = new Size(194, 22);
            dToolStripMenuItem.Text = "Hardware Acceleration";
            dToolStripMenuItem.Click += dToolStripMenuItem_Click;
            // 
            // xMLToolStripMenuItem
            // 
            xMLToolStripMenuItem.Name = "xMLToolStripMenuItem";
            xMLToolStripMenuItem.Size = new Size(194, 22);
            xMLToolStripMenuItem.Text = "XML";
            xMLToolStripMenuItem.Click += xMLToolStripMenuItem_Click;
            // 
            // toolboxToolStripMenuItem
            // 
            toolboxToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setToolToolStripMenuItem });
            toolboxToolStripMenuItem.Name = "toolboxToolStripMenuItem";
            toolboxToolStripMenuItem.Size = new Size(47, 20);
            toolboxToolStripMenuItem.Text = "Tools";
            toolboxToolStripMenuItem.Click += toolboxToolStripMenuItem_Click;
            // 
            // setToolToolStripMenuItem
            // 
            setToolToolStripMenuItem.Name = "setToolToolStripMenuItem";
            setToolToolStripMenuItem.Size = new Size(116, 22);
            setToolToolStripMenuItem.Text = "Set Tool";
            setToolToolStripMenuItem.Click += setToolToolStripMenuItem_Click;
            // 
            // rOIToolStripMenuItem
            // 
            rOIToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rOIManagerToolStripMenuItem, exportCSVToolStripMenuItem, importCSVToolStripMenuItem, exportROIsOfFolderOfImagesToolStripMenuItem, importImageJROIToolStripMenuItem, exportImageJROIToolStripMenuItem, importQuPathROIToolStripMenuItem, exportQuPathROIToolStripMenuItem });
            rOIToolStripMenuItem.Name = "rOIToolStripMenuItem";
            rOIToolStripMenuItem.Size = new Size(38, 20);
            rOIToolStripMenuItem.Text = "ROI";
            // 
            // rOIManagerToolStripMenuItem
            // 
            rOIManagerToolStripMenuItem.Name = "rOIManagerToolStripMenuItem";
            rOIManagerToolStripMenuItem.Size = new Size(281, 22);
            rOIManagerToolStripMenuItem.Text = "ROI Manager";
            rOIManagerToolStripMenuItem.Click += rOIManagerToolStripMenuItem_Click;
            // 
            // exportCSVToolStripMenuItem
            // 
            exportCSVToolStripMenuItem.Name = "exportCSVToolStripMenuItem";
            exportCSVToolStripMenuItem.Size = new Size(281, 22);
            exportCSVToolStripMenuItem.Text = "Export ROI's to CSV";
            exportCSVToolStripMenuItem.Click += exportCSVToolStripMenuItem_Click;
            // 
            // importCSVToolStripMenuItem
            // 
            importCSVToolStripMenuItem.Name = "importCSVToolStripMenuItem";
            importCSVToolStripMenuItem.Size = new Size(281, 22);
            importCSVToolStripMenuItem.Text = "Import ROI's from CSV";
            importCSVToolStripMenuItem.Click += importCSVToolStripMenuItem_Click;
            // 
            // exportROIsOfFolderOfImagesToolStripMenuItem
            // 
            exportROIsOfFolderOfImagesToolStripMenuItem.Name = "exportROIsOfFolderOfImagesToolStripMenuItem";
            exportROIsOfFolderOfImagesToolStripMenuItem.Size = new Size(281, 22);
            exportROIsOfFolderOfImagesToolStripMenuItem.Text = "Export ROI's of Folder of Images";
            exportROIsOfFolderOfImagesToolStripMenuItem.Click += exportROIsOfFolderOfImagesToolStripMenuItem_Click;
            // 
            // importImageJROIToolStripMenuItem
            // 
            importImageJROIToolStripMenuItem.Name = "importImageJROIToolStripMenuItem";
            importImageJROIToolStripMenuItem.Size = new Size(281, 22);
            importImageJROIToolStripMenuItem.Text = "Import ImageJ ROI to Selected Image";
            importImageJROIToolStripMenuItem.Click += importImageJROIToolStripMenuItem_Click;
            // 
            // exportImageJROIToolStripMenuItem
            // 
            exportImageJROIToolStripMenuItem.Name = "exportImageJROIToolStripMenuItem";
            exportImageJROIToolStripMenuItem.Size = new Size(281, 22);
            exportImageJROIToolStripMenuItem.Text = "Export ImageJ ROI from Selected Image";
            exportImageJROIToolStripMenuItem.Click += exportImageJROIToolStripMenuItem_Click;
            // 
            // importQuPathROIToolStripMenuItem
            // 
            importQuPathROIToolStripMenuItem.Name = "importQuPathROIToolStripMenuItem";
            importQuPathROIToolStripMenuItem.Size = new Size(281, 22);
            importQuPathROIToolStripMenuItem.Text = "Import QuPath ROI";
            importQuPathROIToolStripMenuItem.Click += importQuPathROIToolStripMenuItem_Click;
            // 
            // exportQuPathROIToolStripMenuItem
            // 
            exportQuPathROIToolStripMenuItem.Name = "exportQuPathROIToolStripMenuItem";
            exportQuPathROIToolStripMenuItem.Size = new Size(281, 22);
            exportQuPathROIToolStripMenuItem.Text = "Export QuPath ROI";
            exportQuPathROIToolStripMenuItem.Click += exportQuPathROIToolStripMenuItem_Click;
            // 
            // channelsToolToolStripMenuItem
            // 
            channelsToolToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { autoThresholdToolStripMenuItem, channelsToolToolStripMenuItem1, switchRedBlueToolStripMenuItem });
            channelsToolToolStripMenuItem.Name = "channelsToolToolStripMenuItem";
            channelsToolToolStripMenuItem.Size = new Size(68, 20);
            channelsToolToolStripMenuItem.Text = "Channels";
            // 
            // autoThresholdToolStripMenuItem
            // 
            autoThresholdToolStripMenuItem.Name = "autoThresholdToolStripMenuItem";
            autoThresholdToolStripMenuItem.Size = new Size(173, 22);
            autoThresholdToolStripMenuItem.Text = "Auto Threshold All";
            autoThresholdToolStripMenuItem.Click += autoThresholdToolStripMenuItem_Click;
            // 
            // channelsToolToolStripMenuItem1
            // 
            channelsToolToolStripMenuItem1.Name = "channelsToolToolStripMenuItem1";
            channelsToolToolStripMenuItem1.Size = new Size(173, 22);
            channelsToolToolStripMenuItem1.Text = "Channels Tool";
            channelsToolToolStripMenuItem1.Click += channelsToolToolStripMenuItem_Click;
            // 
            // switchRedBlueToolStripMenuItem
            // 
            switchRedBlueToolStripMenuItem.Name = "switchRedBlueToolStripMenuItem";
            switchRedBlueToolStripMenuItem.Size = new Size(173, 22);
            switchRedBlueToolStripMenuItem.Text = "Switch Red Blue";
            switchRedBlueToolStripMenuItem.Click += switchRedBlueToolStripMenuItem_Click;
            // 
            // stackToolsToolStripMenuItem
            // 
            stackToolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { stackToolsToolStripMenuItem1, duplicateToolStripMenuItem, rotateToolStripMenuItem, autoFocusToolStripMenuItem, extractRegionPyramidalToolStripMenuItem });
            stackToolsToolStripMenuItem.Name = "stackToolsToolStripMenuItem";
            stackToolsToolStripMenuItem.Size = new Size(52, 20);
            stackToolsToolStripMenuItem.Text = "Stacks";
            // 
            // stackToolsToolStripMenuItem1
            // 
            stackToolsToolStripMenuItem1.Name = "stackToolsToolStripMenuItem1";
            stackToolsToolStripMenuItem1.Size = new Size(205, 22);
            stackToolsToolStripMenuItem1.Text = "Stack Tool";
            stackToolsToolStripMenuItem1.Click += stackToolsToolStripMenuItem_Click;
            // 
            // duplicateToolStripMenuItem
            // 
            duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            duplicateToolStripMenuItem.Size = new Size(205, 22);
            duplicateToolStripMenuItem.Text = "Duplicate";
            duplicateToolStripMenuItem.Click += duplicateToolStripMenuItem_Click;
            // 
            // rotateToolStripMenuItem
            // 
            rotateToolStripMenuItem.Name = "rotateToolStripMenuItem";
            rotateToolStripMenuItem.Size = new Size(205, 22);
            rotateToolStripMenuItem.Text = "Rotate Flip";
            rotateToolStripMenuItem.DropDownOpening += rotateToolStripMenuItem_DropDownOpening;
            rotateToolStripMenuItem.DropDownItemClicked += rotateToolStripMenuItem_DropDownItemClicked;
            // 
            // autoFocusToolStripMenuItem
            // 
            autoFocusToolStripMenuItem.Name = "autoFocusToolStripMenuItem";
            autoFocusToolStripMenuItem.Size = new Size(205, 22);
            autoFocusToolStripMenuItem.Text = "Auto Focus";
            autoFocusToolStripMenuItem.Click += autoFocusToolStripMenuItem_Click;
            // 
            // extractRegionPyramidalToolStripMenuItem
            // 
            extractRegionPyramidalToolStripMenuItem.Name = "extractRegionPyramidalToolStripMenuItem";
            extractRegionPyramidalToolStripMenuItem.Size = new Size(205, 22);
            extractRegionPyramidalToolStripMenuItem.Text = "Extract Region Pyramidal";
            extractRegionPyramidalToolStripMenuItem.Click += extractRegionPyramidalToolStripMenuItem_Click;
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, sizeModeToolStripMenuItem, toolboxToolStripMenuItem, rOIToolStripMenuItem, channelsToolToolStripMenuItem, stackToolsToolStripMenuItem, formatToolStripMenuItem, filtersToolStripMenuItem, scriptToolStripMenuItem, aboutToolStripMenuItem, recentToolStripMenuItem, commandToolStripMenuItem, runToolStripMenuItem1, oMEROToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(7, 2, 0, 2);
            menuStrip.Size = new Size(807, 24);
            menuStrip.TabIndex = 0;
            // 
            // formatToolStripMenuItem
            // 
            formatToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { bit8ToolStripMenuItem, bit16ToolStripMenuItem, to24BitToolStripMenuItem, to36BitToolStripMenuItem, to48BitToolStripMenuItem });
            formatToolStripMenuItem.Name = "formatToolStripMenuItem";
            formatToolStripMenuItem.Size = new Size(57, 20);
            formatToolStripMenuItem.Text = "Format";
            // 
            // bit8ToolStripMenuItem
            // 
            bit8ToolStripMenuItem.Name = "bit8ToolStripMenuItem";
            bit8ToolStripMenuItem.Size = new Size(119, 22);
            bit8ToolStripMenuItem.Text = "To 8 Bit";
            bit8ToolStripMenuItem.Click += bit8ToolStripMenuItem_Click;
            // 
            // bit16ToolStripMenuItem
            // 
            bit16ToolStripMenuItem.Name = "bit16ToolStripMenuItem";
            bit16ToolStripMenuItem.Size = new Size(119, 22);
            bit16ToolStripMenuItem.Text = "To 16 Bit";
            bit16ToolStripMenuItem.Click += bit16ToolStripMenuItem_Click;
            // 
            // to24BitToolStripMenuItem
            // 
            to24BitToolStripMenuItem.Name = "to24BitToolStripMenuItem";
            to24BitToolStripMenuItem.Size = new Size(119, 22);
            to24BitToolStripMenuItem.Text = "To 24 Bit";
            to24BitToolStripMenuItem.Click += to24BitToolStripMenuItem_Click;
            // 
            // to36BitToolStripMenuItem
            // 
            to36BitToolStripMenuItem.Name = "to36BitToolStripMenuItem";
            to36BitToolStripMenuItem.Size = new Size(119, 22);
            to36BitToolStripMenuItem.Text = "To 32 Bit";
            to36BitToolStripMenuItem.Click += to32BitToolStripMenuItem_Click;
            // 
            // to48BitToolStripMenuItem
            // 
            to48BitToolStripMenuItem.Name = "to48BitToolStripMenuItem";
            to48BitToolStripMenuItem.Size = new Size(119, 22);
            to48BitToolStripMenuItem.Text = "To 48 Bit";
            to48BitToolStripMenuItem.Click += to48BitToolStripMenuItem_Click;
            // 
            // filtersToolStripMenuItem
            // 
            filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            filtersToolStripMenuItem.Size = new Size(50, 20);
            filtersToolStripMenuItem.Text = "Filters";
            filtersToolStripMenuItem.Click += filtersToolStripMenuItem_Click;
            // 
            // scriptToolStripMenuItem
            // 
            scriptToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { runToolStripMenuItem, createFunctionToolStripMenuItem, consoleToolStripMenuItem, scriptRunnerToolStripMenuItem, scriptRecorderToolStripMenuItem });
            scriptToolStripMenuItem.Name = "scriptToolStripMenuItem";
            scriptToolStripMenuItem.Size = new Size(49, 20);
            scriptToolStripMenuItem.Text = "Script";
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(154, 22);
            runToolStripMenuItem.Text = "Run";
            runToolStripMenuItem.DropDownOpening += runToolStripMenuItem_DropDownOpening;
            // 
            // createFunctionToolStripMenuItem
            // 
            createFunctionToolStripMenuItem.Name = "createFunctionToolStripMenuItem";
            createFunctionToolStripMenuItem.Size = new Size(154, 22);
            createFunctionToolStripMenuItem.Text = "Functions Tool";
            createFunctionToolStripMenuItem.Click += createFunctionToolStripMenuItem_Click;
            // 
            // consoleToolStripMenuItem
            // 
            consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            consoleToolStripMenuItem.Size = new Size(154, 22);
            consoleToolStripMenuItem.Text = "Console";
            consoleToolStripMenuItem.Click += consoleToolStripMenuItem_Click;
            // 
            // scriptRunnerToolStripMenuItem
            // 
            scriptRunnerToolStripMenuItem.Name = "scriptRunnerToolStripMenuItem";
            scriptRunnerToolStripMenuItem.Size = new Size(154, 22);
            scriptRunnerToolStripMenuItem.Text = "Script Runner";
            scriptRunnerToolStripMenuItem.Click += scriptRunnerToolStripMenuItem_Click_1;
            // 
            // scriptRecorderToolStripMenuItem
            // 
            scriptRecorderToolStripMenuItem.Name = "scriptRecorderToolStripMenuItem";
            scriptRecorderToolStripMenuItem.Size = new Size(154, 22);
            scriptRecorderToolStripMenuItem.Text = "Script Recorder";
            scriptRecorderToolStripMenuItem.Click += scriptRecorderToolStripMenuItem_Click_1;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // recentToolStripMenuItem
            // 
            recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            recentToolStripMenuItem.Size = new Size(55, 20);
            recentToolStripMenuItem.Text = "Recent";
            // 
            // commandToolStripMenuItem
            // 
            commandToolStripMenuItem.Name = "commandToolStripMenuItem";
            commandToolStripMenuItem.Size = new Size(76, 20);
            commandToolStripMenuItem.Text = "Command";
            // 
            // runToolStripMenuItem1
            // 
            runToolStripMenuItem1.Name = "runToolStripMenuItem1";
            runToolStripMenuItem1.Size = new Size(40, 20);
            runToolStripMenuItem1.Text = "Run";
            // 
            // openImageJDialog
            // 
            openImageJDialog.DefaultExt = "roi";
            openImageJDialog.Filter = "ROI Files (*.roi)|*.roi|All files (*.*)|*.*";
            openImageJDialog.Multiselect = true;
            openImageJDialog.Title = "Import ROI from CSV";
            // 
            // saveImageJDialog
            // 
            saveImageJDialog.DefaultExt = "roi";
            saveImageJDialog.Filter = "ROI Files (*.roi)|*.roi|All files (*.*)|*.*";
            saveImageJDialog.Title = "Save ROIs to CSV";
            // 
            // saveQuPathDialog
            // 
            saveQuPathDialog.DefaultExt = "roi";
            saveQuPathDialog.Filter = "QuPath GeoJSON Files (*.geojson)|*.roi|All files (*.*)|*.*";
            saveQuPathDialog.Title = "Save ROIs to CSV";
            // 
            // openQuPathDialog
            // 
            openQuPathDialog.DefaultExt = "roi";
            openQuPathDialog.Filter = "QuPath GeoJSON Files (*.geojson)|*.roi|All files (*.*)|*.*";
            openQuPathDialog.Multiselect = true;
            openQuPathDialog.Title = "Import ROI from CSV";
            // 
            // openQuPathProjDialog
            // 
            openQuPathProjDialog.DefaultExt = "roi";
            openQuPathProjDialog.Filter = "QuPath Project Files (*.qpproj)|*.qpproj|All files (*.*)|*.*";
            openQuPathProjDialog.Multiselect = true;
            openQuPathProjDialog.Title = "Open QuPath Project.";
            // 
            // saveNumPyDialog
            // 
            saveNumPyDialog.DefaultExt = "roi";
            saveNumPyDialog.Filter = "NumPy Files (*.npy)|*.npy|All files (*.*)|*.*";
            saveNumPyDialog.Title = "QuPath Project Save";
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "roi";
            saveFileDialog1.Filter = "QuPath Project Files (*.qpproj)|*.qpproj|All files (*.*)|*.*";
            saveFileDialog1.Title = "QuPath Project Save";
            // 
            // oMEROToolStripMenuItem
            // 
            oMEROToolStripMenuItem.Name = "oMEROToolStripMenuItem";
            oMEROToolStripMenuItem.Size = new Size(61, 20);
            oMEROToolStripMenuItem.Text = "OMERO";
            oMEROToolStripMenuItem.Click += oMEROToolStripMenuItem_Click;
            // 
            // TabsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(69, 77, 98);
            ClientSize = new Size(807, 29);
            Controls.Add(panel);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(231, 68);
            Name = "TabsView";
            Text = "Bio";
            Activated += ImageViewer_Activated;
            FormClosing += TabsView_FormClosing;
            Load += TabsView_Load;
            KeyDown += TabsView_KeyDown;
            KeyPress += TabsView_KeyPress;
            KeyUp += TabsView_KeyUp;
            PreviewKeyDown += ImageViewer_PreviewKeyDown;
            panel.ResumeLayout(false);
            tabContextMenuStrip.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFilesDialog;
        private System.Windows.Forms.SaveFileDialog saveOMEFileDialog;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.SaveFileDialog saveCSVFileDialog;
        private System.Windows.Forms.OpenFileDialog openCSVFileDialog;
        private System.Windows.Forms.SaveFileDialog saveTiffFileDialog;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveOMEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sizeModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rGBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filteredToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolboxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rOIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rOIManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportROIsOfFolderOfImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem channelsToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoThresholdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem channelsToolToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem stackToolsToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem filtersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bit8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bit16ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem to24BitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem to36BitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem to48BitToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.ContextMenuStrip tabContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptRunnerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptRecorderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOMEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newTabViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nodeViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveOMEToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImagesToTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImagesOMEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOMESeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTabTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stackToolsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem emissionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchRedBlueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createFunctionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imagesToStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sepToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sepToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator sepToolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator sepToolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem _3dToolStripMenuItem;
        private ToolStripMenuItem importImageJROIToolStripMenuItem;
        private ToolStripMenuItem exportImageJROIToolStripMenuItem;
        private OpenFileDialog openImageJDialog;
        private SaveFileDialog saveImageJDialog;
        private ToolStripMenuItem autoFocusToolStripMenuItem;
        private ToolStripMenuItem savePyramidalToolStripMenuItem;
        private ToolStripMenuItem recentToolStripMenuItem;
        private ToolStripMenuItem commandToolStripMenuItem;
        private ToolStripMenuItem runToolStripMenuItem1;
        private ToolStripMenuItem importQuPathROIToolStripMenuItem;
        private ToolStripMenuItem exportQuPathROIToolStripMenuItem;
        private SaveFileDialog saveQuPathDialog;
        private OpenFileDialog openQuPathDialog;
        private ToolStripMenuItem extractRegionPyramidalToolStripMenuItem;
        private ToolStripMenuItem saveNumPyToolStripMenuItem;
        private ToolStripMenuItem saveQuPathProjectToolStripMenuItem;
        private ToolStripMenuItem openQuPathProjectToolStripMenuItem;
        private OpenFileDialog openQuPathProjDialog;
        private SaveFileDialog saveNumPyDialog;
        private ToolStripMenuItem renameToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem oMEROToolStripMenuItem;
    }
}