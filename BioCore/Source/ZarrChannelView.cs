using BioLib;
using AForge;
using ZarrNET.Core.Helpers;

namespace BioCore
{
    internal static class ZarrChannelView
    {
        internal static void InitializeChannels(BioImage image)
        {
            if (!image.IsZarrSource || image.levels == null || image.levels.Count == 0)
                return;
            var level = image.levels[0];
            int axis = Array.FindIndex(level.EffectiveAxes, a => string.Equals(a.Name, "c", StringComparison.OrdinalIgnoreCase));
            int count = axis >= 0 ? checked((int)level.Shape[axis]) : 1;
            int bits = level.DataType.Equals("uint16", StringComparison.OrdinalIgnoreCase) ? 16 : 8;
            while (image.Channels.Count < count)
                image.Channels.Add(new Channel(image.Channels.Count, bits, 1));
        }

        internal static async Task<byte[]> ReadRgbTileAsync(BioImage image, int levelIndex, int x, int y,
            int width, int height, AForge.ZCT coordinate, int[] channels)
        {
            var levels = image.Type == BioImage.ImageType.well && image.ZarrWellLevels.Count > 0
                ? image.ZarrWellLevels[Math.Clamp(image.WellIndex, 0, image.ZarrWellLevels.Count - 1)]
                : image.levels;
            var level = levels[levelIndex];
            int axisX = Array.FindIndex(level.EffectiveAxes, a => string.Equals(a.Name, "x", StringComparison.OrdinalIgnoreCase));
            int axisY = Array.FindIndex(level.EffectiveAxes, a => string.Equals(a.Name, "y", StringComparison.OrdinalIgnoreCase));
            if (axisX < 0 || axisY < 0)
                throw new InvalidDataException("The Zarr image has no X or Y axis.");
            int readWidth = Math.Min(width, checked((int)level.Shape[axisX]) - x);
            int readHeight = Math.Min(height, checked((int)level.Shape[axisY]) - y);
            var result = new byte[checked(width * height * 4)];
            if (readWidth <= 0 || readHeight <= 0)
                return result;
            bool sixteenBit = level.DataType.Equals("uint16", StringComparison.OrdinalIgnoreCase);
            if (!sixteenBit && !level.DataType.Equals("uint8", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Zarr RGB display does not support {level.DataType}.");
            for (int color = 0; color < 3; color++)
            {
                if (channels[color] < 0)
                    continue;
                var plane = await level.ReadTileAsync(x, y, readWidth, readHeight,
                    z: coordinate.Z, c: channels[color], t: coordinate.T).ConfigureAwait(false);
                int bytesPerSample = sixteenBit ? 2 : 1;
                if (plane?.Data == null || plane.Data.Length < checked(readWidth * readHeight * bytesPerSample))
                    throw new InvalidDataException("The Zarr channel tile is incomplete.");
                for (int row = 0; row < readHeight; row++)
                for (int col = 0; col < readWidth; col++)
                {
                    int source = (row * readWidth + col) * bytesPerSample;
                    result[(row * width + col) * 4 + color] = plane.Data[source + (sixteenBit ? 1 : 0)];
                }
            }
            for (int row = 0; row < readHeight; row++)
            for (int col = 0; col < readWidth; col++)
                result[(row * width + col) * 4 + 3] = 255;
            return result;
        }
    }
}
