using AForge;
using AForge.Imaging.Filters;
using BioLib;
using CSScripting;
using Gtk;
using OpenSlideGTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Application = System.Windows.Forms.Application;
using Image = System.Drawing.Image;
using Keys = System.Windows.Forms.Keys;
using RectangleD = AForge.RectangleD;
using SizeF = AForge.SizeF;

namespace BioCore
{

    /// <summary>
    /// ImageView control for image stacks, pyramidal and whole-slide-images.
    /// </summary>
    public partial class ImageView : UserControl, IDisposable
    {
        GLWindow window;
        SlideGLArea gl = new SlideGLArea();
        SlideRenderer slideRenderer;
        ///Initializing the image viewer. */
        public ImageView(BioImage im, bool OpenWithOpenSlide = true)
        {
            string file = im.ID.Replace("\\", "/");
            InitializeComponent();
            InitializeViewport();

            if (im.isPyramidal)
            {
                gl.Size = new System.Drawing.Size(800, 600);
                if (im.openSlideImage != null && OpenWithOpenSlide)
                {
                    openSlide = true;
                    Initialize(im.file, im);
                }
                else
                    Initialize(im.file, im);
                hScrollBar.Visible = true;
                vScrollBar.Visible = true;
            }
            else
            {
                hScrollBar.Visible = false;
                vScrollBar.Visible = false;
                gl.Width += 18;
                gl.Height += 18;
                Tools.selectBoxSize = ROI.selectBoxSize;
            }
            App.viewer = this;
            if (im.SlideBase != null && im.isPyramidal && OpenSlide)
                openSlide = true;
            serie = im.series;
            Dock = DockStyle.Fill;
            AddImage(im);
            App.viewer = this;
            if (file == "" || file == null)
                return;
            InitGUI();
            // Preserve a loader-selected initial plane (for example the middle
            // slice of an NGFF scene). SetCoordinate is asynchronous and raced
            // construction here, so initialise the controls synchronously.
            zBar.Value = Math.Clamp(im.Coordinate.Z, zBar.Minimum, zBar.Maximum);
            cBar.Value = Math.Clamp(im.Coordinate.C, cBar.Minimum, cBar.Maximum);
            tBar.Value = Math.Clamp(im.Coordinate.T, tBar.Minimum, tBar.Maximum);
            gl.Paint += Gl_Paint;
            MouseWheel += new System.Windows.Forms.MouseEventHandler(ImageView_MouseWheel);
            zBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(ZTrackBar_MouseWheel);
            cBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(CTrackBar_MouseWheel);
            tBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(TimeTrackBar_MouseWheel);
            //We set the trackbar event to handled so that it only scrolls one tick not the default multiple.
            zBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            tBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            cBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            this.SizeChanged += ImageView_SizeChanged;
            this.KeyDown += ImageView_KeyDown;
            this.KeyPress += ImageView_KeyPress;
            this.KeyUp += ImageView_KeyUp;
            TimeFps = 60;
            ZFps = 60;
            CFps = 1;
            resolutions = im.Resolutions;
            if (im.isPyramidal)
            {
                // Set the source based on image type
                if (im.OpenSlideBase != null)
                {
                    slideRenderer.SetSource(im.OpenSlideBase);
                }
                else if (im.SlideBase != null)
                {
                    slideRenderer.SetSource(im.SlideBase);
                }
            }
            InitGL();
            update = true;
            InitPreview();
            GoToImage();
            UpdateImages();
            Mode = ViewMode.Filtered;
            UpdateView();
            Initialize(im.ID, im);
        }
        ///Initializing the ImageView class. */
        public ImageView()
        {
            InitializeComponent();
            InitializeViewport();
            InitGL();
            Dock = DockStyle.Fill;
            App.viewer = this;
            SetCoordinate(0, 0, 0);
            InitGUI();
            gl.Paint += Gl_Paint;
            //Buf = image.GetBufByCoord(GetCoordinate());
            MouseWheel += new System.Windows.Forms.MouseEventHandler(ImageView_MouseWheel);
            zBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(ZTrackBar_MouseWheel);
            cBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(CTrackBar_MouseWheel);
            tBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(TimeTrackBar_MouseWheel);
            this.SizeChanged += ImageView_SizeChanged;
            //We set the trackbar event to handled so that it only scrolls one tick not the default multiple.
            zBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            tBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            cBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            TimeFps = 60;
            ZFps = 60;
            CFps = 1;
            update = true;
            UpdateImages();
            GoToImage();
            UpdateView();
        }

        private void InitializeViewport()
        {
            gl.Dock = DockStyle.Fill;
            gl.Visible = false;
            gl.ContextMenuStrip = contextMenuStrip;
            viewport.ContextMenuStrip = contextMenuStrip;
            contextMenuStrip.Opening += contextMenuStrip_Opening;
            viewport.Controls.Add(gl);
            viewpanel.BringToFront();
            InitializeOverview();
            // Both rendering surfaces must forward gestures to the same viewer handlers.
            foreach (System.Windows.Forms.Control surface in new System.Windows.Forms.Control[] { viewpanel, gl })
            {
                surface.MouseDown += pictureBox_MouseDown;
                surface.MouseMove += rgbPictureBox_MouseMove;
                surface.MouseUp += pictureBox_MouseUp;
                surface.MouseWheel += ImageView_MouseWheel;
            }
        }

        private void UpdateImageSurface()
        {
            bool pyramidal = Images.Count > SelectedIndex && Images[SelectedIndex].isPyramidal;
            gl.Visible = pyramidal;
            viewpanel.Visible = !pyramidal;
            if (pyramidal)
                gl.BringToFront();
            else
                viewpanel.BringToFront();
            UpdateOverviewVisibility();
        }

        private void InvalidateView()
        {
            UpdateOverviewVisibility();
            overviewPanel.Invalidate();
            UpdateScrollBars();
            if (Images.Count > SelectedIndex && Images[SelectedIndex].isPyramidal)
                gl.Invalidate();
            else
                viewpanel.Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (viewport == null || statusPanel == null || trackBarPanel == null ||
                hScrollBar == null || vScrollBar == null)
                return;

            int top = statusPanel.Visible ? statusPanel.Bottom : 0;
            int bottom = trackBarPanel.Visible ? trackBarPanel.Top : ClientSize.Height;
            int width = Math.Max(0, ClientSize.Width - (vScrollBar.Visible ? vScrollBar.Width : 0));
            int height = Math.Max(0, bottom - top - (hScrollBar.Visible ? hScrollBar.Height : 0));

            // Hidden scrollbars must not leave unused strips around the image surface.
            viewport.Bounds = new System.Drawing.Rectangle(0, top, width, height);
            hScrollBar.SetBounds(0, top + height, width, hScrollBar.Height);
            vScrollBar.SetBounds(width, top, vScrollBar.Width, height);
            UpdateScrollBars();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            const int WM_CHAR = 0x0102;

            switch (msg.Msg)
            {
                case WM_KEYDOWN:
                    ImageView_KeyDown(this, new KeyEventArgs(keyData));
                    break;

                case WM_KEYUP:
                    ImageView_KeyUp(this, new KeyEventArgs(keyData));
                    break;

                case WM_CHAR:
                    char ch = (char)msg.WParam;
                    ImageView_KeyPress(this, new System.Windows.Forms.KeyPressEventArgs(ch));
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ImageView_SizeChanged(object? sender, EventArgs e)
        {
            if (SelectedImage != null)
                if (SelectedImage.isPyramidal)
                    SelectedImage.PyramidalSize = new AForge.Size(gl.Width, gl.Height);
        }

        private void InitGL()
        {
            var gameWindowSettings = new GameWindowSettings()
            {
                UpdateFrequency = 60,
            };
            NativeWindowSettings nativeSettings;

            nativeSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(Width, Height),
                Location = new Vector2i(0, 0),
                Title = "OpenGL Context",
                StartVisible = false,  // Window starts hidden
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)
            };
            window = new GLWindow(gameWindowSettings, nativeSettings);
            GLFWProvider.SetErrorCallback((error, description) =>
            {
                Console.WriteLine($"BioImager GLFW Error {error}: {description}");
            });
        }

        private void Gl_Paint(object? sender, PaintEventArgs e)
        {
            //Render(((SlideGLArea)sender).CreateGraphics());
        }

        ~ImageView()
        {

        }
        public struct Layer
        {
            public double Resolution
            {
                get; set;
            }
            public bool Enabled { get; set; }
            public Layer(double d)
            {
                Enabled = true;
                Resolution = d;
            }
            public Layer(double d, bool enabled)
            {
                Enabled = enabled;
                Resolution = d;
            }
        }

        public List<Layer> Layers = new List<Layer>();
        private Layers layersTool = new Layers();
        private bool IsLayerEnabled(double d)
        {
            foreach (Layer item in Layers)
            {
                if (d == item.Resolution && item.Enabled)
                    return true;
            }
            return false;
        }
        List<System.Drawing.Bitmap> Bitmaps = new List<System.Drawing.Bitmap>();
        List<Resolution> resolutions = new List<Resolution>();
        public static List<ROI> selectedAnnotations = new List<ROI>();
        private static int selectedIndex = 0;
        public static BioImage SelectedImage
        {
            get
            {
                if (App.viewer != null)
                {
                    if (App.viewer.Images.Count > 0)
                        return App.viewer.Images[selectedIndex];
                    else
                        return null;
                }
                else
                    return null;
            }
            set
            {
                if (App.viewer.Images.Count > 0)
                    App.viewer.Images[selectedIndex] = value;
                else
                    App.viewer.Images.Add(value);
            }
        }
        public static Bitmap SelectedBuffer
        {
            get
            {
                int ind = SelectedImage.Coords[SelectedImage.Coordinate.Z, SelectedImage.Coordinate.C, SelectedImage.Coordinate.T];
                return SelectedImage.Buffers[ind];
            }
        }
        public static PointD mouseDown;
        public static bool down;
        public static PointD mouseUp;
        public static bool up;
        public static bool Ctrl
        {
            get
            {
                return Win32.GetKeyState(Keys.LControlKey);
            }
        }
        private bool x1State = false;
        private bool x2State = false;
        public static MouseButtons mouseUpButtons;
        public static MouseButtons mouseDownButtons;
        private PointD pd;
        public static bool showBounds = true;
        public static bool showText = true;
        public Image Buf = null;
        public bool init = false;
        private bool update = false;
        private bool updateOverlay = false;
        private List<BioImage> imgs = new List<BioImage>();
        public List<BioImage> Images
        {
            get 
            {
                return imgs;
            }
        }
        public System.Drawing.Bitmap overviewImage = null;
        /// <summary>
        /// Called by SlideGLArea during the Skia rendering phase.
        /// Draws annotations on top of the GL-rendered tiles.
        /// </summary>
        private void Render(System.Drawing.Graphics canvas)
        {
            try
            {
                if (Images.Count == 0)
                    return;
                // Draw annotations (ROIs) - keep existing annotation drawing code
                int i = 0;
                foreach (BioImage im in Images)
                {
                    List<ROI> rois = new List<ROI>();
                    rois.AddRange(im.Annotations);
                    int ri = 0;
                    ZCT co = GetCoordinate();
                    canvas.TranslateTransform((float)gl.Width / 2f, (float)gl.Height / 2f);
                    RectangleD rp = ToScreenSpace(new RectangleD(im.StageSizeX * im.PhysicalSizeX, im.StageSizeX * im.PhysicalSizeY, im.SizeX * im.PhysicalSizeX, im.SizeY * im.PhysicalSizeY));
                    UpdateImages();
                    if (!im.isPyramidal)
                        canvas.DrawImage(Bitmaps[i], new System.Drawing.PointF((float)rp.X, (float)rp.Y));
                    i++;
                }
                System.Drawing.Rectangle rf = new System.Drawing.Rectangle((int)canvas.Clip.GetBounds(canvas).X, (int)canvas.Clip.GetBounds(canvas).Y,
                   (int)canvas.Clip.GetBounds(canvas).Width, (int)canvas.Clip.GetBounds(canvas).Height);

                // Plugin rendering hook
                Plugins.Render(this, new PaintEventArgs(canvas, rf));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Annotation render error: {ex.Message}");
            }
        }

        ///A property that is used to set the selected index of the image. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get;
            set
            {
                if (value >= 0 && Images.Count > 0)
                {
                    field = value;
                    selectedIndex = field;
                    InitGUI();
                }
            }
        } = 0;
        public Tools tools { get { return App.tools; } }
        public string filepath = "";
        public int serie = 0;
        public int minSizeX = 50;
        public int minSizeY = 20;
        public bool loopZ = true;
        public bool loopT = true;
        public bool loopC = true;
        private AForge.Rectangle overview;
        public Rectangle OverView
        {
            get { return overview; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStage { get; set; }
        private double pxWmicron = 0.004;
        private double pxHmicron = 0.004;
        AForge.SizeF scale = new AForge.SizeF(1, 1);
        /// > Sets the coordinate of the image to the specified Z, C, and T values
        /// 
        /// @param z the z-coordinate of the image
        /// @param c channel
        /// @param t time
        /// 
        /// @return The method is returning the value of the zBar.Value, cBar.Value, and tBar.Value.
        public void SetCoordinate(int z, int c, int t)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    App.viewer.Invoke((MethodInvoker)delegate
                    {
                        if (SelectedImage == null)
                            return;
                        if (z >= SelectedImage.SizeZ)
                            zBar.Value = zBar.Maximum;
                        if (c >= SelectedImage.SizeC)
                            cBar.Value = cBar.Maximum;
                        if (t >= SelectedImage.SizeT)
                            tBar.Value = tBar.Maximum;
                        zBar.Value = z;
                        cBar.Value = c;
                        tBar.Value = t;
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            });
        }
        /// It returns the coordinate of the selected image
        /// 
        /// @return The Coordinate property of the SelectedImage object.
        public ZCT GetCoordinate()
        {
            if (SelectedImage == null)
                return new ZCT();
            return SelectedImage.Coordinate;
        }
        /// This function adds an image to the list of images, and then updates the GUI to reflect the
        /// new image
        /// 
        /// @param BioImage This is a class that contains the image data, and some other information
        /// about the image.
        public void AddImage(BioImage im)
        {
            ZarrChannelView.InitializeChannels(im);
            if (Images.Contains(im))
                return;
            else
                Images.Add(im);
            SelectedIndex = Images.Count - 1;
            bool c = false;
            foreach (var item in Layers)
            {
                if (item.Resolution == im.PhysicalSizeX)
                    c = true;
            }
            if (!c)
                Layers.Add(new Layer(im.PhysicalSizeX));
            if (im.isPyramidal)
            {
                hScrollBar.Visible = true;
                vScrollBar.Visible = true;
                InitPreview();
            }
            else
            {
                hScrollBar.Visible = false;
                vScrollBar.Visible = false;
                gl.Width += 18;
                gl.Height += 18;
                Tools.selectBoxSize = ROI.selectBoxSize;
            }
            Initialize(im.ID, im);
            InitGUI();
            UpdateImages();
            GoToImage(Images.Count - 1);
        }
        ///Declaring a variable called showControls and setting it to true. */
        private bool showControls = true;
        ///Setting the visibility of the trackBarPanel. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowControls
        {
            get { return trackBarPanel.Visible; }
            set
            {
                showControls = value;
                if (!value)
                {
                    trackBarPanel.Hide();
                    if (ShowStatus)
                    {
                        gl.Top = 25;
                        gl.Height += 75;
                    }
                    else
                    {
                        gl.Top = 0;
                        gl.Height += 75;
                    }
                    hideControlsToolStripMenuItem1.Text = "Show Controls";
                }
                else
                {
                    trackBarPanel.Show();
                    if (ShowStatus)
                    {
                        gl.Top = 25;
                        gl.Height -= 75;
                    }
                    else
                    {
                        gl.Top = 0;
                        gl.Height += 75;
                    }

                    //panel.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
                    hideControlsToolStripMenuItem1.Text = "Hide Controls";
                }
                UpdateView();
            }
        }
        ///Declaring a variable called showStatus and setting it to true. */
        private bool showStatus = true;
        ///Setting the value of the property ShowStatus. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatus
        {
            get { return showStatus; }
            set
            {
                showStatus = true;
                if (!value)
                {
                    statusPanel.Hide();
                    gl.Top = 0;
                    gl.Height += 25;
                    showControlsToolStripMenuItem.Visible = true;
                    hideControlsToolStripMenuItem.Text = "Show Status";
                }
                else
                {
                    statusPanel.Show();
                    statusPanel.Visible = value;
                    gl.Top = 25;
                    showControlsToolStripMenuItem.Visible = false;
                    hideControlsToolStripMenuItem.Text = "Hide Status";
                }
            }
        }
        /// <summary>
        /// Only used when opening stacks.
        /// </summary>

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double PxWmicron
        {
            get
            {
                return pxWmicron;
            }
            set
            {
                pxWmicron = value;
            }
        }
        /// <summary>
        /// Only used when opening stacks.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double PxHmicron
        {
            get
            {
                return pxHmicron;
            }
            set
            {
                pxHmicron = value;
            }
        }
        public enum ViewMode
        {
            Raw,
            Filtered,
            RGBImage,
            Emission,
        }
        private ViewMode viewMode = ViewMode.Filtered;
        ///Setting the view mode of the application. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ViewMode Mode
        {
            get
            {
                return viewMode;
            }
            set
            {
                if (Mode == value)
                    return;
                viewMode = value;
                //If view mode is changed we update.
                update = true;
                UpdateImages();
                App.tabsView.UpdateViewMode(viewMode);
                UpdateView();
                UpdateOverlay();
                if (viewMode == ViewMode.RGBImage)
                {
                    cBar.Value = 0;
                    rgbBoxsPanel.BringToFront();
                    cBar.SendToBack();
                    cLabel.SendToBack();
                    gl.BringToFront();
                }
                else
                if (viewMode == ViewMode.Filtered)
                {
                    rgbBoxsPanel.SendToBack();
                    cBar.BringToFront();
                    cLabel.BringToFront();
                    gl.BringToFront();
                }
                else
                if (viewMode == ViewMode.Raw)
                {
                    rgbBoxsPanel.SendToBack();
                    cBar.BringToFront();
                    cLabel.BringToFront();
                    gl.BringToFront();
                }
                else
                {
                    cBar.Value = 0;
                    rgbBoxsPanel.BringToFront();
                    cBar.SendToBack();
                    cLabel.SendToBack();
                    gl.BringToFront();
                }
            }
        }
        ///A property that returns the filepath. */
        public string Path
        {
            get
            {
                return filepath;
            }
        }
        ///A property that returns the R channel of the selected image. */
        public Channel RChannel
        {
            get
            {
                return SelectedImage.Channels[SelectedImage.rgbChannels[0]];
            }
        }
        ///A property that returns the G channel of the image. */
        public Channel GChannel
        {
            get
            {
                return SelectedImage.Channels[SelectedImage.rgbChannels[1]];
            }
        }
        ///Getting the B channel of the image. */
        public Channel BChannel
        {
            get
            {
                return SelectedImage.Channels[SelectedImage.rgbChannels[2]];
            }
        }
        PointD origin = new PointD(0, 0);
        bool hardwareAcceleration = true;
        bool openSlide = false;
        public bool OpenSlide
        {
            get { return openSlide; }
        }
        public int? MacroResolution { get { return SelectedImage.MacroResolution; } }
        public int? LabelResolution { get { return SelectedImage.LabelResolution; } }
        public int ViewWidth
        {
            get
            {
                return viewport.ClientSize.Width;
            }
        }
        public int ViewHeight
        {
            get
            {
                return viewport.ClientSize.Height;
            }
        }
        ///A property of the class PointD. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD Origin
        {
            get { return origin; }
            set
            {
                origin = value;
                update = true;
                UpdateView();
            }
        }
        ///Setting the value of the pyramidalOrigin variable. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD PyramidalOrigin
        {
            get
            {
                if (SelectedImage == null)
                    return new PointD(0, 0);
                return SelectedImage.PyramidalOrigin;
            }
            set
            {
                SelectedImage.PyramidalOrigin = value;
            }
        }
        ///A property of the class ImageViewer. It is a getter and setter for the resolution of the image.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Resolution
        {
            get
            {
                if (SelectedImage == null)
                    return 1;
                return SelectedImage.Resolution;
            }
            set
            {
                double dp = Resolution / value;
                SelectedImage.PyramidalOrigin = new PointD((dp * PyramidalOrigin.X), (dp * PyramidalOrigin.Y));
                SelectedImage.Resolution = value;
                if (SelectedImage.Type == BioImage.ImageType.well)
                    SelectedImage.Level = (int)value;
                updatePyr = true;
            }
        }
        /// <summary>
        /// Current level when viewing whole-slide and pyramidal images.
        /// </summary>
        public int Level
        {
            get
            {
                if (SelectedImage != null)
                    return SelectedImage.Level;
                else
                    return 0;
            }
        }
        /// <summary>
        /// Scale when opening image stacks.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AForge.SizeF Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                update = true;
                UpdateView();
                if (SelectedImage != null)
                    if (SelectedImage.isPyramidal)
                    {
                        Tools.selectBoxSize = (float)(ROI.selectBoxSize * Resolution);
                    }
                    else
                        Tools.selectBoxSize = ROI.selectBoxSize;
            }
        }
        ///A property that returns a ContextMenuStrip object. */
        public ContextMenuStrip ViewContextMenu
        {
            get
            {
                return contextMenuStrip;
            }
        }
        /// "If the user has selected a channel, then set the channel to the selected index, otherwise
        /// set it to 0."
        /// 
        /// The above function is called when the user changes the channel selection
        private readonly bool[] rgbEnabled = { true, true, true };
        private bool initializingRgbSelectors;
        private BioImage rgbSelectorImage;

        private int[] SelectedRgbChannels(BioImage image) => Enumerable.Range(0, 3)
            .Select(i => rgbEnabled[i] ? image.rgbChannels[i] : -1).ToArray();

        public void UpdateRGBChannels()
        {
            if (SelectedImage == null) return;
            System.Windows.Forms.ComboBox[] boxes = { channelBoxR, channelBoxG, channelBoxB };
            for (int i = 0; i < boxes.Length; i++)
            {
                rgbEnabled[i] = boxes[i].SelectedIndex > 0;
                if (rgbEnabled[i]) SelectedImage.rgbChannels[i] = boxes[i].SelectedIndex - 1;
            }
        }
        private bool timeEnabled = false;
        private int zfps;
        ///Setting the interval of the timer to the value of the fps. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZFps
        {
            get
            {
                return zfps;
            }
            set
            {
                zfps = value;
                float f = value;
                zTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }
        private int timefps;
        ///Setting the interval of the timer to the value of the timefps variable. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TimeFps
        {
            get
            {
                return timefps;
            }
            set
            {
                timefps = value;
                float f = value;
                timelineTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }
        private int cfps;
        ///Setting the interval of the timer to the inverse of the fps. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CFps
        {
            get
            {
                return cfps;
            }
            set
            {
                cfps = value;
                float f = value;
                cTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }
        /// It initializes the GUI
        /// 
        /// @return The image is being returned.
        public void InitGUI()
        {
            if (SelectedImage == null)
                return;
            zBar.Maximum = SelectedImage.SizeZ - 1;
            if (!SelectedImage.IsZarrSource && SelectedImage.Buffers[0].RGBChannelsCount == 3)
                cBar.Maximum = 0;
            else
                cBar.Maximum = SelectedImage.SizeC - 1;
            if (SelectedImage.SizeT > 1)
            {
                tBar.Maximum = SelectedImage.SizeT - 1;
                timeEnabled = true;
            }
            else
            {
                tBar.Enabled = false;
                timeEnabled = false;
                tBar.Maximum = SelectedImage.SizeT - 1;
            }
            //rgbPictureBox.Image = image.plane.GetBitmap();
            //we clear the channel comboboxes incase we have channels from previous loaded image.
            bool sameImage = ReferenceEquals(rgbSelectorImage, SelectedImage);
            rgbSelectorImage = SelectedImage;
            bool[] previousEnabled = (bool[])rgbEnabled.Clone();
            int[] previousChannels = (int[])SelectedImage.rgbChannels.Clone();
            initializingRgbSelectors = true;
            channelBoxR.Items.Clear();
            channelBoxG.Items.Clear();
            channelBoxB.Items.Clear();

            channelBoxR.Items.Add("");
            channelBoxG.Items.Add("");
            channelBoxB.Items.Add("");
            foreach (Channel ch in SelectedImage.Channels)
            {
                channelBoxR.Items.Add(ch);
                channelBoxG.Items.Add(ch);
                channelBoxB.Items.Add(ch);
            }
            System.Windows.Forms.ComboBox[] rgbBoxes = { channelBoxR, channelBoxG, channelBoxB };
            int count = SelectedImage.Channels.Count;
            for (int i = 0; i < rgbBoxes.Length; i++)
                rgbBoxes[i].SelectedIndex = count == 0 || (sameImage && !previousEnabled[i]) ? 0 :
                    (sameImage ? Math.Clamp(previousChannels[i], 0, count - 1) : (i < count ? i : 0)) + 1;
            initializingRgbSelectors = false;
            UpdateRGBChannels();

            init = true;
        }
        /// This function is called from the Unity Editor when the user changes the size of the select
        /// box
        /// 
        /// @param size The size of the box that will be drawn around the ROI.
        public void UpdateSelectBoxSize(float size)
        {
            ROI.selectBoxSize = size;
        }
        /// If hardware acceleration is enabled, render the frame. Otherwise, set a flag to update the
        /// overlay and invalidate the overlay picture box
        /// 
        /// @return The return value is the value of the last expression evaluated in the method.
        public void UpdateOverlay()
        {
            updateOverlay = true;
            InvalidateView();
        }
        /// It updates the status bar of the image viewer
        /// 
        /// @return The statusLabel.Text is being returned.
        public void UpdateStatus()
        {
            if (SelectedImage == null)
                return;
            string res = "";
            if (SelectedImage.Type == BioImage.ImageType.well)
                res = "Well:" + SelectedImage.Level;
            else if (SelectedImage.Type == BioImage.ImageType.pyramidal)
            {
                res = $"Res.:{Resolution:G4} Level:{Level}";
            }
            string origin = $"({-Origin.X:0.##}, {-Origin.Y:0.##})";
            string location = $"({SelectedImage.Volume.Location.X:0.##}, {SelectedImage.Volume.Location.Y:0.##})";
            if (Mode == ViewMode.RGBImage)
            {
                if (timeEnabled)
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (tBar.Value + 1) + "/" + (tBar.Maximum + 1) + ", " +
                        mousePoint + mouseColor + ", " + SelectedImage.Buffers[0].PixelFormat.ToString() + ", " + origin + ", " + location + " " + res;
                }
                else
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + mousePoint + mouseColor + ", " + SelectedImage.Buffers[0].PixelFormat.ToString()
                        + ", " + origin + ", " + location + " " + res;
                }

            }
            else
            {
                if (timeEnabled)
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (cBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + (tBar.Value + 1) + "/" + (tBar.Maximum + 1) + ", " +
                        mousePoint + mouseColor + ", " + SelectedImage.Buffers[0].PixelFormat.ToString() + ", " + origin + ", " + location + " " + res;
                }
                else
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (cBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + mousePoint + mouseColor + ", " +
                        SelectedImage.Buffers[0].PixelFormat.ToString() + ", " + origin + ", " + location + " " + res;
                }
            }
        }
        /// If hardware acceleration is enabled, render the frame. Otherwise, invalidate the picture
        /// box.
        /// 
        /// @return The method is returning void.
        public void UpdateView()
        {
            UpdateImageSurface();
            if(SelectedImage!=null)
            if (SelectedImage.isPyramidal)
            {
                // Set the source based on image type
                if (SelectedImage.OpenSlideBase != null)
                {
                    slideRenderer.SetSource(SelectedImage.OpenSlideBase);
                }
                else if (SelectedImage.SlideBase != null)
                {
                    slideRenderer.SetSource(SelectedImage.SlideBase);
                }
            }
            UpdateStatus();
            update = true;
            UpdateImages();
            InvalidateView();
        }
        /// It draws the images and annotations to the screen
        /// 
        /// @return A Bitmap
        /*
        public void RenderFrame()
        {
            if (!update)
                return;
            drawing = true;
            if (dBitmaps.Count != Images.Count)
                UpdateImages();
            if (HardwareAcceleration && sk != null)
            {
                Plugins.Render(this, sk);
                sk.BeginDraw();
                sk.RenderTarget2D.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));
                RectangleF rg = ToScreenRect(PointD.MinX, PointD.MinY, PointD.MaxX - PointD.MinX, PointD.MaxY - PointD.MinY);
                //dx.RenderTarget2D.Transform = SharpDX.Matrix3x2.Rotation((float)Math.PI);
                SharpDX.Direct2D1.SolidColorBrush pen = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush red = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush green = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(0.0f, 1.0f, 0.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush mag = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(1.0f, 0.0f, 1.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush blue = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(0.0f, 0.0f, 1.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
                SharpDX.Direct2D1.SolidColorBrush gray = new SharpDX.Direct2D1.SolidColorBrush(sk.RenderTarget2D, new RawColor4(0.5f, 0.5f, 0.5f, 1.0f));

                sk.RenderTarget2D.FillRectangle(ToSKRect(rg.X, rg.Y, rg.Width, rg.Height), gray);
                for (int x = 0; x < Images.Count; x++)
                {
                    if (!IsLayerEnabled(Images[x].PhysicalSizeX) && !Images[x].isPyramidal)
                        continue;
                    RectangleF r = ToScreenRect(Images[x].Volume.Location.X, Images[x].Volume.Location.Y, Images[x].Volume.Width, Images[x].Volume.Height);
                    double w = ToViewW(ViewWidth);
                    double h = ToViewH(ViewHeight);
                    RectangleF rge = new RectangleF((float)((-Origin.X) - (w / 2)), (float)((-Origin.Y) - (h / 2)), (float)(Math.Abs(w)), (float)(Math.Abs(h)));
                    RectangleF rec = new RectangleF((float)Images[x].Volume.Location.X, (float)Images[x].Volume.Location.Y, (float)Images[x].Volume.Width, (float)Images[x].Volume.Height);
                    //if (rge.IntersectsWith(rec))
                    sk.RenderTarget2D.DrawBitmap(dBitmaps[x], ToSKRect(r.X, r.Y, r.Width, r.Height), 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
                    if (x == selectedIndex)
                        sk.RenderTarget2D.DrawRectangle(ToSKRect(r.X, r.Y, r.Width, r.Height), blue);
                }
                bool bounds = showBounds;
                bool labels = showText;
                foreach (BioImage bi in Images)
                {
                    if (!IsLayerEnabled(bi.PhysicalSizeX) && !bi.isPyramidal)
                        continue;
                    foreach (ROI an in bi.Annotations)
                    {
                        if (zBar.Value != an.coord.Z || cBar.Value != an.coord.C || tBar.Value != an.coord.T)
                            continue;
                        float w = Math.Abs(Scale.Width);
                        Font fo = new Font(new FontFamily(an.family), (float)((an.strokeWidth / w) * an.fontSize));
                        PointF pc = new PointF((float)(an.BoundingBox.X + (an.BoundingBox.W / 2)), (float)(an.BoundingBox.Y + (an.BoundingBox.H / 2)));
                        float width = 1;
                        sk.RenderTarget2D.StrokeWidth = width;
                        if (an.type == ROI.Type.Point)
                        {
                            PointF pf = ToScreenSpace(new PointF((float)an.Points[0].X + 1, (float)an.Points[0].Y + 1));
                            sk.RenderTarget2D.DrawLine(new RawVector2((float)an.Points[0].X, (float)an.Points[0].Y), new RawVector2(pf.X, pf.Y), b);
                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        else
                        if (an.type == ROI.Type.Line)
                        {
                            PointD pf = ToScreenSpace(an.GetPoint(0));
                            PointD pf2 = ToScreenSpace(an.GetPoint(1));
                            sk.RenderTarget2D.DrawLine(new RawVector2((float)pf.X, (float)pf.Y), new RawVector2((float)pf2.X, (float)pf2.Y), b);
                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        else
                        if (an.type == ROI.Type.Rectangle && an.BoundingBox.W > 0 && an.BoundingBox.H > 0)
                        {
                            RectangleF rec = ToScreenSpace(an.BoundingBox);
                            sk.RenderTarget2D.DrawRectangle(ToSKRect(rec.X, rec.Y, rec.Width, rec.Height), red);
                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        else
                        if (an.type == ROI.Type.Ellipse)
                        {
                            RectangleF r = ToScreenSpace(an.BoundingBox);
                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            RawRectangleF rf = ToSKRect(rfs[0].X, rfs[0].Y, rfs[0].Width, rfs[0].Height);
                            SharpDX.Direct2D1.Ellipse e = new SharpDX.Direct2D1.Ellipse(new RawVector2(rf.Left + (Math.Abs(r.Width) / 2) - (rfs[0].Width / 2), rf.Top + (Math.Abs(r.Height) / 2) - (rfs[0].Height / 2)), -r.Width / 2, -r.Height / 2);
                            sk.RenderTarget2D.DrawEllipse(e, b);
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        else
                        if (an.type == ROI.Type.Polygon && an.closed)
                        {
                            RawVector2 pf;
                            RawVector2 pf2;
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                pf = new RawVector2((float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).X, (float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).Y);
                                pf2 = new RawVector2((float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).X, (float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).Y);
                                sk.RenderTarget2D.DrawLine(pf, pf2, b);
                            }
                            pf = new RawVector2((float)ToScreenSpace(an.GetPoint(0)).X, (float)ToScreenSpace(an.GetPoint(0)).Y);
                            pf2 = new RawVector2((float)ToScreenSpace(an.GetPoint(an.Points.Count - 1)).X, (float)ToScreenSpace(an.GetPoint(an.Points.Count - 1)).Y);
                            sk.RenderTarget2D.DrawLine(pf, pf2, b);

                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        else
                        if (an.type == ROI.Type.Polygon && !an.closed || an.type == ROI.Type.Polyline)
                        {
                            RawVector2 pf;
                            RawVector2 pf2;
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                pf = new RawVector2((float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).X, (float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).Y);
                                pf2 = new RawVector2((float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).X, (float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).Y);
                                sk.RenderTarget2D.DrawLine(pf, pf2, b);
                                sk.RenderTarget2D.DrawLine(pf, pf2, b);
                            }

                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }

                        }
                        else
                        if (an.type == ROI.Type.Freeform && an.closed)
                        {
                            RawVector2 pf;
                            RawVector2 pf2;
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                pf = new RawVector2((float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).X, (float)ToScreenSpace(an.GetPoint(i).X, an.GetPoint(i).Y).Y);
                                pf2 = new RawVector2((float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).X, (float)ToScreenSpace(an.GetPoint(i + 1).X, an.GetPoint(i + 1).Y).Y);
                                sk.RenderTarget2D.DrawLine(pf, pf2, b);
                                sk.RenderTarget2D.DrawLine(pf, pf2, b);
                            }
                            pf = new RawVector2((float)ToScreenSpace(an.GetPoint(0)).X, (float)ToScreenSpace(an.GetPoint(0)).Y);
                            pf2 = new RawVector2((float)ToScreenSpace(an.GetPoint(an.Points.Count - 1)).X, (float)ToScreenSpace(an.GetPoint(an.Points.Count - 1)).Y);
                            sk.RenderTarget2D.DrawLine(pf, pf2, b);
                        }
                        if (an.type == ROI.Type.Label)
                        {
                            RectangleF rec = ToScreenSpace(an.BoundingBox);
                            RawRectangleF r = ToSKRect(rec.X, rec.Y, rec.Width, rec.Height);
                            SharpDX.DirectWrite.TextFormat tex = new SharpDX.DirectWrite.TextFormat(sk.FactoryDWrite, an.family.ToString(), an.fontSize);
                            sk.RenderTarget2D.DrawText(an.Text, tex, r, b);
                            tex.Dispose();
                            RectangleF[] rfs = ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize));
                            for (int i = 0; i < rfs.Length; i++)
                            {
                                sk.RenderTarget2D.DrawRectangle(ToSKRect(rfs[i].X, rfs[i].Y, rfs[i].Width, rfs[i].Height), red);
                            }
                        }
                        if (labels)
                        {
                            if (an.Text != null)
                            {
                                System.Drawing.Size s = TextRenderer.MeasureText(an.Text, new Font(an.family, an.fontSize));
                                //Lets draw the text of this ROI in the middle of the ROI.
                                float fw = ((float)an.BoundingBox.X + ((float)an.BoundingBox.W / 2)) - ((float)s.Width / 2);
                                float fh = ((float)an.BoundingBox.Y + ((float)an.BoundingBox.H / 2)) - ((float)s.Height / 2);
                                RawRectangleF r = ToSKRect(fw, fh, s.Width, s.Height);
                                SharpDX.DirectWrite.TextFormat tex = new SharpDX.DirectWrite.TextFormat(sk.FactoryDWrite, an.family.ToString(), an.fontSize);
                                sk.RenderTarget2D.DrawText(an.Text, tex, r, b);
                                tex.Dispose();
                            }
                        }
                        if (bounds)
                        {
                            RectangleF r = ToScreenSpace(an.BoundingBox);
                            sk.RenderTarget2D.DrawRectangle(ToSKRect(r.X, r.Y, r.Width, r.Height), green);
                        }
                        if (an.Selected)
                        {
                            //Lets draw the selected bounding box.
                            RectangleF r = ToScreenSpace(an.BoundingBox);
                            sk.RenderTarget2D.DrawRectangle(ToSKRect(r.X, r.Y, r.Width, r.Height), mag);

                            //Lets draw the selectBoxes.
                            List<RectangleD> rects = new List<RectangleD>();
                            RectangleD[] sels = an.GetSelectBoxes(Tools.selectBoxSize);
                            for (int i = 0; i < an.selectedPoints.Count; i++)
                            {
                                if (an.selectedPoints[i] < an.GetPointCount())
                                {
                                    rects.Add(sels[an.selectedPoints[i]]);
                                }
                            }
                            if (rects.Count > 0)
                            {
                                for (int i = 0; i < rects.Count; i++)
                                {
                                    RectangleF ri = ToScreenSpace(rects[i]);
                                    sk.RenderTarget2D.DrawRectangle(ToSKRect(ri.X, ri.Y, ri.Width, ri.Height), blue);
                                }
                            }
                            rects.Clear();
                        }

                    }
                }
                if (ShowStage)
                {
                    AForge.RectangleD re = Microscope.GetViewRectangle(false);
                    RectangleF vr = ToScreenRect(re.X, re.Y, re.W, re.H);
                    RawRectangleF rr = ToSKRect(vr.X, vr.Y, vr.Width, vr.Height);
                    sk.RenderTarget2D.DrawRectangle(rr, red);
                }

                if(Tools.currentTool!=null)
                if (Tools.currentTool.type == Tools.Tool.Type.rectSel)// && Modifiers == ModifierType.Button1Mask)
                {
                    var recd = ToScreenRect(Tools.currentTool.Rectangle.X, Tools.currentTool.Rectangle.Y, Tools.currentTool.Rectangle.W, Tools.currentTool.Rectangle.H);
                    sk.RenderTarget2D.DrawRectangle(ToSKRect((float)recd.X, (float)recd.Y, (float)recd.Width, (float)recd.Height), blue);
                }
                pen.Dispose();
                red.Dispose();
                mag.Dispose();
                green.Dispose();
                blue.Dispose();
                gray.Dispose();
                b.Dispose();
                sk.EndDraw();
            }
            drawing = false;
        }
        */
        /// > Update the status of the application and then render the frame
        /// 
        /// @param refresh boolean
        public void UpdateView(bool refresh)
        {
            UpdateStatus();
            update = refresh;
            if (update)
            {
                InvalidateView();
            }
        }
        bool updating = false;
        /// It takes a list of images, and for each image, it gets the image at the current Z, C, T
        /// coordinates, and then converts it to a bitmap
        /// 
        /// @return A Bitmap
        private int zarrRgbRequest;
        public async void UpdateImages(bool updatePyramidal = true)
        {
            if (SelectedImage == null)
                return;
            UpdateImageSurface();
            if (SelectedImage.isPyramidal)
            {
                InitPreview();
                if (!updatePyramidal || gl.Width <= 1 || gl.Height <= 1)
                    return;
                updatePyr = true;
                SelectedImage.PyramidalOrigin = PyramidalOrigin;
                SelectedImage.PyramidalSize = new AForge.Size(gl.Width, gl.Height);

                // Use the new renderer - no more ReadPixels!
                if (slideRenderer == null)
                {
                    slideRenderer = new SlideRenderer(gl);
                }
                try
                {
                    BioImage? directZarr = null;
                    int[]? directZarrChannels = null;
                    if (SelectedImage.IsZarrSource)
                    {
                        if (Mode == ViewMode.RGBImage)
                        {
                            directZarr = SelectedImage;
                            directZarrChannels = SelectedRgbChannels(SelectedImage);
                        }
                        else if (Mode == ViewMode.Filtered)
                        {
                            // Use the same direct, coordinate-aware Zarr reader as
                            // RGB mode. Repeating the selected channel produces the
                            // expected grayscale Filtered view and avoids the legacy
                            // buffered plane path that could remain stuck on Z=0.
                            int channel = Math.Clamp(GetCoordinate().C, 0,
                                Math.Max(0, SelectedImage.SizeC - 1));
                            directZarr = SelectedImage;
                            directZarrChannels = new[] { channel, channel, channel };
                        }
                    }

                    await slideRenderer.UpdateViewAsync(
                        PyramidalOrigin,
                        gl.Width,
                        gl.Height,
                        Resolution,
                        GetCoordinate(),
                        directZarr,
                        directZarrChannels
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Could not update slide view: {ex}");
                }
                // Pyramidal images are drawn from tiles, not the full-plane buffers below.
                return;
            }

            if (SelectedImage.IsZarrSource &&
                (Mode == ViewMode.RGBImage || Mode == ViewMode.Filtered) &&
                Images.Count == 1 && SelectedImage.levels?.Count > 0)
            {
                var image = SelectedImage;
                var requestedMode = Mode;
                int request = ++zarrRgbRequest;
                int width = image.Buffers[0].SizeX, height = image.Buffers[0].SizeY;
                try
                {
                    int[] channels;
                    if (requestedMode == ViewMode.RGBImage)
                    {
                        channels = SelectedRgbChannels(image);
                    }
                    else
                    {
                        int channel = Math.Clamp(GetCoordinate().C, 0,
                            Math.Max(0, image.SizeC - 1));
                        channels = new[] { channel, channel, channel };
                    }

                    byte[] rgba = await ZarrChannelView.ReadRgbTileAsync(image, 0, 0, 0, width, height,
                        GetCoordinate(), channels);
                    if (request != zarrRgbRequest || SelectedImage != image || Mode != requestedMode)
                        return;
                    var frame = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var data = frame.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    try
                    {
                        for (int i = 0; i < rgba.Length; i += 4)
                            (rgba[i], rgba[i + 2]) = (rgba[i + 2], rgba[i]);
                        for (int row = 0; row < height; row++)
                            Marshal.Copy(rgba, row * width * 4, IntPtr.Add(data.Scan0, row * data.Stride), width * 4);
                    }
                    finally { frame.UnlockBits(data); }
                    foreach (var old in Bitmaps) old?.Dispose();
                    Bitmaps.Clear();
                    Bitmaps.Add(frame);
                    update = true;
                    InvalidateView();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Could not render Zarr RGB channels: {ex.Message}");
                }
                return;
            }

            // AForge's filters copy from unmanaged pixel pointers. Missing or disposed
            // buffers have a null pointer, so keep the last frame until pixels are available.
            foreach (BioImage image in Images)
            {
                int index = image.GetFrameIndex(zBar.Value, cBar.Value, tBar.Value);
                bool unavailable = index < 0 || index >= image.Buffers.Count;
                if (!unavailable)
                {
                    var buffers = Mode == ViewMode.Filtered || Mode == ViewMode.Raw
                        ? new[] { image.Buffers[index] }
                        : image.Buffers.AsEnumerable();
                    unavailable = buffers.Any(buffer =>
                        buffer == null || buffer.Bytes == null || buffer.Bytes.Length == 0);
                }
                if (unavailable)
                {
                    System.Diagnostics.Trace.WriteLine($"Image pixels are unavailable: {image.ID}");
                    return;
                }
            }
            for (int i = 0; i < Bitmaps.Count; i++)
            {
                Bitmaps[i].Dispose();
                Bitmaps[i] = null;
            }
            Bitmaps.Clear();
            if (zBar.Maximum != SelectedImage.SizeZ - 1 || tBar.Maximum != SelectedImage.SizeT - 1)
            {
                InitGUI();
            }

            int bi = 0;
            foreach (BioImage b in Images)
            {
                if (b.isPyramidal)
                    updatePyr = true;
                b.PyramidalOrigin = PyramidalOrigin;
                b.PyramidalSize = new AForge.Size(gl.Width, gl.Height);
                ZCT coords = new ZCT(zBar.Value, cBar.Value, tBar.Value);
                Bitmap bitmap = null;

                int index = b.Coords[zBar.Value, cBar.Value, tBar.Value];
                if (Mode == ViewMode.Filtered)
                {
                    bitmap = b.GetFiltered(coords, b.RChannel.RangeR, b.GChannel.RangeG, b.BChannel.RangeB);
                }
                else if (Mode == ViewMode.RGBImage)
                {
                    if (b.Buffers[0].RGBChannelsCount == 1)
                    {
                        var planes = new Bitmap[3];
                        for (int color = 0; color < 3; color++)
                        {
                            int channel = Math.Clamp(b.rgbChannels[color], 0, b.SizeC - 1);
                            planes[2 - color] = b.Buffers[b.GetFrameIndex(coords.Z, channel, coords.T)];
                        }
                        bitmap = Bitmap.GetRGBBitmap(planes, b.RChannel.RangeR, b.GChannel.RangeG, b.BChannel.RangeB);
                    }
                    else
                        bitmap = b.GetRGBBitmap(coords, b.RChannel.RangeR, b.GChannel.RangeG, b.BChannel.RangeB);
                }
                else if (Mode == ViewMode.Raw)
                {
                    bitmap = (Bitmap)b.Buffers[index].GetImageRGBA();
                }
                else
                {
                    bitmap = b.GetEmission(coords, b.RChannel.RangeR, b.GChannel.RangeG, b.BChannel.RangeB);
                }
                var display = ToSysBitmap(bitmap);
                if (Mode == ViewMode.RGBImage)
                    MaskRgbComponents(display, rgbEnabled);
                Bitmaps.Add(display);
                bi++;
            }
            UpdateRegularOverview();
            update = true;
            InvalidateView();
        }
        bool drawing = false;
        public void RemoveImage(int i)
        {
            do
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
            } while (drawing || updating);
            Images[i].Dispose();
            Images.RemoveAt(i);
            UpdateImages();
        }
        public void RemoveImages()
        {
            do
            {
                Thread.Sleep(50);
                Application.DoEvents();
            } while (drawing || updating);
            for (int i = 0; i < Images.Count; i++)
            {
                Images[i].Dispose();
            }
            Images.Clear();
            UpdateImages();
        }
        Bitmap bitmap;
        /// It takes a 16-bit image, converts it to 8-bit, and then converts it to a DirectX texture
        /// 
        /// @return A bitmap
        public void UpdateImage()
        {
            UpdateImages();
        }
        /// When the user selects a new channel from the dropdown menu, the program updates the image to
        /// display the new channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The index of the selected item in the ComboBox.
        private void channelBoxR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializingRgbSelectors || SelectedImage == null || channelBoxR.SelectedIndex == -1)
                return;
            rgbEnabled[0] = channelBoxR.SelectedIndex > 0;
            if (rgbEnabled[0]) SelectedImage.rgbChannels[0] = channelBoxR.SelectedIndex - 1;
            update = true;
            UpdateView();
        }
        /// When the user selects a new channel from the dropdown menu, the program updates the image to
        /// reflect the new channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The index of the selected item in the ComboBox.
        private void channelBoxG_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializingRgbSelectors || SelectedImage == null || channelBoxG.SelectedIndex == -1)
                return;
            rgbEnabled[1] = channelBoxG.SelectedIndex > 0;
            if (rgbEnabled[1]) SelectedImage.rgbChannels[1] = channelBoxG.SelectedIndex - 1;
            update = true;
            UpdateView();
        }
        /// When the user selects a channel from the dropdown menu, the program updates the image to
        /// display the selected channel
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The selected index of the channelBoxB ComboBox.
        private void channelBoxB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializingRgbSelectors || SelectedImage == null || channelBoxB.SelectedIndex == -1)
                return;
            rgbEnabled[2] = channelBoxB.SelectedIndex > 0;
            if (rgbEnabled[2]) SelectedImage.rgbChannels[2] = channelBoxB.SelectedIndex - 1;
            update = true;
            UpdateView();
        }
        /// If the trackBarPanel is visible, hide it, set its height to 0, and set the gl height
        /// to 75. 
        /// If the trackBarPanel is not visible, show it, set the gl height to 75, and set the
        /// trackBarPanel height to 75
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void showControlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trackBarPanel.Visible)
            {
                trackBarPanel.Hide();
                trackBarPanel.Height = 0;
                Application.DoEvents();
                gl.Height += 75;
                showControlsToolStripMenuItem.Text = "Show Controls";
                gl.Dock = DockStyle.Fill;
            }
            else
            {
                trackBarPanel.Show();
                gl.Height -= 75;
                Application.DoEvents();
                trackBarPanel.Height = 75;
                showControlsToolStripMenuItem.Text = "Hide Controls";
                gl.Dock = DockStyle.Fill;
            }

        }
        /// If the playZToolStripMenuItem is checked, we stop the timer, otherwise we start the timer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void playZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (playZToolStripMenuItem.Checked)
            {
                //We stop
                playZToolStripMenuItem.Checked = false;
                stopZToolStripMenuItem.Checked = true;
                zTimer.Stop();
            }
            else
            {
                //We start
                playZToolStripMenuItem.Checked = true;
                stopZToolStripMenuItem.Checked = false;
                zTimer.Start();
            }
        }
        /// If the stopZToolStripMenuItem is checked, we start the zTimer, otherwise we stop it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments that are passed to the event handler.
        private void stopZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (stopZToolStripMenuItem.Checked)
            {
                //We start
                playZToolStripMenuItem.Checked = true;
                stopZToolStripMenuItem.Checked = false;
                zTimer.Start();

            }
            else
            {
                //We stop
                playZToolStripMenuItem.Checked = false;
                stopZToolStripMenuItem.Checked = true;
                zTimer.Stop();
            }
        }
        /// If the playTimeToolStripMenu is checked, we stop the timer, otherwise we start the timer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void playTimeToolStripMenu_Click(object sender, EventArgs e)
        {
            if (playTimeToolStripMenu.Checked)
            {
                //We stop
                playTimeToolStripMenu.Checked = false;
                stopTimeToolStripMenu.Checked = true;
                timelineTimer.Stop();
            }
            else
            {
                //We start
                playTimeToolStripMenu.Checked = true;
                stopTimeToolStripMenu.Checked = false;
                timelineTimer.Start();
            }
        }
        /// This function stops the timer and unchecks the playTimeToolStripMenu and checks the
        /// stopTimeToolStripMenu
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void stopTimeToolStripMenu_Click(object sender, EventArgs e)
        {
            playTimeToolStripMenu.Checked = false;
            stopTimeToolStripMenu.Checked = true;
            timelineTimer.Stop();
        }
        /// If the play button is checked, we stop the timer, otherwise we start the timer
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void playCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (playCToolStripMenuItem.Checked)
            {
                //We stop
                playCToolStripMenuItem.Checked = false;
                stopCToolStripMenuItem.Checked = true;
                cTimer.Stop();
            }
            else
            {
                //We start
                playCToolStripMenuItem.Checked = true;
                stopCToolStripMenuItem.Checked = false;
                cTimer.Start();
            }
        }
        /// If the user clicks the stop button, then the stop button is unchecked and the play button is
        /// checked, and the timer is stopped
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void stopCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopCToolStripMenuItem.Checked = false;
            playCToolStripMenuItem.Checked = true;
            cTimer.Stop();
        }
        private string mousePoint = "";
        private string mouseColor = "";
        /// The function is called when the user clicks on the "Play Speed" menu item
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        /// 
        /// @return The return value is a DialogResult.
        private void playSpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            else
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            zTimer.Interval = sp.TimePlayspeed;
            cTimer.Interval = sp.CPlayspeed;
            timelineTimer.Interval = sp.TimePlayspeed;
        }
        /// The function is called when the user clicks on the "Play Speed" menu item in the "View" menu
        /// 
        /// @param sender System.Object
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The DialogResult.OK is being returned.
        private void playSpeedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            else
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            zTimer.Interval = sp.TimePlayspeed;
            cTimer.Interval = sp.CPlayspeed;
            timelineTimer.Interval = sp.TimePlayspeed;
        }
        /// The function is called when the user clicks on the "Play Speed" menu item. It opens a dialog
        /// box that allows the user to change the speed at which the images are played
        /// 
        /// @param sender System.Object
        /// @param EventArgs e
        /// 
        /// @return The DialogResult.OK is being returned.
        private void CPlaySpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            else
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            zTimer.Interval = sp.TimePlayspeed;
            cTimer.Interval = sp.CPlayspeed;
            timelineTimer.Interval = sp.TimePlayspeed;
        }
        /// It creates a new RangeTool object, which is a form that allows the user to set the range of
        /// the data to be displayed. 
        /// 
        /// The RangeTool object is created with the following parameters: 
        /// 
        /// - timeEnabled: a boolean that indicates whether the data has a time component. 
        /// - Mode: a ViewMode enum that indicates whether the data is filtered or not. 
        /// - zBar.Minimum: the minimum value of the zBar. 
        /// - zBar.Maximum: the maximum value of the zBar. 
        /// - tBar.Minimum: the minimum value of the tBar. 
        /// - tBar.Maximum: the maximum value of the tBar. 
        /// - cBar.Minimum: the minimum value of the cBar. 
        /// - cBar.Maximum: the maximum value of the cBar. 
        /// 
        /// The RangeTool object is then shown to the user. 
        /// 
        /// If the user clicks the
        /// 
        /// @return A RangeTool object.
        public void GetRange()
        {
            RangeTool t;
            if (Mode == ViewMode.Filtered)
                t = new RangeTool(timeEnabled, true, zBar.Minimum, zBar.Maximum, tBar.Minimum, tBar.Maximum, cBar.Minimum, cBar.Maximum);
            else
                t = new RangeTool(timeEnabled, false, zBar.Minimum, zBar.Maximum, tBar.Minimum, tBar.Maximum, cBar.Minimum, cBar.Maximum);
            if (t.ShowDialog() != DialogResult.OK)
                return;
            zBar.Minimum = t.ZMin;
            zBar.Maximum = t.ZMax;
            tBar.Minimum = t.TimeMin;
            tBar.Maximum = t.TimeMax;
            cBar.Minimum = t.CMin;
            cBar.Maximum = t.CMax;
        }
        private void setValueRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetRange();
        }
        private void setValueRangeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetRange();
        }
        private void setCValueRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetRange();
        }
        /// It's a function that handles the mouse wheel event for the image view
        /// 
        /// @param sender The object that raised the event.
        /// @param e the mouse event
        private void ImageView_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handled && handled.Handled)
                return;
            var source = sender as System.Windows.Forms.Control ?? this;
            System.Drawing.Point cursor = source.Parent == viewport
                ? new System.Drawing.Point(source.Left + e.X, source.Top + e.Y)
                : viewport.PointToClient(source.PointToScreen(e.Location));
            if (e.Delta == 0 || !viewport.ClientRectangle.Contains(cursor) || Images.Count == 0)
                return;

            App.viewer = this;
            Plugins.ScrollEvent(sender, e);
            if (e is HandledMouseEventArgs wheel)
            {
                if (wheel.Handled)
                    return;
                // Prevent a canvas event from bubbling up and zooming a second time.
                wheel.Handled = true;
            }

            const double minZoom = 0.000001;
            const double maxZoom = 1000000;
            double factor = Math.Pow(1.2, e.Delta / 120.0);
            PointD anchor = ImageToViewSpace(cursor.X, cursor.Y);
            if (SelectedImage.isPyramidal)
            {
                // Resolution is world units per screen pixel, so zooming in reduces it.
                Resolution = Math.Clamp(Resolution / factor, minZoom, maxZoom);
                PyramidalOrigin = new PointD(anchor.X / Resolution - cursor.X,
                    anchor.Y / Resolution - cursor.Y);
                Tools.selectBoxSize = (float)(ROI.selectBoxSize * Resolution);
                UpdateImages();
            }
            else
            {
                // Clamp the common factor to preserve the X/Y scale ratio.
                factor = Math.Clamp(factor,
                    minZoom / Math.Min(Scale.Width, Scale.Height),
                    maxZoom / Math.Max(Scale.Width, Scale.Height));
                Scale = new SizeF((float)(Scale.Width * factor), (float)(Scale.Height * factor));
                PointD shifted = ImageToViewSpace(cursor.X, cursor.Y);
                Origin = new PointD(Origin.X + shifted.X - anchor.X, Origin.Y + shifted.Y - anchor.Y);
            }
            UpdateView();
        }
        /// If the mouse wheel is scrolled up, increase the value of the trackbar by 1, if the mouse
        /// wheel is scrolled down, decrease the value of the trackbar by 1
        /// 
        /// @param sender The object that raised the event.
        /// @param e The mouse event that was triggered
        private void ZTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (zBar.Value + 1 <= zBar.Maximum)
                    zBar.Value += 1;
            }
            else
            {
                if (zBar.Value - 1 >= zBar.Minimum)
                    zBar.Value -= 1;
            }

        }
        /// If the mouse wheel is scrolled up, the time trackbar value is increased by 1, and if the
        /// mouse wheel is scrolled down, the time trackbar value is decreased by 1
        /// 
        /// @param sender The object that raised the event.
        /// @param e The mouse event that was triggered
        /// 
        /// @return The value of the time trackbar.
        private void TimeTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!timeEnabled)
                return;
            if (e.Delta > 0)
            {
                if (tBar.Value + 1 <= tBar.Maximum)
                    tBar.Value += 1;
            }
            else
            {
                if (tBar.Value - 1 >= tBar.Minimum)
                    tBar.Value -= 1;
            }
        }
        /// If the mouse wheel is scrolled up, increase the value of the trackbar by 1, if the mouse
        /// wheel is scrolled down, decrease the value of the trackbar by 1
        /// 
        /// @param sender The object that raised the event.
        /// @param e The mouse event arguments.
        private void CTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (cBar.Value + 1 <= cBar.Maximum)
                    cBar.Value += 1;
            }
            else
            {
                if (cBar.Value - 1 >= cBar.Minimum)
                    cBar.Value -= 1;
            }
        }

        /// If the user has checked the "Play C" checkbox, and the C bar is not at its maximum value,
        /// then increment the C bar by 1. If the C bar is at its maximum value, and the user has
        /// checked the "Loop C" checkbox, then reset the C bar to its minimum value
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void cTimer_Tick(object sender, EventArgs e)
        {
            if (playCToolStripMenuItem.Checked)
            {
                if (cBar.Maximum >= cBar.Value + 1)
                    cBar.Value++;
                else
                {
                    if (loopC)
                        cBar.Value = cBar.Minimum;
                }
            }
        }
        /// If the user has checked the "play" checkbox, then if the maximum value of the slider is
        /// greater than the current value of the slider, then increment the slider by one. Otherwise,
        /// if the user has checked the "loop" checkbox, then set the slider to its minimum value
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void zTimer_Tick(object sender, EventArgs e)
        {
            if (playZToolStripMenuItem.Checked)
            {
                if (zBar.Maximum >= zBar.Value + 1)
                    zBar.Value++;
                else
                {
                    if (loopZ)
                        zBar.Value = zBar.Minimum;
                }
            }
        }
        /// If the playTimeToolStripMenu is checked, then if the maximum value of the trackbar is
        /// greater than or equal to the current value of the trackbar plus 1, then the value of the
        /// trackbar is incremented by 1. Otherwise, if the loopT variable is true, then the value of
        /// the trackbar is set to the minimum value of the trackbar.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void timer_Tick(object sender, EventArgs e)
        {
            if (playTimeToolStripMenu.Checked)
            {
                if (tBar.Maximum >= tBar.Value + 1)
                    tBar.Value++;
                else
                {
                    if (loopT)
                        tBar.Value = tBar.Minimum;
                }
            }
        }
        /// When the value of the zBar is changed, the z coordinate of the selected image is changed to
        /// the value of the zBar
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The value of the ZBar.Value is being returned.
        private void zBar_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            SelectedImage.Coordinate = new ZCT(zBar.Value, SelectedImage.Coordinate.C, SelectedImage.Coordinate.T);
            update = true;
            UpdateImage();
            UpdateView();
        }
        /// When the time bar is changed, the image's coordinate is changed to the new time value
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The image is being returned.
        private void timeBar_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            SelectedImage.Coordinate = new ZCT(SelectedImage.Coordinate.Z, SelectedImage.Coordinate.C, tBar.Value);
            update = true;
            UpdateImage();
            UpdateView();
        }
        /// When the value of the cBar is changed, the coordinate of the selected image is changed to the
        /// new value of the cBar
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs System.EventArgs
        /// 
        /// @return The value of the slider.
        private void cBar_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            SelectedImage.Coordinate = new ZCT(SelectedImage.Coordinate.Z, cBar.Value, SelectedImage.Coordinate.T);
            update = true;
            UpdateImage();
            UpdateView();
        }

        private void loopTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopT = loopTimeToolStripMenuItem.Checked;
        }
        private void loopZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopZ = loopZToolStripMenuItem.Checked;
        }
        private void loopCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopC = loopCToolStripMenuItem.Checked;
        }

        public bool showRROIs = true;
        public bool showGROIs = true;
        public bool showBROIs = true;

        private List<ROI> annotationsR = new List<ROI>();
        public List<ROI> AnnotationsR
        {
            get
            {
                return SelectedImage.GetAnnotations(SelectedImage.Coordinate.Z, SelectedImage.RChannel.Index, SelectedImage.Coordinate.T);
            }
        }
        private List<ROI> annotationsG = new List<ROI>();
        public List<ROI> AnnotationsG
        {
            get
            {
                return SelectedImage.GetAnnotations(SelectedImage.Coordinate.Z, SelectedImage.GChannel.Index, SelectedImage.Coordinate.T);
            }
        }
        private List<ROI> annotationsB = new List<ROI>();
        public List<ROI> AnnotationsB
        {
            get
            {
                return SelectedImage.GetAnnotations(SelectedImage.Coordinate.Z, SelectedImage.BChannel.Index, SelectedImage.Coordinate.T);
            }
        }
        public List<ROI> AnnotationsRGB
        {
            get
            {
                if (SelectedImage == null)
                    return null;
                List<ROI> ans = new List<ROI>();
                if (Mode == ViewMode.RGBImage)
                {
                    if (showRROIs && rgbEnabled[0])
                        ans.AddRange(AnnotationsR);
                    if (showGROIs && rgbEnabled[1])
                        ans.AddRange(AnnotationsG);
                    if (showBROIs && rgbEnabled[2])
                        ans.AddRange(AnnotationsB);
                }
                else
                {
                    ans.AddRange(SelectedImage.GetAnnotations(SelectedImage.Coordinate));
                }
                return ans;
            }
        }
        private System.Drawing.Point ToPoint(AForge.Point p)
        {
            return new System.Drawing.Point((int)p.X, (int)p.Y);
        }
        private System.Drawing.PointF ToPointF(PointD p)
        {
            return new System.Drawing.PointF((float)p.X, (float)p.Y);
        }
        /// This function draws the ROIs on the image
        /// 
        /// @param g Graphics object
        /// 
        /// @return A list of ROI objects.
        private void DrawOverlay(System.Drawing.Graphics g)
        {
            if (SelectedImage == null)
                return;
            if (!updateOverlay)
                return;
            System.Drawing.Pen pen = null;
            System.Drawing.Pen red = null;
            System.Drawing.Pen green = null;
            System.Drawing.Pen mag = null;
            System.Drawing.Pen blue = null;
            Brush b = null;
            bool bounds = showBounds;
            bool labels = showText;
            ZCT cor = GetCoordinate();
            foreach (BioImage bi in Images)
            {
                if (!IsLayerEnabled(bi.PhysicalSizeX))
                    continue;
                foreach (ROI an in bi.Annotations)
                {
                    if (Mode == ViewMode.RGBImage)
                    {
                        if (!showRROIs && an.coord.C == 0)
                            continue;
                        if (!showGROIs && an.coord.C == 1)
                            continue;
                        if (!showBROIs && an.coord.C == 2)
                            continue;
                    }
                    else if (zBar.Value != an.coord.Z || cBar.Value != an.coord.C || tBar.Value != an.coord.T)
                        continue;
                    float w = Math.Abs(Scale.Width);
                    pen = new System.Drawing.Pen(new SolidBrush(System.Drawing.Color.FromArgb(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B)), (float)an.strokeWidth / w);
                    red = new System.Drawing.Pen(Brushes.Red, (float)an.strokeWidth / w);
                    mag = new System.Drawing.Pen(Brushes.Magenta, (float)an.strokeWidth / w);
                    green = new System.Drawing.Pen(Brushes.Green, (float)an.strokeWidth / w);
                    blue = new System.Drawing.Pen(Brushes.Blue, (float)an.strokeWidth / w);
                    Font fo = new Font(an.family, (float)(an.strokeWidth / w) * an.fontSize);
                    if (an.Selected)
                    {
                        b = new SolidBrush(System.Drawing.Color.Magenta);
                    }
                    else
                        b = new SolidBrush(System.Drawing.Color.FromArgb(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B));
                    PointF pc = new PointF((float)(an.BoundingBox.X + (an.BoundingBox.W / 2)), (float)(an.BoundingBox.Y + (an.BoundingBox.H / 2)));
                    float width = (float)ToViewSizeW(ROI.selectBoxSize / w);
                    if (SelectedImage.isPyramidal)
                    {
                        width = (float)(ROI.selectBoxSize * Resolution);
                    }
                    if (an.type == ROI.Type.Point)
                    {
                        g.DrawLine(pen, ToPointF(ToScreenSpace(an.Points[0])), ToPointF(ToScreenSpace(an.Points[0].X + 1, an.Points[0].Y + 1)));
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Line)
                    {
                        g.DrawLine(pen, ToPointF(ToScreenSpace(an.Points[0])), ToPointF(ToScreenSpace(an.GetPoint(1))));
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Rectangle && an.BoundingBox.W > 0 && an.BoundingBox.H > 0)
                    {
                        g.DrawRectangle(pen, new System.Drawing.RectangleF((float)an.BoundingBox.X, (float)an.BoundingBox.Y, (float)an.W, (float)an.H));
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Ellipse)
                    {
                        RectangleD rec = ToScreenSpace(an.BoundingBox);
                        g.DrawEllipse(pen, (float)rec.X, (float)rec.Y, (float)rec.W, (float)rec.H);
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Polygon && an.closed)
                    {
                        if (an.Points.Count > 1)
                        {
                            System.Drawing.Point[] pts = new System.Drawing.Point[an.Points.Count];
                            for (int i = 0; i < an.Points.Count; i++)
                            {
                                var p = ToScreenSpace(an.Points[i].X, an.Points[i].Y);
                                pts[i] = new System.Drawing.Point((int)p.X, (int)p.Y);
                            }
                            g.DrawLines(pen, pts);
                        }
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Polygon && !an.closed)
                    {
                        if (an.Points.Count > 1)
                        {
                            System.Drawing.Point[] pts = new System.Drawing.Point[an.Points.Count - 1];
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                var p = ToScreenSpace(an.Points[i].X, an.Points[i].Y);
                                pts[i] = new System.Drawing.Point((int)p.X, (int)p.Y);
                            }
                            g.DrawLines(pen, pts);
                        }
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    else
                    if (an.type == ROI.Type.Polyline)
                    {
                        if (an.Points.Count > 1)
                        {
                            System.Drawing.Point[] pts = new System.Drawing.Point[an.Points.Count - 1];
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                var p = ToScreenSpace(an.Points[i].X, an.Points[i].Y);
                                pts[i] = new System.Drawing.Point((int)p.X, (int)p.Y);
                            }
                            g.DrawLines(pen, pts);
                        }
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }

                    else
                    if (an.type == ROI.Type.Freeform && an.closed)
                    {
                        if (an.Points.Count > 1)
                        {
                            System.Drawing.Point[] pts = new System.Drawing.Point[an.Points.Count];
                            for (int i = 0; i < an.Points.Count; i++)
                            {
                                var p = ToScreenSpace(an.Points[i].X, an.Points[i].Y);
                                pts[i] = new System.Drawing.Point((int)p.X, (int)p.Y);
                            }
                            g.DrawLines(pen, pts);
                        }
                    }
                    else
                    if (an.type == ROI.Type.Freeform && !an.closed)
                    {
                        if (an.Points.Count > 1)
                        {
                            System.Drawing.Point[] pts = new System.Drawing.Point[an.Points.Count - 1];
                            for (int i = 0; i < an.Points.Count - 1; i++)
                            {
                                var p = ToScreenSpace(an.Points[i].X, an.Points[i].Y);
                                pts[i] = new System.Drawing.Point((int)p.X, (int)p.Y);
                            }
                            g.DrawLines(pen, pts);
                        }
                    }
                    if (an.type == ROI.Type.Label)
                    {
                        var v = ToScreenSpace(an.Points[0]);
                        g.DrawString(an.Text, fo, b, (float)v.X, (float)v.Y);
                        foreach (var item in ToScreenSpace(an.GetSelectBoxes(Tools.selectBoxSize)))
                        {
                            g.DrawRectangle(red, new System.Drawing.RectangleF((float)item.X, (float)item.Y, (float)item.W, (float)item.H));
                        }
                    }
                    if (labels)
                    {
                        System.Drawing.SizeF s = System.Windows.Forms.TextRenderer.MeasureText(an.Text, new Font(an.family, an.fontSize));
                        //Lets draw the text of this ROI in the middle of the RO
                        float fw = ((float)an.BoundingBox.X + ((float)an.BoundingBox.W / 2)) - ((float)s.Width / 2);
                        float fh = ((float)an.BoundingBox.Y + ((float)an.BoundingBox.H / 2)) - ((float)s.Height / 2);
                        var v = ToScreenSpace(new PointF(fw, fh));
                        g.DrawString(an.Text, fo, b, v.X, v.Y);
                    }
                    if (bounds)
                    {
                        var v = ToScreenSpace(an.BoundingBox);
                        g.DrawRectangles(green, new System.Drawing.RectangleF[1] { new System.Drawing.RectangleF((float)v.X, (float)v.Y, (float)v.W, (float)v.H) });
                    }
                    if (an.Selected)
                    {
                        //Lets draw the bounding box.
                        var v = ToScreenSpace(an.BoundingBox);
                        g.DrawRectangles(mag, new System.Drawing.RectangleF[1] { new System.Drawing.RectangleF((float)v.X, (float)v.Y, (float)v.W, (float)v.H) });
                        //Lets draw the selectBoxes.
                        List<RectangleD> rects = new List<RectangleD>();
                        RectangleD[] sels = an.GetSelectBoxes(Tools.selectBoxSize);
                        for (int i = 0; i < an.selectedPoints.Count; i++)
                        {
                            if (an.selectedPoints[i] < an.GetPointCount())
                            {
                                rects.Add(sels[an.selectedPoints[i]]);
                            }
                        }
                        for (int i = 0; i < rects.Count; i++)
                        {
                            var va = ToScreenSpace(rects.ToArray());
                            g.DrawRectangle(blue, new System.Drawing.RectangleF((float)va[0].X, (float)va[0].Y, (float)va[0].W, (float)va[0].H));
                        }

                        rects.Clear();
                        //Lets draw the text of this ROI in the middle of the ROI
                        System.Drawing.Size s = System.Windows.Forms.TextRenderer.MeasureText(an.Text, new Font(an.family, an.fontSize));
                        float fw = ((float)an.BoundingBox.X + ((float)an.BoundingBox.W / 2)) - ((float)s.Width / 2);
                        float fh = ((float)an.BoundingBox.Y + ((float)an.BoundingBox.H / 2)) - ((float)s.Height / 2);
                        var vv = ToScreenSpace(new PointF(fw, fh));
                        g.DrawString(an.Text, fo, b, vv.X, vv.Y);
                    }
                    pen.Dispose();
                    red.Dispose();
                    mag.Dispose();
                    green.Dispose();
                    blue.Dispose();
                    b.Dispose();
                }
            }
        }
        /// It draws the overlay on the picturebox
        /// 
        /// @param sender The object that raised the event.
        /// @param PaintEventArgs e
        bool updatePyr = false;
        /// <summary>
        /// Draws the viewport when Hardware Acceleration is off.
        /// </summary>
        /// <param name="g"></param>
        public void DrawView(System.Drawing.Graphics g)
        {
            try
            {
                drawing = true;
                if (SelectedImage == null)
                    return;
                if (SelectedImage.isPyramidal)
                {
                    UpdateImages(true);
                    updatePyr = false;
                    return;
                }
                g.TranslateTransform(ViewWidth / 2, ViewHeight / 2);
                if (Scale.Width == 0 || float.IsInfinity(Scale.Width))
                    Scale = new SizeF(1, 1);
                g.ScaleTransform(Scale.Width, Scale.Height);
                if (Bitmaps.Count == 0)
                    return;
                System.Drawing.RectangleF[] rf = new System.Drawing.RectangleF[1];
                using System.Drawing.Pen blue = new System.Drawing.Pen(Brushes.Blue, 1 / Scale.Width);
                for (int i = 0; i < Images.Count && i < Bitmaps.Count; i++)
                {
                    BioImage im = Images[i];
                    if (!IsLayerEnabled(im.PhysicalSizeX) && !im.isPyramidal)
                        continue;
                    RectangleD r = ToScreenRect(im.Volume.Location.X, im.Volume.Location.Y, im.Volume.Width, im.Volume.Height);
                    double w = ToViewW(ViewWidth);
                    double h = ToViewH(ViewHeight);
                    RectangleF rg = new RectangleF((float)((-Origin.X) - (w / 2)), (float)((-Origin.Y) - (h / 2)), (float)(w), (float)(h));
                    RectangleF rec = new RectangleF((float)im.Volume.Location.X, (float)im.Volume.Location.Y, (float)im.Volume.Width, (float)im.Volume.Height);
                    if (SelectedImage.isPyramidal)
                    {
                        var tr = g.Transform;
                        g.ResetTransform();
                        g.DrawImage(Bitmaps[i], 0, 0);
                        g.Transform = tr;
                    }
                    else
                    if (rg.IntersectsWith(rec))
                    {
                        g.DrawImage(Bitmaps[i], (float)r.X, (float)r.Y, (float)r.W, (float)r.H);
                    }
                    if (i == SelectedIndex && !SelectedImage.isPyramidal)
                    {
                        rf[0] = new System.Drawing.RectangleF((float)r.X, (float)r.Y, (float)r.W, (float)r.H);
                        g.DrawRectangles(blue, rf);
                    }
                }
                update = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                drawing = false;
            }
        }
        private double scaleorig = 0;
        /// It draws the view.
        /// 
        /// @param sender The object that raised the event.
        /// @param PaintEventArgs The PaintEventArgs object that contains the event data.
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            var state = e.Graphics.Save();
            try
            {
                DrawView(e.Graphics);
                updateOverlay = true;
                DrawOverlay(e.Graphics);
            }
            finally
            {
                e.Graphics.Restore(state);
            }
            Plugins.Render(this, e);
            Plugins.Paint(sender, e);
        }
        /// GetScale() returns the scale of the image in the viewport.
        /// 
        /// @return The scale of the image.
        public double GetScale()
        {
            return ToViewSizeW(ROI.selectBoxSize / Scale.Width);
        }
        private System.Drawing.Bitmap ToSysBitmap(Bitmap bm)
        {
            // Own the display pixels: a Bitmap constructed over AForge's temporary
            // managed buffer would retain an unpinned pointer after this method returns.
            using Bitmap rgba = bm.GetImageRGBA();
            var result = new System.Drawing.Bitmap(rgba.Width, rgba.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = result.LockBits(new System.Drawing.Rectangle(0, 0, result.Width, result.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                int rowBytes = rgba.Width * 4;
                for (int y = 0; y < rgba.Height; y++)
                    Marshal.Copy(rgba.Bytes, y * rgba.Stride, IntPtr.Add(data.Scan0, y * data.Stride), rowBytes);
            }
            finally
            {
                result.UnlockBits(data);
            }
            return result;
        }
        PointD mouseD = new PointD(0, 0);
        PointD prevMove = new PointD();
        /// The function is called when the mouse is moved over the image. It updates the mouse
        /// position, and if the mouse is clicked, it updates the image
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs e.Location.X, e.Location.Y
        /// 
        /// @return The point in the image space.
        private void rgbPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PointD p = ImageToViewSpace(e.Location.X, e.Location.Y);
            tools.ToolMove(p, e.Button);
            mouseMove = p;
            MouseMoveInt = new PointD(e.X, e.Y);
            if (SelectedImage == null)
                return;
            if (SelectedImage != null)
            {
                PointD ip = SelectedImage.ToImageSpace(p);
                mousePoint = $"({p.X:0.##}, {p.Y:0.##})";
            }
            if (e.Button == MouseButtons.XButton1 && !x1State && !Ctrl && Mode != ViewMode.RGBImage)
            {
                if (cBar.Value < cBar.Maximum)
                    cBar.Value++;
                x1State = true;
            }
            else if (e.Button == MouseButtons.XButton1 && !x1State && Ctrl)
            {
                if (tBar.Value < tBar.Maximum)
                    tBar.Value++;
                x1State = true;
            }
            if (e.Button != MouseButtons.XButton1)
                x1State = false;

            if (e.Button == MouseButtons.XButton2 && !x2State && !Ctrl && Mode != ViewMode.RGBImage)
            {
                if (cBar.Value > cBar.Minimum)
                    cBar.Value--;
                x2State = true;
            }
            else if (e.Button == MouseButtons.XButton2 && !x2State && Ctrl)
            {
                if (tBar.Value > tBar.Minimum)
                    tBar.Value--;
                x2State = true;
            }
            if (e.Button != MouseButtons.XButton2)
                x2State = false;

            if (Tools.currentTool.type == Tools.Tool.Type.move && e.Button == MouseButtons.Left)
            {
                foreach (ROI an in selectedAnnotations)
                {
                    if (an.selectedPoints.Count > 0 && an.selectedPoints.Count < an.GetPointCount())
                    {
                        if (an.type == ROI.Type.Rectangle || an.type == ROI.Type.Ellipse)
                        {
                            RectangleD d = an.BoundingBox;
                            if (an.selectedPoints[0] == 0)
                            {
                                double dw = d.X - p.X;
                                double dh = d.Y - p.Y;
                                d.X = p.X;
                                d.Y = p.Y;
                                d.W += dw;
                                d.H += dh;
                            }
                            else
                            if (an.selectedPoints[0] == 1)
                            {
                                double dw = p.X - (d.W + d.X);
                                double dh = d.Y - p.Y;
                                d.W += dw;
                                d.H += dh;
                                d.Y -= dh;
                            }
                            else
                            if (an.selectedPoints[0] == 2)
                            {
                                double dw = d.X - p.X;
                                double dh = p.Y - (d.Y + d.H);
                                d.W += dw;
                                d.H += dh;
                                d.X -= dw;
                            }
                            else
                            if (an.selectedPoints[0] == 3)
                            {
                                double dw = d.X - p.X;
                                double dh = d.Y - p.Y;
                                d.W = p.X - an.X;
                                d.H = p.Y - an.Y;
                            }
                            an.BoundingBox = d;
                        }
                        else
                        {
                            PointD pod = new PointD(p.X - pd.X, p.Y - pd.Y);
                            for (int i = 0; i < an.selectedPoints.Count; i++)
                            {
                                PointD poid = an.GetPoint(an.selectedPoints[i]);
                                an.UpdatePoint(new PointD(poid.X + pod.X, poid.Y + pod.Y), an.selectedPoints[i]);
                            }
                        }
                    }
                    else
                    {
                        PointD pod = new PointD(p.X - pd.X, p.Y - pd.Y);
                        for (int i = 0; i < an.GetPointCount(); i++)
                        {
                            PointD poid = an.Points[i];
                            an.UpdatePoint(new PointD(poid.X + pod.X, poid.Y + pod.Y), i);
                        }
                    }
                }
                UpdateOverlay();
            }

            if (Tools.currentTool != null)
                if (Tools.currentTool.type == Tools.Tool.Type.pencil && e.Button == MouseButtons.Left)
                {
                    PointD ip = SelectedImage.ToImageSpace(p);
                    Tools.Tool tool = Tools.currentTool;
                    Graphics g = Graphics.FromImage(SelectedBuffer);
                    Pen pen = new Pen(Tools.DrawColor, (int)Tools.StrokeWidth, ImageView.SelectedImage.bitsPerPixel);
                    g.FillEllipse(new Rectangle((int)ip.X, (int)ip.Y, (int)Tools.StrokeWidth, (int)Tools.StrokeWidth), pen.color);
                    UpdateImage();
                }


            UpdateStatus();
            prevMove = p;
            pd = p;
        }
        /// The function is called when the mouse is released
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs e.Location.X, e.Location.Y
        /// 
        /// @return The mouse location in the picture box.
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            PointD p = ImageToViewSpace(e.Location.X, e.Location.Y);
            mouseUpButtons = e.Button;
            mouseDownButtons = MouseButtons.None;
            MouseUpInt = new PointD(e.X, e.Y);
            mouseUp = p;
            down = false;
            up = true;
            if (SelectedImage == null)
                return;
            App.viewer = this;
            tools.ToolUp(p, e.Button);
        }
        /// The function is called when the mouse is pressed down on the picturebox
        /// 
        /// @param sender the object that raised the event
        /// @param MouseEventArgs e
        /// 
        /// @return The mouseDownButtons is being returned.
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            App.viewer = this;
            MouseDownInt = new PointD(e.X, e.Y);
            mouseDownButtons = e.Button;
            mouseUpButtons = MouseButtons.None;
            PointD p = ImageToViewSpace(e.Location.X, e.Location.Y);
            pd = new PointD(p.X, p.Y);
            mouseDown = pd;
            mouseD = new PointD(e.Location.X, e.Location.Y);
            down = true;
            up = false;
            PointD ip;
            if (SelectedImage == null)
                return;
            // Middle drag always starts a pan, including over the overview. Keep
            // focus on the canvas instead of activating the floating tools window.
            if (e.Button == MouseButtons.Middle)
            {
                tools.ToolDown(mouseDown, e.Button);
                return;
            }
            ip = SelectedImage.ToImageSpace(p);
            tools.BringToFront();
            int ind = 0;
            foreach (BioImage b in Images)
            {
                RectangleD r = new RectangleD(b.Volume.Location.X, b.Volume.Location.Y, b.Volume.Width, b.Volume.Height);
                if (r.IntersectsWith(p))
                {
                    b.selected = true;
                    SelectedIndex = ind;
                }
                else
                    b.selected = false;
                ind++;
            }
            if (!Ctrl && e.Button == MouseButtons.Left && Tools.currentTool.type != Tools.Tool.Type.rectSel)
            {
                foreach (ROI item in selectedAnnotations)
                {
                    if (item.Selected)
                        item.selectedPoints.Clear();
                }
                selectedAnnotations.Clear();
            }

            if (Tools.currentTool.type == Tools.Tool.Type.move && e.Button == MouseButtons.Left)
            {
                float width = (float)ToViewSizeW(ROI.selectBoxSize / Scale.Width);
                float height = (float)ToViewSizeH(ROI.selectBoxSize / Scale.Height);
                if (openSlide)
                {
                    width = Tools.selectBoxSize;
                    height = Tools.selectBoxSize;
                }
                foreach (BioImage bi in Images)
                {
                    foreach (ROI an in bi.Annotations)
                    {
                        if (an.GetSelectBound(width, height).IntersectsWith(new RectangleD(p.X, p.Y, 1, 1)))
                        {
                            selectedAnnotations.Add(an);
                            an.Selected = true;
                            RectangleD[] sels = an.GetSelectBoxes(Tools.selectBoxSize);
                            RectangleD r = new RectangleD(p.X, p.Y, sels[0].W, sels[0].H);
                            for (int i = 0; i < sels.Length; i++)
                            {
                                if (sels[i].IntersectsWith(r))
                                {
                                    an.selectedPoints.Add(i);
                                }
                            }
                        }
                        else
                            if (!Ctrl)
                            an.Selected = false;
                    }
                }
                UpdateOverlay();
            }

            if (e.Button == MouseButtons.Left)
            {
                AForge.Point s = new AForge.Point(SelectedImage.SizeX, SelectedImage.SizeY);
                if ((ip.X < s.X && ip.Y < s.Y) || (ip.X >= 0 && ip.Y >= 0))
                {
                    int zc = SelectedImage.Coordinate.Z;
                    int cc = SelectedImage.Coordinate.C;
                    int tc = SelectedImage.Coordinate.T;
                    if (SelectedImage.isPyramidal)
                    {
                        if (SelectedImage.isRGB)
                        {
                            int r = (int)SelectedImage.GetValueRGB(zc, RChannel.Index, tc, e.X, e.Y, 0);
                            int g = (int)SelectedImage.GetValueRGB(zc, GChannel.Index, tc, e.X, e.Y, 1);
                            int b = (int)SelectedImage.GetValueRGB(zc, BChannel.Index, tc, e.X, e.Y, 2);
                            mouseColor = ", " + r + "," + g + "," + b;
                        }
                        else
                        {
                            int r = (int)SelectedImage.GetValueRGB(zc, 0, tc, e.X, e.Y, 0);
                            mouseColor = ", " + r;
                        }
                    }
                    else
                    {
                        if (SelectedImage.isRGB)
                        {
                            int r = (int)SelectedImage.GetValueRGB(zc, RChannel.Index, tc, (int)ip.X, (int)ip.Y, 0);
                            int g = (int)SelectedImage.GetValueRGB(zc, GChannel.Index, tc, (int)ip.X, (int)ip.Y, 1);
                            int b = (int)SelectedImage.GetValueRGB(zc, BChannel.Index, tc, (int)ip.X, (int)ip.Y, 2);
                            mouseColor = ", " + r + "," + g + "," + b;
                        }
                        else
                        {
                            int r = (int)SelectedImage.GetValueRGB(zc, 0, tc, (int)ip.X, (int)ip.Y, 0);
                            mouseColor = ", " + r;
                        }
                    }
                }
            }
            UpdateStatus();
            tools.ToolDown(mouseDown, e.Button);
        }
        /// > If the user double clicks on the picture box, then the selected image is set to the image
        /// that the user double clicked on
        /// 
        /// @param sender The object that raised the event.
        /// @param MouseEventArgs e.Location.X, e.Location.Y
        /// 
        /// @return The return value is a PointD object.
        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedImage == null)
                return;
            App.viewer = this;
            PointD p = ToViewSpace(e.Location.X, e.Location.Y);
            tools.ToolDown(p, e.Button);
            if (e.Button != MouseButtons.XButton1 && e.Button != MouseButtons.XButton2)
                Origin = new PointD(-p.X, -p.Y);
        }
        /// If the user has selected a ROI, then delete it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The selected ROI is being returned.
        private void deleteROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            foreach (ROI item in AnnotationsRGB)
            {
                if (item.Selected && (item.selectedPoints.Count == 0 || item.selectedPoints.Count == item.GetPointCount() || item.type == ROI.Type.Ellipse || item.type == ROI.Type.Rectangle))
                {
                    SelectedImage.Annotations.Remove(item);
                }
                else
                {
                    if ((item.type == ROI.Type.Polygon || item.type == ROI.Type.Freeform || item.type == ROI.Type.Polyline) && item.selectedPoints.Count > 0)
                    {
                        item.RemovePoints(item.selectedPoints.ToArray());

                    }
                    item.selectedPoints.Clear();
                }
            }
            UpdateOverlay();
        }
        /// This function is used to set the text of the selected ROI
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        /// 
        /// @return The DialogResult.OK is being returned.
        private void setTextSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedImage == null)
                return;
            foreach (ROI item in AnnotationsRGB)
            {
                if (item.Selected)
                {
                    TextInput input = new TextInput(item.Text);
                    if (input.ShowDialog() != DialogResult.OK)
                        return;
                    item.Text = input.TextValue;
                    item.family = input.font.FontFamily.Name;
                    item.fontSize = input.font.Size;
                    item.strokeColor = AForge.Color.FromArgb(input.color.R, input.color.G, input.color.B);
                    UpdateOverlay();
                }
            }
        }

        /// It creates a bitmap, draws the view and overlay to it, and then copies it to the clipboard
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void copyViewToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ViewWidth, ViewHeight);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                DrawView(g);
                DrawOverlay(g);
            }
            System.Windows.Forms.Clipboard.SetImage(bmp);
        }
        List<ROI> copys = new List<ROI>();

        /// It takes the selected ROIs and copies them to the clipboard
        public void CopySelection()
        {
            copys.Clear();
            string s = "";
            foreach (ROI item in AnnotationsRGB)
            {
                if (item.Selected)
                {
                    copys.Add(item);
                    s += BioImage.ROIToString(item);
                }
            }
            System.Windows.Forms.Clipboard.SetText(s);
        }
        /// We get the text from the clipboard, split it into lines, and then for each line, if it's
        /// longer than 8 characters, we convert it to an ROI and add it to the image
        public void PasteSelection()
        {
            string[] sts = System.Windows.Forms.Clipboard.GetText().Split(BioImage.NewLine);
            foreach (string line in sts)
            {
                if (line.Length > 8)
                {
                    ROI an = BioImage.StringToROI(line);
                    //We set the coordinates of the ROI's we are pasting
                    an.coord = GetCoordinate();
                    SelectedImage.Annotations.Add(an);
                }
            }
            UpdateOverlay();
        }
        /// CopySelection() copies the selected region of the image to the clipboard
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void copyROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySelection();
        }

        /// PasteSelection() is a function that takes the current selection and pastes it into the
        /// current image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event data.
        private void pasteROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteSelection();
        }

        /// If the ShowControls variable is true, set it to false. Otherwise, set it to true
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void hideControlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowControls)
                ShowControls = false;
            else
                ShowControls = true;
        }

        /// If the ShowControls variable is true, set it to false. Otherwise, set it to true
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void showControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowControls)
                ShowControls = false;
            else
                ShowControls = true;
        }

        /// It sets the value of the ShowStatus variable to false
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void HideStatusMenuItem_Click(object sender, EventArgs e)
        {
            ShowStatus = false;
        }
        /// When the user clicks on the RGB menu item, set the mode to RGB.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void rGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.RGBImage;
        }

        /// The function is called when the user clicks on the "Filtered" menu item
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void filteredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.Filtered;
        }

        /// If the user clicks on the "Raw" menu item, then set the Mode property to ViewMode.Raw
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes that contain event data.
        private void rawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.Raw;
        }
        /// It takes a point in the image space and returns the point in the view space
        /// 
        /// @param x the x coordinate of the point in the image
        /// @param y the y coordinate of the point in the image
        /// 
        /// @return The point in the image space that corresponds to the point in the view space.
        public PointD ImageToViewSpace(double x, double y)
        {
            if (SelectedImage == null)
                return ToViewSpace(x, y);
            if (SelectedImage.isPyramidal)
            {
                return new PointD((PyramidalOrigin.X + x) * Resolution, (PyramidalOrigin.Y + y) * Resolution);
            }
            else
                return ToViewSpace(x, y);
        }
        /// Convert a point from world space to view space
        /// 
        /// @param PointF The point to convert
        /// 
        /// @return A PointD object.
        public PointF ToViewSpace(PointF p)
        {
            PointD d = ToViewSpace(p.X, p.Y);
            return new PointF((float)d.X, (float)d.Y);
        }
        /// > Converts a point from world space to view space
        /// 
        /// @param PointD A class that contains an X and Y value.
        /// 
        /// @return A PointD object.
        public PointD ToViewSpace(PointD p)
        {
            return ToViewSpace(p.X, p.Y); ;
        }
        /// > ToViewSpace(x, y) = (ToViewSizeW(x - (ViewWidth / 2)) / Scale.Width) - Origin.X;
        /// 
        /// @param x The x coordinate of the point to convert
        /// @param y The y coordinate of the point to convert.
        /// 
        /// @return A PointD object.
        public PointD ToViewSpace(double x, double y)
        {
            if (SelectedImage != null)
                if (SelectedImage.isPyramidal)
                {
                    double ddx = x / Resolution;
                    double ddy = y / Resolution;
                    return new PointD(ddx, ddy);
                }
            double dx, dy;
            dx = (ToViewSizeW(x - (ViewWidth / 2)) / Scale.Width) - Origin.X;
            dy = (ToViewSizeH(y - (ViewHeight / 2)) / Scale.Height) - Origin.Y;
            return new PointD(dx, dy);
        }
        /// Convert a value in microns to a value in pixels
        /// 
        /// @param d the size in microns
        /// 
        /// @return The return value is the size of the object in pixels.
        private double ToViewSizeW(double d)
        {
            if (SelectedImage != null)
                if (SelectedImage.isPyramidal)
                {
                    return d / Resolution;
                }
            double x = (double)(d / PxWmicron);
            return x;
        }
        /// > Convert a value in microns to a value in pixels
        /// 
        /// @param d the size in microns
        /// 
        /// @return The return value is the size of the object in pixels.
        public double ToViewSizeH(double d)
        {
            if (SelectedImage != null)
                if (SelectedImage.isPyramidal)
                {
                    return d / Resolution;
                }
            double y = (double)(d / PxHmicron);
            return y;
        }
        /// Convert a distance in microns to a distance in pixels on the screen
        /// 
        /// @param d the distance in microns
        /// 
        /// @return The width of the image in pixels.
        public double ToViewW(double d)
        {
            double x = (double)(d / PxWmicron) / scale.Width;
            return x;
        }
        /// > Convert a distance in microns to a distance in pixels
        /// 
        /// @param d the distance in microns
        /// 
        /// @return The return value is the y-coordinate of the point in the view.
        public double ToViewH(double d)
        {
            double y = (double)(d / PxHmicron) / scale.Height;
            return y;
        }
        /// > It converts a point in world space to a point in screen space
        /// 
        /// @param x The x coordinate of the point to convert.
        /// @param y The y coordinate of the point to transform.
        /// 
        /// @return A PointD object.
        public PointD ToScreenSpace(double x, double y)
        {
            double fx = ToScreenScaleW(Origin.X + x);
            double fy = ToScreenScaleH(Origin.Y + y);
            return new PointD(fx, fy);
        }
        /// > Converts a point from world space to screen space
        /// 
        /// @param PointD A class that contains an X and Y value.
        /// 
        /// @return A PointD object.
        public PointD ToScreenSpace(PointD p)
        {
            return ToScreenSpace(p.X, p.Y);
        }
        /// Convert a point in the world coordinate system to the screen coordinate system
        /// 
        /// @param PointF The point you want to convert to screen space.
        /// 
        /// @return A PointD object.
        public PointF ToScreenSpace(PointF p)
        {
            PointD pd = ToScreenSpace(p.X, p.Y);
            return new PointF((float)pd.X, (float)pd.Y);
        }
        /// > It takes an array of points and returns an array of points
        /// 
        /// @param p The point to convert
        /// 
        /// @return A PointF array.
        public PointF[] ToScreenSpace(PointF[] p)
        {
            PointF[] pf = new PointF[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                pf[i] = ToScreenSpace(p[i]);
            }
            return pf;
        }
        /// > It converts a 3D point to a 2D point
        /// 
        /// @param Point3D 
        /// 
        /// @return A PointF object.
        public PointF ToScreenSpace(Point3D p)
        {
            PointD pd = ToScreenSpace(p.X, p.Y);
            return new PointF((float)pd.X, (float)pd.Y);
        }
        /// ToScreenScaleW() returns the number of pixels that correspond to the given number of microns
        /// 
        /// @param x the x coordinate of the point to be converted
        /// 
        /// @return The return value is a float.
        public float ToScreenScaleW(double x)
        {
            return (float)(x * PxWmicron);
        }
        /// > Convert a value in microns to a value in pixels
        /// 
        /// @param y the y coordinate of the point to be converted
        /// 
        /// @return The return value is a float.
        public float ToScreenScaleH(double y)
        {
            return (float)(y * PxHmicron);
        }
        /// > Convert a point in the world coordinate system to a point in the screen coordinate system
        /// 
        /// @param PointD 
        /// 
        /// @return A PointF object.
        public PointF ToScreenScale(PointD p)
        {
            float x = ToScreenScaleW((float)p.X);
            float y = ToScreenScaleH((float)p.Y);
            return new PointF(x, y);
        }
        /// It converts a rectangle in microns to a rectangle in pixels
        /// 
        /// @param x The x coordinate of the rectangle
        /// @param y -0.0015
        /// @param w width of the image in microns
        /// @param h height of the rectangle
        /// 
        /// @return A RectangleF object.
        public RectangleD ToScreenRect(double x, double y, double w, double h)
        {
            if (SelectedImage == null)
            {
                PointD pf;
                pf = ToScreenSpace(x, y);
                return new RectangleD(pf.X, pf.Y, ToScreenScaleW(w), ToScreenScaleH(h));
            }
            if (SelectedImage.isPyramidal)
            {
                PointD d = ToViewSpace(x, y);
                double dw = ToViewSizeW(w);
                double dh = ToViewSizeH(h);
                return new RectangleD((d.X - PyramidalOrigin.X), (d.Y - PyramidalOrigin.Y), dw, dh);
            }
            else
            {
                PointD pf = ToScreenSpace(x, y);
                return new RectangleD(pf.X, pf.Y, ToScreenScaleW(w), ToScreenScaleH(h));
            }
        }
        /// > It takes a rectangle in the coordinate system of the stage and returns a rectangle in the
        /// coordinate system of the picturebox
        /// 
        /// @param x The x coordinate of the upper-left corner of the rectangle.
        /// @param y The y-coordinate of the upper-left corner of the rectangle.
        /// @param w width of the rectangle
        /// @param h height of the rectangle
        /// 
        /// @return A SKRect object.
        public SKRect ToSKRect(double x, double y, double w, double h)
        {
            double xd = gl.Width / 2;
            double yd = gl.Height / 2;
            return new SKRect((float)(xd - x), (float)(yd - y), (float)(xd - (x + w)), (float)(yd - (y + h)));
        }
        /// > It converts a rectangle from world space to screen space
        /// 
        /// @param RectangleD The rectangle to convert.
        /// 
        /// @return A RectangleF object.
        public RectangleD ToScreenSpace(RectangleD p)
        {
            return ToScreenRect(p.X, p.Y, p.W, p.H);
        }
        /// > It converts a rectangle from world space to screen space
        /// 
        /// @param RectangleF The rectangle to convert.
        /// 
        /// @return A RectangleF object.
        public RectangleD ToScreenSpace(RectangleF p)
        {
            return ToScreenRect(p.X, p.Y, p.Width, p.Height);
        }
        /// It takes an array of RectangleD objects and returns an array of RectangleF objects
        /// 
        /// @param p The rectangle to convert
        /// 
        /// @return A RectangleF[]
        public RectangleD[] ToScreenSpace(RectangleD[] p)
        {
            RectangleD[] rs = new RectangleD[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                rs[i] = ToScreenSpace(p[i]);
            }
            return rs;
        }
        /// It takes an array of RectangleF objects and returns an array of RectangleF objects
        /// 
        /// @param p The rectangle to convert
        /// 
        /// @return A RectangleF[]
        public RectangleF[] ToScreenSpace(RectangleF[] p)
        {
            RectangleF[] rs = new RectangleF[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                rs[i] = ToScreenSpace(p[i]).ToRectangleF();
            }
            return rs;
        }
        /// > Convert a list of points from world space to screen space
        /// 
        /// @param p The point to convert
        /// 
        /// @return A PointF[] array of points.
        public PointF[] ToScreenSpace(PointD[] p)
        {
            PointF[] rs = new PointF[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                PointD pd = ToScreenSpace(p[i]);
                rs[i] = new PointF((float)pd.X, (float)pd.Y);
            }
            return rs;
        }
        /// ToScreenW(x) = x * PxWmicron
        /// 
        /// @param x the x coordinate of the point to be converted
        /// 
        /// @return The return value is a float.
        public float ToScreenW(double x)
        {
            return (float)(x * PxWmicron);
        }
        /// > Convert a value in microns to a value in pixels
        /// 
        /// @param y the y coordinate of the point to be converted
        /// 
        /// @return The return value is a float.
        public float ToScreenH(double y)
        {
            return (float)(y * PxHmicron);
        }
        /// If the user presses the "C" key while holding down the "Control" key, then the function
        /// "CopySelection" is called
        /// 
        /// @param sender The object that raised the event.
        /// @param KeyEventArgs The event arguments for the key press.
        /// 
        /// @return The return value is a PointD object.
        public void ImageView_KeyDown(object sender, KeyEventArgs e)
        {
            double moveAmount = 5 * Scale.Width;
            Plugins.KeyDownEvent(sender, e);
            if (e.KeyCode == Keys.C && e.Control)
            {
                CopySelection();
                return;
            }
            if (e.KeyCode == Keys.V && e.Control)
            {
                PasteSelection();
                return;
            }
            if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.NumPad7)
            {
                if (SelectedImage.isPyramidal)
                    Resolution *= 0.9f;
                else
                    Scale = new SizeF(Scale.Width - 0.1f, Scale.Height - 0.1f);
                UpdateView();
            }
            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.NumPad9)
            {
                if (SelectedImage.isPyramidal)
                    Resolution *= 1.1f;
                else
                    Scale = new SizeF(Scale.Width + 0.1f, Scale.Height + 0.1f);
                UpdateView();
            }
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.NumPad8)
            {
                if (SelectedImage.isPyramidal)
                    PyramidalOrigin = new PointD(PyramidalOrigin.X, PyramidalOrigin.Y - moveAmount);
                else
                    Origin = new PointD(Origin.X, Origin.Y + moveAmount);
                UpdateView();
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.NumPad2)
            {
                if (SelectedImage.isPyramidal)
                    PyramidalOrigin = new PointD(PyramidalOrigin.X, PyramidalOrigin.Y + moveAmount);
                else
                    Origin = new PointD(Origin.X, Origin.Y - moveAmount);
                UpdateView();
            }
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.NumPad4)
            {
                if (SelectedImage.isPyramidal)
                    PyramidalOrigin = new PointD(PyramidalOrigin.X - moveAmount, PyramidalOrigin.Y);
                else
                    Origin = new PointD(Origin.X + moveAmount, Origin.Y);
                UpdateView();
            }
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.NumPad6)
            {
                if (SelectedImage.isPyramidal)
                    PyramidalOrigin = new PointD(PyramidalOrigin.X + moveAmount, PyramidalOrigin.Y);
                else
                    Origin = new PointD(Origin.X - moveAmount, Origin.Y);
                UpdateView();
            }
        }
        public void ImageView_KeyUp(object sender, KeyEventArgs e)
        {
            Plugins.KeyUpEvent(sender, e);
        }
        public void ImageView_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Plugins.KeyPressEvent(sender, e);
        }
        /// If the selected image is not null, set the origin to the center of the image, and set the
        /// scale to the height of the image.
        /// 
        /// @return The method is returning the value of the variable "Scale"
        public void GoToImage()
        {
            GoToImage(SelectedIndex);
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD PyramidalOriginTransformed
        {
            get { return new PointD(PyramidalOrigin.X * Resolution, PyramidalOrigin.Y * Resolution); }
            set { PyramidalOrigin = new PointD(value.X / Resolution, value.Y / Resolution); }
        }
        PointD mouseDownInt = new PointD(0, 0);
        PointD mouseUpInt = new PointD(0, 0);
        PointD mouseMoveInt = new PointD(0, 0);
        PointD mouseMove = new PointD(0, 0);
        /* A property that returns the value of the mouseDownInt variable. */
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD MouseDownInt
        {
            get { return mouseDownInt; }
            set { mouseDownInt = value; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD MouseUpInt
        {
            get { return mouseUpInt; }
            set { mouseUpInt = value; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointD MouseMoveInt
        {
            get { return mouseMoveInt; }
            set { mouseMoveInt = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMasks { get; set; } = true;

        /// It takes an image index and centers the image in the viewport
        /// 
        /// @param i the index of the image to go to
        /// 
        /// @return The method is returning the value of the variable "i"
        public void GoToImage(int i)
        {
            if (i < 0 || Images.Count <= i || ViewWidth <= 0 || ViewHeight <= 0)
                return;
            App.viewer = this;
            if (SelectedIndex != i)
                SelectedIndex = i;
            else
                selectedIndex = i;
            BioImage image = Images[i];
            if (image.isPyramidal)
            {
                // The schema extent uses the same world coordinates as the slide
                // renderer. BioImage.Volume may only describe its initial Zarr tile.
                RectangleD bounds = OverviewWorldBounds(image);
                if (bounds.W <= 0 || bounds.H <= 0 || !double.IsFinite(bounds.W) || !double.IsFinite(bounds.H))
                    return;
                double fit = Math.Max(bounds.W / ViewWidth, bounds.H / ViewHeight);
                if (!double.IsFinite(fit) || fit <= 0)
                    return;
                Resolution = fit;
                PyramidalOrigin = new PointD((bounds.X + bounds.W / 2) / fit - ViewWidth / 2.0,
                    (bounds.Y + bounds.H / 2) / fit - ViewHeight / 2.0);
                UpdateView();
                return;
            }
            double dx = image.Volume.Width / 2;
            double dy = image.Volume.Height / 2;
            Origin = new PointD(-(image.Volume.Location.X + dx), -(image.Volume.Location.Y + dy));
            double wx, wy;
            wx = ViewWidth / ToScreenScaleW(image.Volume.Width);
            wy = ViewHeight / ToScreenScaleH(image.Volume.Height);
            double fitScale = Math.Min(wx, wy);
            if (double.IsFinite(fitScale) && fitScale > 0)
                Scale = new SizeF((float)fitScale, (float)fitScale);
            UpdateView();
        }

        /// It opens a new form, and then it opens a new image in that form.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void goToImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToImage();
        }
        /// The function is called when the user clicks on the "Go To" menu item. It sets the origin to
        /// the point where the user clicked
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void goToToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Origin = new PointD(mouseDown.X, mouseDown.Y);
        }
        /// > When the user clicks on the "Go to Image" menu item, the program will clear the menu
        /// item's drop down menu and then add a menu item for each image in the Images list
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void goToImageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            item.DropDownItems.Clear();
            foreach (BioImage im in Images)
            {
                ToolStripMenuItem it = new ToolStripMenuItem();
                it.Text = im.filename;
                item.DropDownItems.Add(it);
            }
        }
        /// When the user clicks on an item in the dropdown menu, the function finds the index of the
        /// image in the list of images and then calls the GoToImage function with that index
        /// 
        /// @param sender System.Object
        /// @param ToolStripItemClickedEventArgs 
        /// 
        /// @return The filename of the image that was clicked on.
        private void goToImageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int i = 0;
            foreach (var item in Images)
            {
                if (item.filename == e.ClickedItem.Text)
                {
                    GoToImage(i);
                    return;
                }
                i++;
            }
        }
        public new void Dispose()
        {
            for (int i = 0; i < Bitmaps.Count; i++)
            {
                if (Bitmaps[i] != null)
                    Bitmaps[i].Dispose();
            }
            for (int i = 0; i < Bitmaps.Count; i++)
            {
                if (Bitmaps[i] != null)
                    Bitmaps[i].Dispose();
            }
        }

        /// The function is called when the user clicks on the "Go To" menu item in the context menu.
        /// The function sets the stage position to the position of the mouse click
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void goToToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Microscope.Stage.SetPosition(mouseDown.X, mouseDown.Y);
        }

        /// It draws the selected ROI's on the image
        /// 
        /// @param sender
        /// @param EventArgs e
        private void drawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromImage(SelectedBuffer);
            foreach (ROI item in AnnotationsRGB)
            {
                Pen p = new Pen(Tools.DrawColor, (int)Tools.StrokeWidth, SelectedBuffer.BitsPerPixel);
                g.pen = p;
                if (item.Selected)
                {
                    if (item.type == ROI.Type.Line)
                    {
                        g.DrawLine(new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(0)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(0)).Y), new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(1)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(1)).Y));
                    }
                    else
                    if (item.type == ROI.Type.Rectangle)
                    {
                        g.DrawRectangle(SelectedImage.ToImageSpace(item.BoundingBox));
                    }
                    else
                    if (item.type == ROI.Type.Ellipse)
                    {
                        g.DrawEllipse(SelectedImage.ToImageSpace(item.BoundingBox));
                    }
                    else
                    if (item.type == ROI.Type.Freeform || item.type == ROI.Type.Polygon || item.type == ROI.Type.Polyline)
                    {
                        if (item.closed)
                        {
                            for (int i = 0; i < item.GetPointCount() - 1; i++)
                            {
                                g.DrawLine(new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(i)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(i)).Y), new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(i + 1)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(i + 1)).Y));
                            }
                            g.DrawLine(new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(0)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(0)).Y), new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(item.GetPointCount() - 1)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(item.GetPointCount() - 1)).Y));

                        }
                        else
                        {
                            for (int i = 0; i < item.GetPointCount() - 1; i++)
                            {
                                g.DrawLine(new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(i)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(i)).Y), new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(i + 1)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(i + 1)).Y));
                            }
                        }
                    }
                }
            }
            update = true;
            UpdateImage();
        }

        /// It takes the selected ROI's and draws them on the image
        /// 
        /// @param sender
        /// @param EventArgs e
        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromImage(SelectedBuffer);
            foreach (ROI item in AnnotationsRGB)
            {
                Pen p = new Pen(Tools.DrawColor, (int)Tools.StrokeWidth, SelectedBuffer.BitsPerPixel);
                if (item.Selected)
                {
                    if (item.type == ROI.Type.Line)
                    {
                        g.DrawLine(new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(0)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(0)).Y), new PointF((float)SelectedImage.ToImageSpace(item.GetPoint(1)).X, (float)SelectedImage.ToImageSpace(item.GetPoint(1)).Y));
                    }
                    else
                    if (item.type == ROI.Type.Rectangle)
                    {
                        g.FillRectangle(SelectedImage.ToImageSpace(item.BoundingBox), p.color);
                    }
                    else
                    if (item.type == ROI.Type.Ellipse)
                    {
                        g.FillEllipse(SelectedImage.ToImageSpace(item.BoundingBox), p.color);
                    }
                    else
                    if (item.type == ROI.Type.Freeform || item.type == ROI.Type.Polygon || item.type == ROI.Type.Polyline)
                    {
                        g.FillPolygon(SelectedImage.ToImageSpace(item.GetPointsF()), SelectedImage.ToImageSpace(item.BoundingBox), p.color);
                    }
                }
            }
            update = true;
            UpdateImage();
        }

        /// Move the stage to the location of the selected image
        /// 
        /// @return The method is returning the value of the SelectedImage property.
        public void MoveStageToImage()
        {
            if (SelectedImage == null)
                return;
            Microscope.Stage.SetPosition(SelectedImage.Volume.Location.X, SelectedImage.Volume.Location.Y);
            update = true;
            UpdateView();
        }
        /// Move the stage to the location of the image at index i
        /// 
        /// @param i the index of the image in the list of images
        public void MoveStageToImage(int i)
        {
            update = true;
            Microscope.Stage.SetPosition(Images[i].Volume.Location.X, Images[i].Volume.Location.Y);
            UpdateView();
        }

        /// It moves the stage to the image that was clicked on in the drop down menu
        /// 
        /// @param sender System.Windows.Forms.ToolStripMenuItem
        /// @param ToolStripItemClickedEventArgs 
        /// 
        /// @return The index of the image in the list.
        private void moveStageToImageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int i = 0;
            foreach (var item in Images)
            {
                if (item.ID.Contains(e.ClickedItem.Text))
                {
                    MoveStageToImage(i);
                    return;
                }
                i++;
            }
        }

        /// Move the stage to the center of the image
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void moveStageToImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveStageToImage();
        }
        private void layersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layersTool == null)
                layersTool = new Layers();
            layersTool.Show();
        }
        /// If the image is pyramidal, update the image and update the view
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs 
        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            if (ViewWidth < 2 || ViewHeight < 2) return;
            if (SelectedImage.isPyramidal)
            {
                if (SelectedImage.isPyramidal)
                    UpdateImages();
            }
        }
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            App.viewer = this;
            if (App.tabsView != null)
                Function.Initialize();
        }
        private void hideOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hideOverViewToolStripMenuItem.Text == "Hide Overview")
            {
                ShowOverview = false;
                hideOverViewToolStripMenuItem.Text = "Show Overview";
            }
            else
            {
                ShowOverview = true;
                hideOverViewToolStripMenuItem.Text = "Hide Overview";
            }
        }
        private void removeImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveImage(SelectedIndex);
        }

        private void removeImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveImages();
        }
        #region OpenSlide
        private OpenSlideBase _openSlideBase;
        private SlideBase _slideBase;

        /// <summary>
        /// Open slide file
        /// </summary>
        private void Initialize(string file, BioImage bm)
        {
            if (bm.Resolutions.Count == 1)
                return;
            if (bm.OpenSlideBase == null)
            {
                _slideBase = bm.SlideBase;
                slideRenderer = new SlideRenderer(gl);
                slideRenderer.SetSource(_slideBase);
            }
            else
            {
                _openSlideBase = bm.OpenSlideBase;
                slideRenderer = new SlideRenderer(gl);
                slideRenderer.SetSource(_openSlideBase);
            }
        }

        #endregion

        private void viewpanel_Paint(object sender, PaintEventArgs e)
        {
            pictureBox_Paint(sender, e);
        }

        private void hideAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStage = false;
            ShowOverview = false;
            ShowStatus = false;
            ShowControls = false;
        }

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStage = true;
            ShowOverview = true;
            ShowStatus = true;
            ShowControls = true;
        }
    }
}
