using BioLib;
using AForge;
using ZarrNET.Core.Helpers;
using ZarrNET.Core.Nodes;

namespace BioCore
{
    internal static class ZarrChannelView
    {
        private const int HistogramSampleSide = 1024;

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

        internal static async Task EnsureHistogramStatisticsAsync(BioImage image, ZCT coordinate)
        {
            if (!image.IsZarrSource || image.levels == null || image.levels.Count == 0)
            {
                await Task.Run(() => CalculateBufferedFrameStatistics(image, coordinate));
                return;
            }

            var levels = image.Type == BioImage.ImageType.well && image.ZarrWellLevels.Count > 0
                ? image.ZarrWellLevels[Math.Clamp(image.WellIndex, 0, image.ZarrWellLevels.Count - 1)]
                : image.levels;
            if (levels == null || levels.Count == 0)
                return;

            ResolutionLevelNode level = SelectHistogramLevel(levels);
            int axisX = Array.FindIndex(level.EffectiveAxes,
                a => string.Equals(a.Name, "x", StringComparison.OrdinalIgnoreCase));
            int axisY = Array.FindIndex(level.EffectiveAxes,
                a => string.Equals(a.Name, "y", StringComparison.OrdinalIgnoreCase));
            int axisC = Array.FindIndex(level.EffectiveAxes,
                a => string.Equals(a.Name, "c", StringComparison.OrdinalIgnoreCase));
            if (axisX < 0 || axisY < 0)
                return;

            int levelWidth = checked((int)level.Shape[axisX]);
            int levelHeight = checked((int)level.Shape[axisY]);
            int width = Math.Min(levelWidth, HistogramSampleSide);
            int height = Math.Min(levelHeight, HistogramSampleSide);
            int x = Math.Max(0, (levelWidth - width) / 2);
            int y = Math.Max(0, (levelHeight - height) / 2);
            int sourceChannels = axisC >= 0 ? checked((int)level.Shape[axisC]) : 1;
            bool sixteenBit = level.DataType.Equals("uint16", StringComparison.OrdinalIgnoreCase);
            if (!sixteenBit && !level.DataType.Equals("uint8", StringComparison.OrdinalIgnoreCase))
                return;

            int bits = sixteenBit ? 16 : 8;
            int stride = checked(width * (sixteenBit ? 2 : 1));
            AForge.PixelFormat pixelFormat = sixteenBit
                ? AForge.PixelFormat.Format16bppGrayScale
                : AForge.PixelFormat.Format8bppIndexed;
            var combined = new Statistics(bits);
            var sourceStatistics = new Statistics[sourceChannels];
            int z = ClampCoordinate(level, "z", coordinate.Z);
            int t = ClampCoordinate(level, "t", coordinate.T);

            for (int sourceChannel = 0; sourceChannel < sourceChannels; sourceChannel++)
            {
                var plane = await level.ReadTileAsync(x, y, width, height,
                    z: z, c: sourceChannel, t: t);
                if (plane?.Data == null || plane.Data.Length < checked(stride * height))
                    continue;

                Statistics[] planeStatistics = Statistics.FromBytes(plane.Data, width, height,
                    1, bits, stride, pixelFormat);
                if (planeStatistics == null || planeStatistics.Length == 0)
                    continue;

                var aggregate = new Statistics(bits);
                aggregate.AddStatistics(planeStatistics[0]);
                aggregate.MeanHistogram();
                sourceStatistics[sourceChannel] = aggregate;
                combined.AddStatistics(planeStatistics[0]);
            }

            if (sourceStatistics.Any(s => s == null))
                return;

            combined.MeanHistogram();
            for (int i = 0; i < image.Channels.Count; i++)
            {
                int sampleCount = Math.Max(1, image.Channels[i].range?.Length ?? 0);
                var statistics = new Statistics[sampleCount];
                for (int sample = 0; sample < sampleCount; sample++)
                {
                    int sourceChannel = sampleCount > 1 ? sample : i;
                    statistics[sample] = sourceStatistics[
                        Math.Clamp(sourceChannel, 0, sourceStatistics.Length - 1)];
                }
                image.Channels[i].stats = statistics;
                for (int sample = 0; sample < Math.Min(statistics.Length, image.Channels[i].range.Length); sample++)
                {
                    image.Channels[i].range[sample].Min = (int)statistics[sample].StackMin;
                    image.Channels[i].range[sample].Max = (int)statistics[sample].StackMax;
                }
            }
            image.Statistics = combined;
        }

        private static void CalculateBufferedFrameStatistics(BioImage image, ZCT coordinate)
        {
            if (image.Buffers.Count == 0 || image.Channels.Count == 0)
                return;

            var combined = new Statistics(image.bitsPerPixel);
            bool found = false;
            for (int channelIndex = 0; channelIndex < image.Channels.Count; channelIndex++)
            {
                int frameIndex = image.GetFrameIndex(
                    Math.Clamp(coordinate.Z, 0, Math.Max(0, image.SizeZ - 1)),
                    Math.Clamp(channelIndex, 0, Math.Max(0, image.SizeC - 1)),
                    Math.Clamp(coordinate.T, 0, Math.Max(0, image.SizeT - 1)));
                if (frameIndex < 0 || frameIndex >= image.Buffers.Count || image.Buffers[frameIndex] == null)
                    continue;

                var buffer = image.Buffers[frameIndex];
                buffer.Stats ??= Statistics.FromBytes(buffer);
                if (buffer.Stats == null || buffer.Stats.Length == 0)
                    continue;

                int sampleCount = Math.Max(1, image.Channels[channelIndex].range?.Length ?? 0);
                var statistics = new Statistics[sampleCount];
                for (int sample = 0; sample < sampleCount; sample++)
                {
                    Statistics raw = buffer.Stats[Math.Clamp(sample, 0, buffer.Stats.Length - 1)];
                    var aggregate = new Statistics(image.bitsPerPixel);
                    aggregate.AddStatistics(raw);
                    aggregate.MeanHistogram();
                    statistics[sample] = aggregate;
                    combined.AddStatistics(raw);
                    image.Channels[channelIndex].range[sample].Min = (int)aggregate.StackMin;
                    image.Channels[channelIndex].range[sample].Max = (int)aggregate.StackMax;
                    found = true;
                }
                image.Channels[channelIndex].stats = statistics;
            }
            if (found)
            {
                combined.MeanHistogram();
                image.Statistics = combined;
            }
        }

        internal static bool HasHistogramStatistics(BioImage image)
        {
            return image?.Statistics?.StackValues != null &&
                image.Channels.Count > 0 &&
                image.Channels.All(channel => channel.range?.Length > 0 &&
                    channel.stats != null && channel.stats.Length >= channel.range.Length &&
                    channel.stats.All(stat => stat?.StackValues != null));
        }

        private static ResolutionLevelNode SelectHistogramLevel(IReadOnlyList<ResolutionLevelNode> levels)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                int axisX = Array.FindIndex(level.EffectiveAxes,
                    a => string.Equals(a.Name, "x", StringComparison.OrdinalIgnoreCase));
                int axisY = Array.FindIndex(level.EffectiveAxes,
                    a => string.Equals(a.Name, "y", StringComparison.OrdinalIgnoreCase));
                if (axisX >= 0 && axisY >= 0 &&
                    level.Shape[axisX] <= HistogramSampleSide &&
                    level.Shape[axisY] <= HistogramSampleSide)
                    return level;
            }
            return levels[^1];
        }

        private static int ClampCoordinate(ResolutionLevelNode level, string axisName, int value)
        {
            int axis = Array.FindIndex(level.EffectiveAxes,
                a => string.Equals(a.Name, axisName, StringComparison.OrdinalIgnoreCase));
            return axis < 0 ? 0 : Math.Clamp(value, 0, checked((int)level.Shape[axis]) - 1);
        }
    }
}
