using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScottPlot;
using AForge;
namespace BioCore
{
    public partial class Plot : Form
    {
        public ScottPlot.Plot plot;
        string file;
        string name;
        System.Drawing.Bitmap bitmap;
        List<double[]> data = new List<double[]>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<double[]> Data
        {
            get { return data; }
            set { data = value; UpdateImage(); }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Bitmap Image
        {
            get { return bitmap; }
            set { bitmap = value; }
        }
        public Plot()
        {
            InitializeComponent();
            UpdateImage();
            name = DateTime.Now.Ticks.ToString();
            this.Show();
        }
        public Plot(double[] vals, string name)
        {
            InitializeComponent();
            data.Add(vals);
            UpdateImage();
            this.name = name;
            this.Show();
        }

        public void UpdateImage()
        {
            /*
            plot = new ScottPlot.Plot();
            foreach (double[] val in data) 
            {
                Bar b = new Bar();
                b.Value = val;

                plot.Add.Bar(val);
            }
            file = plot.SaveFig(name + ".png");
            */
            this.Text = name;
            bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(file);
            pictureBox.Image = bitmap;
        }

        private void Plot_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Plot_ResizeEnd(object sender, EventArgs e)
        {
            UpdateImage();
        }
    }
}
