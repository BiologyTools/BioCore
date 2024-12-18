using AForge;
using BioCore;
using BioLib;
namespace BioCore
{
    public class App
    {
        public static ROIManager manager = null;
        public static ChannelsTool channelsTool = null;
        public static TabsView tabsView = null;
        public static NodeView nodeView = null;
        public static Scripting runner = null;
        public static Recorder recorder = null;
        public static Tools tools = null;
        public static StackTools stackTools = null;
        public static ImageView viewer = null;
        public static Series seriesTool = null;
        public static BioConsole console = null;
        public static Library lib = null;
        public static List<string> recent = new List<string>();

        /* A property that returns the current image. */
        /// <summary>
        /// Gets the BioImage object for the selected image in the ImageView.
        /// If no image is selected, returns the image from the tabsView.
        /// </summary>
        /// <value>The selected BioImage object.</value>
        public static BioImage Image
        {
            get
            {
                if (ImageView.SelectedImage == null)
                    return tabsView.Image;
                return ImageView.SelectedImage;
            }
        }
        public static List<Channel> Channels
        {
            get { return Image.Channels; }
        }

        public static List<ROI> Annotations
        {
            get { return Image.Annotations; }
        }
        public static bool Initialized { get; private set; }
        /// Initialize() is a function that initializes the BioImage Suite Web viewer
        public static void Initialize(bool requireImageJ = false)
        {
            BioImage.Initialize();
            Microscope.Initialize();
            Fiji.Initialize(requireImageJ);
            tabsView = new TabsView();
            viewer = new ImageView();
            stackTools = new StackTools();
            tools = new Tools();
            manager = new ROIManager();
            runner = new Scripting();
            recorder = new Recorder();
            seriesTool = new Series();
            lib = new Library();
            console = new BioConsole();
            Initialized = true;
        }
        /// Hide() hides all the tools
        public static void Hide()
        {
            tools.Hide();
            stackTools.Hide();
            manager.Hide();
            runner.Hide();
            recorder.Hide();
            seriesTool.Hide();
            console.Hide();
        }
        /// It takes a string and a function, and adds the function to the menu item specified by the
        /// string
        /// 
        /// @param s The path to the menu item.
        /// @param Function 
        /// 
        /// @return A ToolStripItem
        public static ToolStripItem GetMenuItemFromPath(string s)
        {
            if (s == "" || s == null)
                return null;
            string[] sts = s.Split('/');
        start:
            List<ToolStripMenuItem> allItems = GetMenuItems();
            //Find path or create it.
            bool found = false;
            ToolStripMenuItem item = null;

            for (int t = 0; t < sts.Length; t++)
            {
                found = false;
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Text == sts[t])
                    {
                        item = allItems[i];
                        found = true;
                        if (t == sts.Length - 1)
                            return allItems[i];
                    }
                }
                if (!found)
                {
                    if (t == 0)
                    {
                        tabsView.MainMenuStrip.Items.Add(sts[t]);
                        goto start;
                    }
                    else if (t > 0 && t < sts.Length)
                    {
                        ToolStripMenuItem itm = new ToolStripMenuItem();
                        itm.Text = Path.GetFileName(s);
                        item.DropDownItems.Add(Path.GetFileName(s), null, ItemClicked);
                        return item;
                    }
                    else
                    {
                        item.DropDownItems.Add(sts[t]);
                    }
                }
            }
            return item;
        }
        /// It takes a string and a function, and adds the function to the context menu at the location
        /// specified by the string
        /// 
        /// @param s The path to the item.
        /// @param Function 
        /// 
        /// @return A ToolStripItem
        public static ToolStripItem GetContextMenuItemFromPath(string s)
        {
            if (s == "" || s == null)
                return null;
            string[] sts = s.Split('/');
        start:
            List<ToolStripMenuItem> allItems = GetContextItems();
            //Find path or create it.
            bool found = false;
            ToolStripMenuItem item = null;

            for (int t = 0; t < sts.Length; t++)
            {
                found = false;
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Text == sts[t])
                    {
                        item = allItems[i];
                        found = true;
                        if (t == sts.Length - 1)
                            return allItems[i];
                    }
                }
                if (!found)
                {
                    if (t == 0)
                    {
                        viewer.ViewContextMenu.Items.Add(sts[t]);
                        goto start;
                    }
                    else if (t > 0 && t < sts.Length)
                    {
                        ToolStripMenuItem itm = new ToolStripMenuItem();
                        itm.Name = Path.GetFileName(s);
                        item.DropDownItems.Add(Path.GetFileName(s), null, ItemClicked);
                        return item;
                    }
                    else
                    {
                        item.DropDownItems.Add(sts[t]);
                    }
                }
            }
            return item;
        }

        /// It takes a string and a function and adds the function to the menu item specified by the
        /// string
        /// 
        /// @param menu The menu path to add the menu item to.
        /// @param Function The function that will be called when the menu item is clicked.
        public static void AddMenu(string menu)
        {
            GetMenuItemFromPath(menu);
        }
        /// It takes a string and a function and adds a context menu item to the context menu
        /// 
        /// @param menu The path to the menu item.
        /// @param Function The function that will be called when the menu item is clicked.
        public static void AddContextMenu(string menu)
        {
            GetContextMenuItemFromPath(menu);
        }

        public static void SelectWindow(string name)
        {
            TabsView tbs = App.tabsView;

            int c = tbs.GetViewerCount();
            for (int v = 0; v < c; v++)
            {
                int cc = tbs.GetViewer(v).Images.Count;
                for (int im = 0; im < cc; im++)
                {
                    if (tbs.GetViewer(v).Images[im].Filename == Path.GetFileName(name))
                    {
                        tbs.SetTab(v);
                        BioLib.Recorder.Record("App.SelectWindow(\"" + name + "\")");
                        ImageView.SelectedImage = tbs.GetViewer(v).Images[im];
                        viewer = tbs.GetViewer(v);
                        viewer.BringToFront();
                        viewer.Focus();
                        return;
                    }
                }
            }
        }

        public static void CloseWindow(string name)
        {
            BioLib.Recorder.Record("App.CloseWindow(\"" + name + "\")");
            TabsView tbs = App.tabsView;
            int c = tbs.GetViewerCount();
            if (name == "ROI Manager")
                App.manager.Close();
            else
            if (name == "Stack Tool")
                App.stackTools.Close();
            else
            if (name == "Channels Tool")
                App.channelsTool.Close();
            else
            if (name == "Recorder")
                App.recorder.Close();
            else
            if (name == "Tools")
                App.tools.Close();
            else
                for (int v = 0; v < c; v++)
                {
                    int cc = tbs.GetViewer(v).Images.Count;
                    for (int im = 0; im < cc; im++)
                    {
                        ImageView vi = tbs.GetViewer(v);
                        if (vi.Images[im].Filename == Path.GetFileName(name))
                        {
                            tbs.RemoveTab(name);
                            vi.Dispose();
                            return;
                        }
                        else if (vi.Name == name)
                        {
                            tbs.RemoveTab(name);
                            vi.Dispose();
                            return;
                        }
                    }
                }

        }

        public static void Rename(string text)
        {
            App.tabsView.RenameTab(ImageView.SelectedImage.Filename, text);
            ImageView.SelectedImage.Rename(text);
            ImageView v = App.tabsView.GetViewer(ImageView.SelectedImage.Filename);
            v.SetTitle(ImageView.SelectedImage.Filename);
            BioLib.Recorder.AddLine("App.Rename(\"" + text + "\");", false);
        }
        /// It returns a list of all the items in a menu, including submenus
        /// 
        /// @param ToolStripMenuItem The menu item you want to get the sub items from.
        private static IEnumerable<ToolStripMenuItem> GetItems(ToolStripMenuItem item)
        {
            foreach (var dropDownItem in item.DropDownItems)
            {
                if (dropDownItem.GetType() == typeof(ToolStripMenuItem))
                {
                    if (((ToolStripMenuItem)dropDownItem).HasDropDownItems)
                    {
                        foreach (ToolStripMenuItem subItem in GetItems((ToolStripMenuItem)dropDownItem))
                            yield return subItem;
                    }
                    yield return (ToolStripMenuItem)dropDownItem;
                }
            }
        }
        /// <summary>
        /// Retrieves all the menu items from the main menu strip
        /// </summary>
        /// <returns>A list of ToolStripMenuItem objects representing the menu items</returns>
        public static List<ToolStripMenuItem> GetMenuItems()
        {
            List<ToolStripMenuItem> allItems = new List<ToolStripMenuItem>();
            foreach (ToolStripMenuItem toolItem in tabsView.MainMenuStrip.Items)
            {
                allItems.Add(toolItem);
                allItems.AddRange(GetItems(toolItem));
            }
            return allItems;
        }
        /// It takes a list of items, and for each item in the list, it adds that item to the list, and
        /// then adds all the items in that item's submenu to the list
        /// 
        /// @return A list of ToolStripMenuItems
        public static List<ToolStripMenuItem> GetContextItems()
        {
            List<ToolStripMenuItem> allItems = new List<ToolStripMenuItem>();
            foreach (ToolStripMenuItem toolItem in viewer.ViewContextMenu.Items)
            {
                allItems.Add(toolItem);
                allItems.AddRange(GetItems(toolItem));
            }
            return allItems;
        }
        /// It takes a string and a function, and adds the function to the menu item specified by the
        /// string
        /// 
        /// @param s The path to the menu item.
        /// @param Function 
        /// 
        /// @return A ToolStripItem
        public static ToolStripItem GetMenuItemFromPath(string s, Function f)
        {
            if (s == "" || s == null)
                return null;
            string[] sts = s.Split('/');
        start:
            List<ToolStripMenuItem> allItems = GetMenuItems();
            //Find path or create it.
            bool found = false;
            ToolStripMenuItem item = null;

            for (int t = 0; t < sts.Length; t++)
            {
                found = false;
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Text == sts[t])
                    {
                        item = allItems[i];
                        found = true;
                        if (t == sts.Length - 1)
                            return allItems[i];
                    }
                }
                if (!found)
                {
                    if (t == 0)
                    {
                        tabsView.MainMenuStrip.Items.Add(sts[t]);
                        goto start;
                    }
                    else if (t > 0 && t < sts.Length)
                    {
                        ToolStripMenuItem itm = new ToolStripMenuItem();
                        itm.Tag = f;
                        itm.Name = f.Name;
                        item.DropDownItems.Add(f.Name, null, ItemClicked);
                        return item;
                    }
                    else
                    {
                        item.DropDownItems.Add(sts[t]);
                    }
                }
            }
            return item;
        }
        /// It takes a string and a function, and adds the function to the context menu at the location
        /// specified by the string
        /// 
        /// @param s The path to the item.
        /// @param Function 
        /// 
        /// @return A ToolStripItem
        public static ToolStripItem GetContextMenuItemFromPath(string s, Function f)
        {
            if (s == "" || s == null)
                return null;
            string[] sts = s.Split('/');
        start:
            List<ToolStripMenuItem> allItems = GetContextItems();
            //Find path or create it.
            bool found = false;
            ToolStripMenuItem item = null;

            for (int t = 0; t < sts.Length; t++)
            {
                found = false;
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Text == sts[t])
                    {
                        item = allItems[i];
                        found = true;
                        if (t == sts.Length - 1)
                            return allItems[i];
                    }
                }
                if (!found)
                {
                    if (t == 0)
                    {
                        viewer.ViewContextMenu.Items.Add(sts[t]);
                        goto start;
                    }
                    else if (t > 0 && t < sts.Length)
                    {
                        ToolStripMenuItem itm = new ToolStripMenuItem();
                        itm.Tag = f;
                        itm.Name = f.Name;
                        item.DropDownItems.Add(f.Name, null, ItemClicked);
                        return item;
                    }
                    else
                    {
                        item.DropDownItems.Add(sts[t]);
                    }
                }
            }
            return item;
        }
        /// It takes a string and a function and adds the function to the menu item specified by the
        /// string
        /// 
        /// @param menu The menu path to add the menu item to.
        /// @param Function The function that will be called when the menu item is clicked.
        public static void AddMenu(string menu, Function f)
        {
            GetMenuItemFromPath(menu, f);
        }
        /// It takes a string and a function and adds a context menu item to the context menu
        /// 
        /// @param menu The path to the menu item.
        /// @param Function The function that will be called when the menu item is clicked.
        public static void AddContextMenu(string menu, Function f)
        {
            GetContextMenuItemFromPath(menu, f);
        }
        /// It takes the sender object, casts it to a ToolStripMenuItem, gets the Tag property, casts it
        /// to a Function, and calls the PerformFunction method
        /// 
        /// @param sender The object that sent the event.
        /// @param EventArgs The event arguments.
        private static void ItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem ts = (ToolStripMenuItem)sender;
            if (ts.Text.EndsWith(".dll"))
            {
                Plugin.Plugins[ts.Text].Execute(new string[] { });
            }
            else if (ts.Text.EndsWith(".ijm"))
            {
                string s =  File.ReadAllText(Path.GetDirectoryName(Fiji.ImageJPath) + "/macros/" + ts.Text);
                Fiji.RunOnImage(s, BioConsole.headless, BioConsole.onTab, BioConsole.useBioformats, BioConsole.newTab);
            }
            else
            {
                Function f = (Function)ts.Tag;
                f.PerformFunction(true);
            }
        }
        /// It takes a string, converts it to a ROI, and adds it to the list of ROIs
        /// 
        /// @param an the string representation of the ROI
        public static void AddROI(string an)
        {
            Annotations.Add(BioImage.StringToROI(an));
            Recorder.AddLine("App.AddROI(" + '"' + an + "'" + ");");
        }
    }
}
