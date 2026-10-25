using AForge;

namespace BioCore
{
    public partial class ImageView
    {
        // Normalized positions avoid integer overflow at high slide magnifications.
        private const int ScrollRange = 1_000_000;
        private bool updatingScrollBars;
        private double scrollLeft, scrollTop, scrollWidth, scrollHeight;

        private void UpdateScrollBars()
        {
            if (updatingScrollBars || hScrollBar == null || vScrollBar == null || viewport == null)
                return;
            BioImage image = OverviewSelectedImage;
            if (image == null || !image.isPyramidal || image.Resolution <= 0 ||
                !double.IsFinite(image.Resolution) || ViewWidth <= 0 || ViewHeight <= 0)
                return;

            RectangleD bounds = OverviewWorldBounds(image);
            double left = bounds.X / image.Resolution;
            double top = bounds.Y / image.Resolution;
            double width = bounds.W / image.Resolution;
            double height = bounds.H / image.Resolution;
            if (!double.IsFinite(left) || !double.IsFinite(top) || !double.IsFinite(width) ||
                !double.IsFinite(height) || width <= 0 || height <= 0)
                return;

            scrollLeft = left;
            scrollTop = top;
            scrollWidth = Math.Max(0, width - ViewWidth);
            scrollHeight = Math.Max(0, height - ViewHeight);
            updatingScrollBars = true;
            try
            {
                ConfigureScrollBar(hScrollBar, image.PyramidalOrigin.X, left, scrollWidth, width, ViewWidth);
                ConfigureScrollBar(vScrollBar, image.PyramidalOrigin.Y, top, scrollHeight, height, ViewHeight);
            }
            finally
            {
                updatingScrollBars = false;
            }
        }

        private static void ConfigureScrollBar(ScrollBar bar, double position, double start,
            double travel, double imageSize, int viewSize)
        {
            int page = (int)Math.Clamp(Math.Round(ScrollRange * Math.Min(1, viewSize / imageSize)), 1, ScrollRange);
            bar.Minimum = 0;
            bar.Maximum = ScrollRange;
            bar.LargeChange = page;
            bar.SmallChange = Math.Max(1, page / 10);
            int effectiveMaximum = bar.Maximum - bar.LargeChange + 1;
            bar.Enabled = travel > 0;
            double fraction = travel > 0 ? Math.Clamp((position - start) / travel, 0, 1) : 0;
            bar.Value = (int)Math.Round(fraction * effectiveMaximum);
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            BioImage image = OverviewSelectedImage;
            if (updatingScrollBars || image == null || !image.isPyramidal)
                return;
            App.viewer = this;
            PointD origin = image.PyramidalOrigin;
            if (sender == hScrollBar && scrollWidth > 0)
                origin.X = scrollLeft + scrollWidth * hScrollBar.Value / (hScrollBar.Maximum - hScrollBar.LargeChange + 1.0);
            else if (sender == vScrollBar && scrollHeight > 0)
                origin.Y = scrollTop + scrollHeight * vScrollBar.Value / (vScrollBar.Maximum - vScrollBar.LargeChange + 1.0);
            else
                return;
            image.PyramidalOrigin = origin;
            UpdateView();
        }
    }
}
