using System.Diagnostics;

namespace Bio
{
    public partial class Progress : Form
    {
        Stopwatch watch = new Stopwatch();
        public Progress(string file, string status)
        {
            InitializeComponent();
            watch.Start();
            statusLabel.Text = status;
            fileLabel.Text = file;
            timer.Start();
        }
        public void UpdateProgress(int p)
        {
            progressBar.Value = p;
        }
        public void UpdateProgressF(float p)
        {
            if (p > 1)
                progressBar.Value = 100;
            else
                progressBar.Value = (int)(p * 100);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timeLabel.Text = watch.Elapsed.Seconds + "." + watch.Elapsed.Milliseconds + "s";
        }
    }
}
