namespace BioCore
{
    public partial class ImageWindow : Form
    {
        /* Creating a new window with the image. */
        public ImageWindow(BioImage im)
        {
            InitializeComponent();
            ImageView iv = new ImageView(im);
            iv.Dock = DockStyle.Fill;
            this.Controls.Add(iv);
            this.Text = System.IO.Path.GetFileName(im.ID);
            this.Size = new Size(im.SizeX, im.SizeY);
            this.Show();
        }
    }
}
