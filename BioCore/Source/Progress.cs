using System.Diagnostics;

namespace BioCore
{
    public partial class Progress : Form
    {
        Stopwatch watch = new Stopwatch();
        /* A constructor. */
        public Progress(string file, string status)
        {
            InitializeComponent();
            watch.Start();
            statusLabel.Text = status;
            fileLabel.Text = file;
            timer.Start();
        }
       /// The function takes an integer as a parameter and sets the value of the progress bar to that
       /// integer
       /// 
       /// @param p The progress value.
        public void UpdateProgress(int p)
        {
            progressBar.Value = p;
        }
        /// If the progress is greater than 1, set the progress bar to 100, otherwise set the progress
        /// bar to the progress times 100
        /// 
        /// @param p The progress value.
        public void UpdateProgressF(float p)
        {
            if (p > 1)
                progressBar.Value = 100;
            else
                progressBar.Value = (int)(p * 100);
        }

        /// The timer_Tick function is called every time the timer ticks, and it updates the timeLabel
        /// to show the current time
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void timer_Tick(object sender, EventArgs e)
        {
            timeLabel.Text = watch.Elapsed.Seconds + "." + watch.Elapsed.Milliseconds + "s";
        }

        public string Status 
        {  
            get { return statusLabel.Text; } 
            set { statusLabel.Text = value; }
        }
    }
}
