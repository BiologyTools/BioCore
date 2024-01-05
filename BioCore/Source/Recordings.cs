using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BioCore
{
    public partial class Recordings : Form
    {
       /* Creating a new directory called Recordings. */
        public Recordings()
        {
            InitializeComponent();
            if (!Directory.Exists("Recordings"))
                Directory.CreateDirectory("Recordings");
            foreach (string file in Directory.GetFiles("Recordings"))
            {
                if (file.EndsWith("reco"))
                    OpenRecording(file);
                else if (file.EndsWith("pro"))
                    OpenProperty(file);
            }
            foreach (Automation.Action.ValueType val in (Automation.Action.ValueType[])Enum.GetValues(typeof(Automation.Action.ValueType)))
            {
                propBox.Items.Add(val);
            }

        }
        /// It's a function that adds nodes to a treeview
        public void InitElements()
        {
            view.Nodes.Clear();
            for (int i = 0; i < Automation.Recordings.Count; i++)
            {

            }
            foreach (Automation.Recording rec in Automation.Recordings.Values)
            {
                TreeNode tr = new TreeNode();
                Node n = new Node(rec, tr, Node.Type.recording);
                tr.Tag = n;
                tr.Text = rec.Name;
                foreach (Automation.Action item in rec.List)
                {
                    try
                    {
                        TreeNode tn = new TreeNode();
                        tn.Text = item.ToString();
                        
                        Node no = new Node(item, tn,Node.Type.action);
                        no.recording = rec;
                        tn.Tag = no;
                        tr.Nodes.Add(tn);
                    }
                    catch (Exception)
                    {

                    }
                }
                view.Nodes.Add(tr);
            }

            propView.Nodes.Clear();
            foreach (Automation.Recording rec in Automation.Properties.Values)
            {
                TreeNode tr = new TreeNode();
                Node n = new Node(rec, tr, Node.Type.recording);
                tr.Tag = n;
                tr.Text = rec.Name;
                foreach (Automation.Action item in rec.List)
                {
                    try
                    {
                        TreeNode tn = new TreeNode();
                        tn.Text = item.ToString();

                        Node no = new Node(item, tn, Node.Type.action);
                        no.recording = rec;
                        tn.Tag = no;
                        tr.Nodes.Add(tn);
                    }
                    catch (Exception)
                    {

                    }
                }
                propView.Nodes.Add(tr);
            }
        }
        /// It clears the nodes of the treeview, then adds the nodes back in.
        public void UpdateElements()
        {
            foreach (TreeNode rec in view.Nodes)
            {
                rec.Nodes.Clear();
                Node n = (Node)rec.Tag;
                foreach (Automation.Action item in n.recording.List)
                {
                    try
                    {
                        TreeNode tr = new TreeNode();
                        tr.Text = item.ToString();
                        Node no = new Node(item, rec, Node.Type.action);
                        no.recording = n.recording;
                        tr.Tag = no;
                        rec.Nodes.Add(tr);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            foreach (TreeNode rec in propView.Nodes)
            {
                rec.Nodes.Clear();
                Node n = (Node)rec.Tag;
                foreach (Automation.Action item in n.recording.List)
                {
                    try
                    {
                        TreeNode tr = new TreeNode();
                        tr.Text = item.ToString();
                        Node no = new Node(item, rec, Node.Type.action);
                        no.recording = n.recording;
                        tr.Tag = no;
                        rec.Nodes.Add(tr);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        /* It's a class that contains a recording and an action, and it's used to store the data in the
        treeview */
        public class Node
        {
            public Automation.Recording recording;
            public Automation.Action action;
            public Type type;
            public enum Type
            {
                recording,
                action
            }

            public TreeNode node;
            /* Creating a new node in the tree. */
            public Node(Automation.Action el,TreeNode n,Type t)
            {
                action = el;
                node = n;
                type = t;
                recording = new Automation.Recording();
                recording.List.Add(el);
                //items = Automation.AutomationHelpers.GetAllChildren(el.element);
            }
            /* Creating a new node in the tree. */
            public Node(Automation.Recording rec, TreeNode n, Type t)
            {
                recording = rec;
                node = n;
                type = t;
                //items = Automation.AutomationHelpers.GetAllChildren(el.element);
            }
            public override string ToString()
            {
                return action.Name + ", " + action.AutomationID + ", " + action.ClassName.ToString() + ", " + action.ActionType; 
            }
        }

        /// It runs a recording
        /// 
        /// @param act The name of the recording you want to run.
        public static void Perform(string act)
        {
            Automation.Recording rec = (Automation.Recording)Automation.Recordings[act];
            rec.Run();
            Recorder.AddLine("Recordings.Perform(" + rec.Name + ");");
        }
        /// It gets the value of a property from the automation object
        /// 
        /// @param automation This is the automation object that is passed to the function.
        /// @param pro The name of the property you want to get.
        /// 
        /// @return The value of the property.
        public static object GetProperty(Automation.Action.ValueType automation,string pro)
        {
            Automation.Recording rec = null;
            if (Automation.Properties.ContainsKey(pro))
                rec = (Automation.Recording)Automation.Properties[pro];
            else
                return null;
            Recorder.AddLine("Recordings.Get(" + rec.Name + ");");
            return rec.Get();
        }

        /// The function is called after a node is selected in the tree view
        /// 
        /// @param sender The object that raised the event.
        /// @param TreeViewEventArgs 
        private void view_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        /// The function starts recording the user's actions on the screen
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The return value is the result of the ShowDialog method.
        private void startBut_Click(object sender, EventArgs e)
        {
            TextInput ti = new TextInput("Property" + propBox.Items.Count);
            ti.Text = "Set Property Name";
            if (ti.ShowDialog() != DialogResult.OK)
                return;
            recordStatusLabel.Text = "Recording: Started";
            if (Automation.IsRecording)
                return;
            Automation.StartRecording();
        }

        /// The function stops the recording and initializes the elements
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The actions that are being recorded.
        private void stopBut_Click(object sender, EventArgs e)
        {
            recordStatusLabel.Text = "Recording: Stopped";
            if (!Automation.IsRecording)
                return;
            Automation.StopRecording();
            if (Automation.Recordings.Count > 0)
            {
                //actions = Automation.Recordings[0].List;
                InitElements();
            }
        }

        /// If the user clicks the play button, and the selected node is not null, and the selected node
        /// is not an action, then play the recording
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The recording is being returned.
        private void playBut_Click(object sender, EventArgs e)
        {
            if (view.SelectedNode == null)
                return;
            Node n = (Node)view.SelectedNode.Tag;
            if (n.type == Node.Type.action)
                return;;
            n.recording.Run();
        }

        private void view_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            
        }

        private void Elements_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        /// When the user clicks on the "Perform" menu item, the selected node is retrieved and the
        /// action or recording is performed
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void performToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tr = view.SelectedNode;
            Node no = (Node)tr.Tag;
            if (no.type == Node.Type.action)
                no.action.Perform();
            else
                no.recording.Run();
        }
        /// If the user clicks the save button, then open a dialog box to save the file.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the file that was saved.
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveRecDialog.ShowDialog() != DialogResult.OK)
                return;
            SaveRecording(saveRecDialog.FileName);
        }
        /// It saves the recording to a file
        /// 
        /// @param file the file path
        /// 
        /// @return The file name of the file being saved.
        private void SaveRecording(string file)
        {
            Automation.Recording n;
            if (view.SelectedNode == null)
                return;
            else
            {
                n = ((Node)view.SelectedNode.Tag).recording;
            }
            string s = Path.GetFileNameWithoutExtension(saveRecDialog.FileName);
            n.Name = s;
            string j = JsonConvert.SerializeObject(n.List, Formatting.None);
            File.WriteAllText(file, j);
            InitElements();
        }
        /// The function takes a file name as a parameter and saves the property of the selected node in
        /// the tree view to the file
        /// 
        /// @param file the file path
        /// 
        /// @return The file name of the file being saved.
        private void SaveProperty(string file)
        {
            Automation.Recording n;
            if (propView.SelectedNode == null)
                return;
            else
            {
                n = ((Node)propView.SelectedNode.Tag).recording;
            }
            string s = Path.GetFileNameWithoutExtension(savePropDialog.FileName);
            n.Name = s;
            
            string j = JsonConvert.SerializeObject(n.List, Formatting.None);
            File.WriteAllText(file, j);
            InitElements();
        }
        /// It reads a JSON file and converts it into a list of actions
        /// 
        /// @param file the file path of the recording
        /// 
        /// @return A list of Automation.Recording objects.
        private Automation.Recording OpenRec(string file)
        {
            string st = File.ReadAllText(file);
            Automation.Recording rec = new Automation.Recording();
            rec.Name = Path.GetFileNameWithoutExtension(file);
            rec.File = file;
            Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(st);
            for (int i = 0; i < ar.Count; i++)
            {
                Automation.Action.Type t = (Automation.Action.Type)Enum.Parse(typeof(Automation.Action.Type), ar[i].ElementAt(0).First.ToString());
                if (t == Automation.Action.Type.keydown || t == Automation.Action.Type.keyup)
                {
                    Keys k = (Keys)int.Parse(ar[i].ElementAt(2).First.ToString());
                    KeyEventArgs kea = new KeyEventArgs(k);
                    int ind = int.Parse(ar[i].ElementAt(9).First.ToString());
                    Automation.Action ac = new Automation.Action(t, ar[i].ElementAt(8).First.ToString(), ar[i].ElementAt(9).First.ToString(), ind, ar[i].ElementAt(3).First.ToString(), ar[i].ElementAt(4).First.ToString(), ar[i].ElementAt(5).First.ToString(), kea);
                    rec.List.Add(ac);
                }
                if (t == Automation.Action.Type.mousedown || t == Automation.Action.Type.mouseup)
                {
                    MouseButtons mb = (MouseButtons)int.Parse(ar[i].ElementAt(2).First.ToString());
                    string ps = ar[i].ElementAt(3).First.ToString();
                    string xs = ps.Substring(0, ps.IndexOf(','));
                    string ys = ps.Substring(ps.IndexOf(',')+1, ps.Length - (ps.IndexOf(',')+1));
                    int ind = int.Parse(ar[i].ElementAt(9).First.ToString());
                    System.Drawing.Point po = new Point(int.Parse(xs),int.Parse(ys));
                    MouseEventArgs mo = new MouseEventArgs(mb, 1, po.X, po.Y, 0);
                    Automation.Action ac = new Automation.Action(t, ar[i].ElementAt(7).First.ToString(), ar[i].ElementAt(8).First.ToString(), ind, ar[i].ElementAt(4).First.ToString(), ar[i].ElementAt(5).First.ToString(), ar[i].ElementAt(6).First.ToString(),mo);
                    rec.List.Add(ac);
                }
            }
            return rec;
        }
        /// This function opens a recording file and adds it to the Automation.Recordings dictionary
        /// 
        /// @param file The file path to the recording
        public void OpenRecording(string file)
        {
            Automation.Recording rec = OpenRec(file);
            Automation.Recordings.Add(rec.Name,rec);
            InitElements();
        }
        /// This function opens a recording file and adds it to the Properties collection
        /// 
        /// @param file The file path of the recording
        public void OpenProperty(string file)
        {
            Automation.Recording rec = OpenRec(file);
            Automation.Properties.Add(rec.Name,rec);
            InitElements();
        }

        /// It opens a file dialog box and if the user selects a file, it opens the file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the recording.
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openRecDialog.InitialDirectory = Application.StartupPath + "\\Recordings";
            if (openRecDialog.ShowDialog() != DialogResult.OK)
                return;
            OpenRecording(openRecDialog.FileName);
        }

/// It deletes the selected node
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The event arguments.
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node n = (Node)view.SelectedNode.Tag;
            if(n.type == Node.Type.recording)
            {
                Automation.Recordings.Remove(n.recording.Name);
                File.Delete(n.recording.File);
                InitElements();
            }
            else if (n.type == Node.Type.action)
            {
                n.recording.List.Remove(n.action);
                UpdateElements();
            }
        }

        /// It's a function that refreshes the elements in the listbox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitElements();
        }

        /// The function starts the property recording by calling the StartPropertyRecording() function
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        /// 
        /// @return The return value is a string.
        private void startPropBut_Click(object sender, EventArgs e)
        {
            TextInput ti = new TextInput("Property" + propBox.Items.Count);
            ti.Text = "Set Property Name";
            if (ti.ShowDialog() != DialogResult.OK)
                return;
            propRecStatusLabel.Text = "Property Recording: Started";
            Automation.StartPropertyRecording(ti.Text);
        }

/// If the user clicks the stop button, the label will change to "Property Recording: Stopped" and if
/// the recording is not stopped, it will stop the recording. If the recording has more than 0
/// properties, it will initialize the elements
/// 
/// @param sender The object that raised the event.
/// @param EventArgs 
/// 
/// @return The return value is a list of the properties that were recorded.
        private void stopPropBut_Click(object sender, EventArgs e)
        {
            propRecStatusLabel.Text = "Property Recording: Stopped";
            if (!Automation.IsRecording)
                return;
            Automation.StopPropertyRecording();
            if (Automation.Properties.Count > 0)
            {
                //actions = Automation.Recordings[0].List;
                InitElements();
            }
        }

        /// If the selected node is not an action, then get the property of the node and put it on the
        /// clipboard
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The value of the property.
        private void getPropBut_Click(object sender, EventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.action)
                return;
            if (n.recording.List.Last().Value == Automation.Action.ValueType.Image)
                Clipboard.SetImage((Bitmap)Automation.GetProperty(n.recording.Name));
            else
            {
                string s = (string)Automation.GetProperty(n.recording.Name);
                MessageBox.Show(s);
            }
        }
        /// It takes the text from the textbox and sets the property of the selected node to that text
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The text value of the textbox in the form.
        private void setPropBut_Click(object sender, EventArgs e)
        {
            TextInput ti = new TextInput("Property" + propBox.Items.Count);
            ti.Text = "Set Text To Set";
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.action)
                return;
            Automation.SetProperty(n.recording.Name, ti.TextValue);
        }
        /// It saves the selected property to a file
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the file that was selected.
        private void saveSelectedValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            savePropDialog.InitialDirectory = Application.StartupPath + "\\Recordings";
            if (savePropDialog.ShowDialog() != DialogResult.OK)
                return;
            SaveProperty(savePropDialog.FileName);
        }

/// It opens a file dialog box and then opens the file that the user selected
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The event arguments.
/// 
/// @return The file name of the selected file.
        private void openPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openPropDialog.InitialDirectory = Application.StartupPath + "\\Recordings";
            if (openPropDialog.ShowDialog() != DialogResult.OK)
                return;
            OpenProperty(openPropDialog.FileName);
        }

        /// The function is called when the user clicks on the "Get" menu item in the context menu
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void getMenuItem_Click(object sender, EventArgs e)
        {
            getPropBut.PerformClick();
        }

        /// It deletes the selected node from the tree view
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void deletePropMenuItem_Click(object sender, EventArgs e)
        {
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
            {
                Automation.Properties.Remove(n.recording.Name);
                if(n.recording.File!=null)
                File.Delete(n.recording.File);
                InitElements();
            }
            else if (n.type == Node.Type.action)
            {
                n.recording.List.Remove(n.action);
                UpdateElements();
            }
        }

        
        /// If the selected node is not a recording, set the selected index of the property box to the
        /// value of the selected node's action
        /// 
        /// @param sender The object that raised the event.
        /// @param TreeViewEventArgs
        /// https://msdn.microsoft.com/en-us/library/system.windows.forms.treevieweventargs(v=vs.110).aspx
        /// 
        /// @return The selected node's tag.
        private void propView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            propBox.SelectedIndex = (int)n.action.Value;
        }

        /// When the user changes the value of the dropdown box, the value of the selected node's action
        /// is changed to the value of the dropdown box
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The selected item in the ComboBox.
        private void propBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            n.action.Value = (Automation.Action.ValueType)propBox.SelectedItem;
        }

        /// It moves the selected node up one position in the list
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The index of the selected node.
        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (view.SelectedNode == null)
                return;
            Node n = (Node)view.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            int oldindex = n.recording.List.IndexOf(n.action);
            n.recording.List.RemoveAt(oldindex);
            int newindex = oldindex - 1;
            if (newindex > oldindex) newindex--;
            // the actual index could have shifted due to the removal
            n.recording.List.Insert(newindex, n.action);
            UpdateElements();
        }

        /// It removes the selected node from the list, then inserts it back into the list at the index
        /// above it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The index of the selected node.
        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (view.SelectedNode == null)
                return;
            Node n = (Node)view.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            int oldindex = n.recording.List.IndexOf(n.action);
            n.recording.List.RemoveAt(oldindex);
            int newindex = oldindex - 1;
            // the actual index could have shifted due to the removal
            n.recording.List.Insert(newindex, n.action);
            UpdateElements();
        }

        /// It removes the selected action from the list of actions, then inserts it back into the list
        /// at the index above the original index
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The selected node.
        private void moveUpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            int oldindex = n.recording.List.IndexOf(n.action);
            n.recording.List.RemoveAt(oldindex);
            int newindex = oldindex - 1;
            // the actual index could have shifted due to the removal
            n.recording.List.Insert(newindex, n.action);
            UpdateElements();
        }

        /// It moves the selected action down one position in the list of actions.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The selected node.
        private void moveDownToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            int oldindex = n.recording.List.IndexOf(n.action);
            n.recording.List.RemoveAt(oldindex);
            int newindex = (oldindex) + 1;
            // the actual index could have shifted due to the removal
            n.recording.List.Insert(newindex, n.action);
            UpdateElements();
        }

        /// If the selected node is not null, and the node is not a recording, then set the action value
        /// to the selected item in the property box
        /// 
        /// @param sender System.Object
        /// @param EventArgs 
        /// 
        /// @return The selected item in the property box.
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (propView.SelectedNode == null)
                return;
            Node n = (Node)propView.SelectedNode.Tag;
            if (n.type == Node.Type.recording)
                return;
            n.action.Value = (Automation.Action.ValueType)propBox.SelectedItem;
        }
    }
}
