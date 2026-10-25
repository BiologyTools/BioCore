using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BioCore
{
    public partial class ImageView
    {
        private static void MaskRgbComponents(System.Drawing.Bitmap image, bool[] enabled)
        {
            if (enabled.All(value => value)) return;
            var data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var row = new byte[image.Width * 4];
                for (int y = 0; y < image.Height; y++)
                {
                    IntPtr address = IntPtr.Add(data.Scan0, y * data.Stride);
                    Marshal.Copy(address, row, 0, row.Length);
                    for (int x = 0; x < row.Length; x += 4)
                    {
                        if (!enabled[0]) row[x + 2] = 0;
                        if (!enabled[1]) row[x + 1] = 0;
                        if (!enabled[2]) row[x] = 0;
                    }
                    Marshal.Copy(row, 0, address, row.Length);
                }
            }
            finally { image.UnlockBits(data); }
        }
    }
}
