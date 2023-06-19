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

namespace Bio
{
    public partial class Plot : Form
    {
        public ScottPlot.Plot plot;
        string file;
        Bitmap bitmap;
        List<double[]> data = new List<double[]>();
        public List<double[]> Data
        {
            get { return data; }
            set { data = value; UpdateImage(); }
        }
        public Bitmap Image
        {
            get { return bitmap; }
            set { bitmap = value; }
        }
        public Plot()
        {
            InitializeComponent();
            UpdateImage();
            this.Show();
        }
        public Plot(double[] vals)
        {
            InitializeComponent();
            data.Add(vals);
            UpdateImage();
            this.Show();
        }

        public void UpdateImage()
        {
            plot = new ScottPlot.Plot(Width, Height);
            foreach (double[] val in data) 
            {
                plot.AddBar(val);
            }
            file = plot.SaveFig(DateTime.Now.Ticks.ToString() + ".png");
            bitmap = (Bitmap)Bitmap.FromFile(file);
            pictureBox.Image = bitmap;
        }

        private void Plot_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.Delete(file);
        }

        private void Plot_ResizeEnd(object sender, EventArgs e)
        {
            UpdateImage();
        }
    }
}
