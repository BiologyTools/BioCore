using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BioCore
{
    internal static class SlideOverviewBuilder
    {
        internal static System.Drawing.Bitmap Build(int width, int height, int tileWidth, int tileHeight,
            Func<int, int, byte[]> readTile)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileWidth);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileHeight);
            ArgumentNullException.ThrowIfNull(readTile);
            var result = new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppPArgb);
            try
            {
                var pixels = result.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                try
                {
                    int tileStride = checked(tileWidth * 4);
                    for (int y = 0, row = 0; y < height; y += tileHeight, row++)
                    for (int x = 0, column = 0; x < width; x += tileWidth, column++)
                    {
                        byte[] bytes = readTile(column, row);
                        if (bytes == null || bytes.Length != checked(tileStride * tileHeight))
                            throw new InvalidDataException("The overview tile does not match the slide tile dimensions.");
                        int copyWidth = Math.Min(tileWidth, width - x);
                        int copyHeight = Math.Min(tileHeight, height - y);
                        // Tiles retain their full row stride even at the right and bottom edges.
                        for (int line = 0; line < copyHeight; line++)
                            Marshal.Copy(bytes, line * tileStride,
                                IntPtr.Add(pixels.Scan0, (y + line) * pixels.Stride + x * 4), copyWidth * 4);
                    }
                }
                finally
                {
                    result.UnlockBits(pixels);
                }
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
    }
}
