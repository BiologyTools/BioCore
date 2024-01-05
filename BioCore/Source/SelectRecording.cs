using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BioCore
{
    public partial class SelectRecording : Form
    {
        /* Adding the items in the Automation.Properties.Values and Automation.Recordings.Values to the
        propsBox and recsBox. */
        public SelectRecording()
        {
            InitializeComponent();
            foreach (var item in Automation.Properties.Values)
            {
                propsBox.Items.Add(item);
            }
            foreach (var item in Automation.Recordings.Values)
            {
                recsBox.Items.Add(item);
            }
        }
        private Automation.Recording rec = null;
        /* A property that returns the value of the variable rec. */
        public Automation.Recording Recording
        {
            get
            {
                return rec;
            }
        }

        /// When the user selects a recording from the dropdown list, the recording is assigned to the
        /// variable rec
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void recsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            rec = (Automation.Recording)recsBox.SelectedItem;
        }

        /// When the user selects a recording from the dropdown menu, the recording is assigned to the
        /// variable rec
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void propsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            rec = (Automation.Recording)propsBox.SelectedItem;
        }

        /// The function is called when the user clicks the OK button
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void okBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
