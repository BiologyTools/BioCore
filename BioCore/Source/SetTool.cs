namespace Bio
{
    public partial class SetTool : Form
    {

        /* Initializing the form. */
        public SetTool()
        {
            InitializeComponent();
            timer.Start();
            foreach (Scripting.Script tool in Scripting.Scripts.Values)
            {
                if (tool.type == Scripting.ScriptType.tool)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = tool.name;
                    item.Tag = tool;
                    toolView.Items.Add(item);
                }
            }
        }
        public Scripting.Script Script
        {
            get { return (Scripting.Script)toolView.SelectedItems[0].Tag; }
        }
        /// This function is called when the user clicks the "Set Tool" button.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void setToolBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

        }
/// If the script is running, stop it, then run it again
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The event arguments.
        private void toolView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Script.thread != null)
                Script.Stop();
            Script.Run();
        }

        /// > Every second, update the text of each item in the list view to the string representation
        /// of the script object that the item is associated with
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void timer_Tick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in toolView.Items)
            {
                item.Text = ((Scripting.Script)item.Tag).ToString();
            }
        }

        /// If the script is running, stop it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data,
        /// and provides a value to use for events that do not include event data.
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Script.thread != null)
                Script.Stop();
        }
    }
}
