using AForge;
using System.ComponentModel;
using Drawing = System.Drawing;

namespace BioCore
{
    public partial class ImageView
    {
        private readonly PictureBox overviewPanel = new PictureBox();
        private BioImage overviewSource;
        private BioImage previewRequestImage;
        private (int Z, int C, int T) previewRequestCoordinate;
        private int previewVersion;
        private bool overviewDragging;
        private bool showOverview = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOverview
        {
            get => showOverview;
            set
            {
                showOverview = value;
                UpdateOverviewVisibility();
            }
        }

        private void InitializeOverview()
        {
            overviewPanel.Name = "overviewPanel";
            overviewPanel.BackColor = Drawing.Color.Black;
            overviewPanel.Cursor = Cursors.Cross;
            overviewPanel.Visible = false;
            overviewPanel.ContextMenuStrip = contextMenuStrip;
            overviewPanel.Paint += OverviewPaint;
            overviewPanel.MouseDown += OverviewMouseDown;
            overviewPanel.MouseMove += OverviewMouseMove;
            overviewPanel.MouseUp += OverviewMouseUp;
            overviewPanel.MouseWheel += ImageView_MouseWheel;
            overviewPanel.MouseCaptureChanged += (_, _) =>
            {
                if (!overviewPanel.Capture)
                    overviewDragging = false;
            };
            viewport.Controls.Add(overviewPanel);
            Disposed += (_, _) =>
            {
                previewVersion++;
                overviewImage?.Dispose();
                overviewImage = null;
            };
        }

        private BioImage OverviewSelectedImage =>
            SelectedIndex >= 0 && SelectedIndex < Images.Count ? Images[SelectedIndex] : null;

        private void UpdateOverviewVisibility()
        {
            if (overviewPanel.IsDisposed)
                return;
            overviewPanel.Visible = ShowOverview && overviewImage != null &&
                overviewSource == OverviewSelectedImage;
            if (overviewPanel.Visible)
                overviewPanel.BringToFront();
        }

        private void SetOverviewPreview(Drawing.Image source, BioImage image)
        {
            int width = image.isPyramidal ? image.Resolutions[0].SizeX : source.Width;
            int height = image.isPyramidal ? image.Resolutions[0].SizeY : source.Height;
            double ratio = 160.0 / Math.Max(width, height);
            int previewWidth = Math.Max(1, (int)Math.Round(width * ratio));
            int previewHeight = Math.Max(1, (int)Math.Round(height * ratio));
            var thumbnail = new Drawing.Bitmap(previewWidth, previewHeight);
            using (var graphics = Drawing.Graphics.FromImage(thumbnail))
            {
                graphics.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                graphics.DrawImage(source, 0, 0, previewWidth, previewHeight);
            }
            overviewImage?.Dispose();
            overviewImage = thumbnail;
            overviewSource = image;
            overview = new AForge.Rectangle(8, 8, previewWidth, previewHeight);
            overviewPanel.SetBounds(overview.X, overview.Y, overview.Width, overview.Height);
            UpdateOverviewVisibility();
            overviewPanel.Invalidate();
        }

        private void UpdateRegularOverview()
        {
            BioImage image = OverviewSelectedImage;
            if (image == null || image.isPyramidal || SelectedIndex >= Bitmaps.Count)
                return;
            previewVersion++;
            previewRequestImage = null;
            SetOverviewPreview(Bitmaps[SelectedIndex], image);
        }

        private async void InitPreview()
        {
            BioImage image = OverviewSelectedImage;
            if (image == null || !image.isPyramidal)
                return;
            var coordinate = (image.Coordinate.Z, image.Coordinate.C, image.Coordinate.T);
            if (previewRequestImage == image && previewRequestCoordinate == coordinate)
                return;
            previewRequestImage = image;
            previewRequestCoordinate = coordinate;
            int version = ++previewVersion;
            UpdateOverviewVisibility();
            try
            {
                if (image.OpenSlideBase != null)
                {
                    // OpenSlideBase.GetTile returns a schema-sized tile, not an arbitrary
                    // region. Choose a level that retains enough pixels for the overview.
                    var slide = image.OpenSlideBase;
                    var levels = slide.Schema.Resolutions.OrderBy(r => r.Value.UnitsPerPixel).ToArray();
                    var levelInfo = levels.Where(r =>
                        Math.Max(slide.Schema.Extent.Width, slide.Schema.Extent.Height) /
                            r.Value.UnitsPerPixel >= 160).DefaultIfEmpty(levels[0]).Last();
                    int width = (int)Math.Ceiling(slide.Schema.Extent.Width / levelInfo.Value.UnitsPerPixel);
                    int height = (int)Math.Ceiling(slide.Schema.Extent.Height / levelInfo.Value.UnitsPerPixel);
                    if ((long)width * height > 16_000_000)
                        return;
                    using Drawing.Bitmap slidePreview = SlideOverviewBuilder.Build(width, height,
                        levelInfo.Value.TileWidth, levelInfo.Value.TileHeight,
                        (column, row) => slide.GetTile(new BruTile.TileInfo
                        {
                            Index = new BruTile.TileIndex(column, row, levelInfo.Key)
                        }));
                    if (version == previewVersion && !IsDisposed && !Disposing && image == OverviewSelectedImage)
                        SetOverviewPreview(slidePreview, image);
                    return;
                }
                // Use the smallest slide level, never the label or macro image.
                int level = image.Resolutions.Count - 1;
                var resolution = image.Resolutions[level];
                if ((long)resolution.SizeX * resolution.SizeY > 16_000_000)
                    return;
                int frame = image.GetFrameIndex(coordinate.Z, coordinate.C, coordinate.T);
                using Bitmap tile = await image.GetTile(frame, level, 0, 0, resolution.SizeX, resolution.SizeY);
                if (tile == null || version != previewVersion || IsDisposed || Disposing ||
                    image != OverviewSelectedImage)
                    return;
                using Drawing.Bitmap display = ToSysBitmap(tile);
                SetOverviewPreview(display, image);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Could not load slide overview: {ex.Message}");
            }
        }

        private RectangleD OverviewWorldBounds(BioImage image)
        {
            if (image.isPyramidal)
            {
                // The renderer addresses the tile schema's world coordinates, which
                // may use a different calibration from BioImage's metadata.
                var extent = image.OpenSlideBase?.Schema.Extent ?? image.SlideBase?.Schema.Extent;
                if (extent is { } schemaExtent)
                    return new RectangleD(schemaExtent.MinX, -schemaExtent.MaxY, schemaExtent.Width, schemaExtent.Height);
                double units = image.GetUnitPerPixel(0);
                return new RectangleD(0, 0, image.Resolutions[0].SizeX * units,
                    image.Resolutions[0].SizeY * units);
            }
            return new RectangleD(image.Volume.Location.X, image.Volume.Location.Y,
                image.Volume.Width, image.Volume.Height);
        }

        private void OverviewPaint(object sender, PaintEventArgs e)
        {
            BioImage image = OverviewSelectedImage;
            if (overviewImage == null || image == null || overviewSource != image)
                return;
            e.Graphics.DrawImageUnscaled(overviewImage, 0, 0);
            RectangleD bounds = OverviewWorldBounds(image);
            if (bounds.W <= 0 || bounds.H <= 0)
                return;
            // Use this viewer's transform even when a different tab is active.
            PointD topLeft = image.isPyramidal
                ? new PointD(image.PyramidalOrigin.X * image.Resolution, image.PyramidalOrigin.Y * image.Resolution)
                : new PointD(-ViewWidth / 2 / (PxWmicron * Scale.Width) - Origin.X,
                    -ViewHeight / 2 / (PxHmicron * Scale.Height) - Origin.Y);
            double viewWidth = image.isPyramidal ? ViewWidth * image.Resolution : ViewWidth / (PxWmicron * Scale.Width);
            double viewHeight = image.isPyramidal ? ViewHeight * image.Resolution : ViewHeight / (PxHmicron * Scale.Height);
            double left = Math.Clamp((topLeft.X - bounds.X) / bounds.W, 0, 1);
            double top = Math.Clamp((topLeft.Y - bounds.Y) / bounds.H, 0, 1);
            double right = Math.Clamp((topLeft.X + viewWidth - bounds.X) / bounds.W, 0, 1);
            double bottom = Math.Clamp((topLeft.Y + viewHeight - bounds.Y) / bounds.H, 0, 1);
            if (right > left && bottom > top)
            {
                using var pen = new Drawing.Pen(Drawing.Color.OrangeRed, 2);
                e.Graphics.DrawRectangle(pen, (float)(left * (overview.Width - 1)),
                    (float)(top * (overview.Height - 1)), (float)((right - left) * (overview.Width - 1)),
                    (float)((bottom - top) * (overview.Height - 1)));
            }
            e.Graphics.DrawRectangle(Drawing.Pens.White, 0, 0, overview.Width - 1, overview.Height - 1);
        }

        private void NavigateOverview(int x, int y)
        {
            BioImage image = OverviewSelectedImage;
            if (image == null || overviewImage == null || overviewSource != image)
                return;
            App.viewer = this;
            RectangleD bounds = OverviewWorldBounds(image);
            double targetX = bounds.X + Math.Clamp(x / (double)overview.Width, 0, 1) * bounds.W;
            double targetY = bounds.Y + Math.Clamp(y / (double)overview.Height, 0, 1) * bounds.H;
            if (image.isPyramidal)
            {
                PyramidalOrigin = new PointD(targetX / Resolution - ViewWidth / 2.0,
                    targetY / Resolution - ViewHeight / 2.0);
                UpdateImages();
            }
            else
                Origin = new PointD(-targetX, -targetY);
            UpdateView();
        }

        private MouseEventArgs OverviewCanvasEvent(MouseEventArgs e) =>
            new MouseEventArgs(e.Button, e.Clicks, e.X + overview.X, e.Y + overview.Y, e.Delta);

        private void OverviewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                overviewDragging = true;
                overviewPanel.Capture = true;
                NavigateOverview(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Middle)
                pictureBox_MouseDown(sender, OverviewCanvasEvent(e));
        }

        private void OverviewMouseMove(object sender, MouseEventArgs e)
        {
            if (overviewDragging && e.Button.HasFlag(MouseButtons.Left))
                NavigateOverview(e.X, e.Y);
            else if (e.Button == MouseButtons.Middle)
                rgbPictureBox_MouseMove(sender, OverviewCanvasEvent(e));
        }

        private void OverviewMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                overviewDragging = false;
                overviewPanel.Capture = false;
            }
            else if (e.Button == MouseButtons.Middle)
                pictureBox_MouseUp(sender, OverviewCanvasEvent(e));
        }
    }
}
